using Assets.Scripts.ConfigHandler;
using Assets.Scripts.HeightMap;
using Assets.Scripts.OpenStreetMap;
using Assets.Scripts.SceneObjects;
using OsmCityEngine.OpenStreetMap;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class CityConstructor : Singleton<CityConstructor>, ICreateMe
{
    public CityScene city;
    public HeightmapLoader heightMap;
    private OsmLoader osm;
    private bool startedTerrain = false;
    private OsmConnector osmConnector;
    private OSMXml osmxml;

    public CityScene scene => city;

    public void CreateScene(float lat, float lng, float size, string preset = "default")
    {
        var go = new GameObject("City");
        city = go.AddComponent<CityScene>();

        // default settings
        if(size == 0)
        {
            size = 1;
        }
        var radius = 500f* size;
        if(size > 1)
        {
            RenderSettings.fogDensity = RenderSettings.fogDensity/size;

        }

      
        var geo = new Geography();
        var mercator = Geography.LatLontoMeters(lat, lng);
        var lbgeo_lat_lon = geo.meterstoLatLon(mercator[0] - radius, mercator[1] - radius);
        var rtgeo = geo.meterstoLatLon(mercator[0] + radius, mercator[1] + radius);

        // get bbox

        var bbox = new BBox()
        {
            bottom = (float)lbgeo_lat_lon[0],
            left = (float)lbgeo_lat_lon[1],
            top = (float)rtgeo[0],
            right = (float)rtgeo[1],
            meterBottom = (float)(mercator[0] - radius),
            meterLeft = (float)(mercator[1] - radius),
            meterTop = (float)(mercator[0] + radius),
            meterRight = (float)(mercator[1] + radius),
        };


        city.lat = lat;
        city.lng = lng;
        city.scenebbox = bbox;
        city.centerMercator = new Vector2((float)mercator[0], (float)mercator[1]);
        city.centerScene = new Vector2(radius, radius);

        InitialConfigLoader configloader = new InitialConfigLoader();
        city.config = configloader.loadInitialConfig();

        GamePlayManager.Instance.ChangeConstructionStatus("Loading relief data...");
        heightMap = new HeightmapLoader(city.scenebbox);

    }
    private void OnEnable()
    {
        osmConnector = OsmConnector.Instance;
        osmConnector.OnLoaded -= OnOsmLayerLoaded;
        osmConnector.OnLoaded += OnOsmLayerLoaded;
    }

    private void OnDisable()
    {
        if (osmConnector != null)
        {
            OsmConnector.Instance.OnLoaded -= OnOsmLayerLoaded;
        }
    }

    internal void CleanCityData()
    {
        var go = city.gameObject;

        city = null;
        heightMap = null;

        go.SetActive(false);
        Destroy(go, 0.1f);


    }

    public void OnHeightMapLoaded()
    {
        CreateTerrain();
    }
    internal void OnTexturesLoaded()
    {
        city.terrain.UpdateTexture();
    }

    public void OnOsmLayerLoaded(string id, string json)
    {
        if (id.StartsWith(OsmLoader.BUILDINGS_PREFIX))
        {
            GamePlayManager.Instance.ChangeConstructionStatus($"Build building...");
        }

        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        OSMparser parser = new OSMparser();
        osmxml = parser.parseOSMFromJson(json);
        assignNodePositions();
        stopwatch.Stop();
        Debug.Log("<color=blue>OSM PARSING TIME:</color>" + stopwatch.ElapsedMilliseconds);
        stopwatch.Reset();
        stopwatch.Start();

        if (osmxml.defaultobject3DList.Count > 0)
        {
            city.defaultObject3DList.AddRange(DefaultObject3DHandler.drawDefaultObjects(osmxml.defaultobject3DList));

        }

        stopwatch.Stop();
        Debug.Log("<color=blue>3D OBJECT RENDER TIME:</color>" + stopwatch.ElapsedMilliseconds);
        stopwatch.Reset();
        stopwatch.Start();

        List<Way> WayListforHighway = new List<Way>();
        List<Way> WayListforBuilding = new List<Way>();

        var config = city.config;
        for (int k = 0; k < osmxml.wayList.Count; k++)
        {
            Way w = osmxml.wayList[k];

            switch (w.type)
            {
                case ItemEnumerator.wayType.building:
                    WayListforBuilding.Add(w);
                    break;
                case ItemEnumerator.wayType.highway:
                    WayListforHighway.Add(w);
                    break;
                case ItemEnumerator.wayType.area:
                    break;
                case ItemEnumerator.wayType.barrier:
                    city.barrierList.Add(new Barrier(w, config.barrierConfig));
                    break;
                case ItemEnumerator.wayType.river:
                    city.highwayList.Add(new Highway(w, config.highwayConfig, city.terrain));
                    break;
                case ItemEnumerator.wayType.none:
                    break;
            }
        }

        stopwatch.Stop();
        Debug.Log("<color=blue>ITEM ENUMERATING TIME:</color>" + stopwatch.ElapsedMilliseconds);
        stopwatch.Reset();
        if (id.StartsWith(OsmLoader.STREETS_PREFIX))// || id.StartsWith(OsmLoader.GREEN_PREFIX))
        {
            GamePlayManager.Instance.ChangeConstructionStatus($"Build Road...");

            Debug.Log("<color=blue>HIGHWAY RENDERING:</color> Start rendering");

            stopwatch.Start();

            var highwayModeller = new HighwayModeller(WayListforHighway, city.terrain, city.config.highwayConfig);
            city.highwayModeller = highwayModeller;
            highwayModeller.renderHighwayList();
            highwayModeller.renderPavementList();
            city.highwayList.AddRange(highwayModeller.highwayList);
            city.pavementList.AddRange(highwayModeller.pavementList);

            stopwatch.Stop();
            Debug.Log("<color=blue>HIGHWAY RENDERING TIME:</color>" + stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
            GamePlayManager.Instance.ChangeConstructionStatus($"Road built...");

        }
        if (id.StartsWith(OsmLoader.BUILDINGS_PREFIX))
        {
            if (WayListforBuilding.Any())
            {
                Debug.Log("<color=blue>BUILDING :</color> Start rendering" );
                stopwatch.Start();

                BuildingListModeller buildingListModeller = new BuildingListModeller(WayListforBuilding, osmxml.buildingRelations, config.buildingConfig);
                buildingListModeller.renderBuildingList();
                city.buildingList.AddRange(buildingListModeller.buildingList);

                stopwatch.Stop();

            }
        }

        if (id.StartsWith(OsmLoader.BUILDINGS_PREFIX)){
            GamePlayManager.Instance.ChangeConstructionStatus($"Building built...");
        }

        if (osmConnector.queue.Count == 0){
            GamePlayManager.Instance.ChangeConstructionStatus($"all data built...");
        }


        Debug.Log("<color=blue>BUILDING RENDERING TIME:</color>" + stopwatch.ElapsedMilliseconds);

        Debug.Log("<color=red>Scene Info:</color> BuildingCount:" + city.buildingList.Count.ToString() + " HighwayCount:" + city.highwayList.Count);

    }

    public void CreateTerrain()
    {
        if (startedTerrain) return;
        startedTerrain = true;

        var provider = //MapProvider.OpenStreetMap;
             MapProvider.BingMapAerial;

        // set terrain heights and loading textures
        city.terrain = new myTerrain(heightMap, city.scenebbox, "123", provider);
        GamePlayManager.Instance.OnTerrainCreated();

        GamePlayManager.Instance.ChangeConstructionStatus("Loading textures data...");

        // after terrain created bbox can be changed
        OsmConnector.Instance.OnLoaded = OnOsmLayerLoaded;
        osm = new OsmLoader(city.terrain.terrainInfo.terrainBBox, null);

    }
    public Vector3 CenterPointOnTerrain()
    {
        var pos = new Vector3(city.centerScene.x, city.terrain.maxHeight + 2f, city.centerScene.y);
        if (Physics.Raycast(pos, Vector3.down, out RaycastHit hit, 10000f))
        {
            return hit.point + Vector3.up * 2;
        }
        else
        {
            return pos;
        }

        //return new Vector3(city.centerScene.x, city.terrain.maxHeight + 2f, city.centerScene.y);
    }

    private void assignNodePositions()
    {
        var terrain = city.terrain;
        var scenebbox = city.scenebbox;

        for (int k = 0; k < osmxml.nodeList.Count; k++)
        {
            Node nd = osmxml.nodeList[k];
            Vector2 meterCoord = Geography.LatLontoMeters(nd.lat, nd.lon);
            Vector2 shift = new Vector2(scenebbox.meterBottom, scenebbox.meterLeft);
            meterCoord = meterCoord - shift;
            float height = terrain.getTerrainHeight(nd.lat, nd.lon);
            nd.meterPosition = new Vector3(meterCoord.y, height, meterCoord.x);
            osmxml.nodeList[k] = nd;
        }

        for (int i = 0; i < osmxml.wayList.Count; i++)
        {
            for (int k = 0; k < osmxml.wayList[i].nodes.Count; k++)
            {
                Node nd = osmxml.wayList[i].nodes[k];
                Vector2 meterCoord = Geography.LatLontoMeters(nd.lat, nd.lon);
                Vector2 shift = new Vector2(scenebbox.meterBottom, scenebbox.meterLeft);
                meterCoord = meterCoord - shift;
                float height = terrain.getTerrainHeight(nd.lat, nd.lon);
                nd.meterPosition = new Vector3(meterCoord.y, height, meterCoord.x);
                osmxml.wayList[i].nodes[k] = nd;
            }


        }

        for (int i = 0; i < osmxml.defaultobject3DList.Count; i++)
        {
            Node nd = osmxml.defaultobject3DList[i];
            Vector2 meterCoord = Geography.LatLontoMeters(nd.lat, nd.lon);
            Vector2 shift = new Vector2(scenebbox.meterBottom, scenebbox.meterLeft);
            meterCoord = meterCoord - shift;
            float height = terrain.getTerrainHeight(nd.lat, nd.lon);
            nd.meterPosition = new Vector3(meterCoord.y, height, meterCoord.x);
            osmxml.defaultobject3DList[i] = nd;
        }

    }
}
