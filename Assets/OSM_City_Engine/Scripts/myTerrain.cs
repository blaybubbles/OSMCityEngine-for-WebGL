﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.HeightMap;
using Assets.Scripts.OpenStreetMap;

namespace Assets.Scripts.SceneObjects
{
    public struct TerrainInfo
    {
        public int leftIndex;
        public int rightIndex;
        public int topIndex;
        public int bottomIndex;

        public BBox terrainBBox;

        public float[,] terrainHeights;

        /// <summary>
        /// x = meterX(lat) , y = meterZ(lon)
        /// </summary>
        public Vector2[,] meterPositions;

        /// <summary>
        /// scenebbox.meterLeft;
        /// </summary>
        public float shiftX;

        /// <summary>
        /// scenebbox.meterBottom;
        /// </summary>
        public float shiftZ;

        public int ColumnCount, RowCount;

        public List<GridInfo> grids;


    }

    public class GridInfo
    {
        internal BBox bbox;
        internal string name;
        internal int indexX;
        internal int indexY;
    }

    public class myTerrain
    {

        HeightmapLoader heightmap;
        public BBox scenebbox;
        public TerrainInfo terrainInfo;
        //private string OSMfileName;

        public GameObject terrainObject;
        public List<GameObject> gridList;
        public MapProvider textureType;
        internal TerrainTextureHandler texturehandler;
        public float minHeight;
        public float maxHeight;

        public myTerrain(HeightmapLoader _heightmap, BBox _bbox, string _OSMfileName, MapProvider _provider)
        {
            //OSMfileName = _OSMfileName;     
            heightmap = _heightmap;
            scenebbox = _bbox;
            textureType = _provider;

            terrainObject = new GameObject("Terrain");
            terrainObject.transform.SetParent(CityConstructor.Instance.city.transform);
            gridList = new List<GameObject>();

            int leftIndex = (int)Math.Floor((scenebbox.left - (float)Math.Floor(scenebbox.left)) * 1200.0f);
            int rightIndex = (int)Math.Ceiling((scenebbox.right - (float)Math.Floor(scenebbox.right)) * 1200.0f);

            if ((rightIndex - leftIndex) % 2 != 0)
                rightIndex++;

            int topIndex, bottomIndex;


            topIndex = (int)Math.Floor(((float)Math.Ceiling(scenebbox.top) - scenebbox.top) * 1200.0f);
            bottomIndex = (int)Math.Ceiling(((float)Math.Ceiling(scenebbox.bottom) - scenebbox.bottom) * 1200.0f);

            if ((bottomIndex - topIndex) % 2 != 0)
                topIndex -= 1;

            Debug.Log("<color=yellow>TERRAIN:</color>" + "left:" + leftIndex + " right:" + rightIndex
                       + " bottom:" + bottomIndex + " top:" + topIndex);


            float[,] myTerrainHeights = new float[1 + bottomIndex - topIndex, 1 + rightIndex - leftIndex];
            Vector2[,] meterPositions = new Vector2[1 + bottomIndex - topIndex, 1 + rightIndex - leftIndex];
            Geography geo = new Geography();

            float left = (float)Math.Floor(scenebbox.left) + (leftIndex / 1200.0f);
            float right = (float)Math.Floor(scenebbox.left) + (rightIndex / 1200.0f);
            float top = (float)Math.Ceiling(scenebbox.top) - (topIndex / 1200.0f);
            float bottom = (float)Math.Ceiling(scenebbox.top) - (bottomIndex / 1200.0f);

            minHeight = float.MaxValue;
            maxHeight = float.MinValue;

            for (int i = 0; i <= bottomIndex - topIndex; i++)
            {
                for (int j = 0; j <= rightIndex - leftIndex; j++)
                {
                    myTerrainHeights[i, j] = heightmap.heightmap[topIndex + i, leftIndex + j];
                    meterPositions[i, j] = Geography.LatLontoMeters(top - (i / 1200.0f), left + (j / 1200.0f));
                    minHeight = Mathf.Min(minHeight, myTerrainHeights[i, j]);
                    maxHeight = Mathf.Max(maxHeight, myTerrainHeights[i, j]);
                }
            }


            terrainInfo.leftIndex = leftIndex;
            terrainInfo.rightIndex = rightIndex;
            terrainInfo.bottomIndex = bottomIndex;
            terrainInfo.topIndex = topIndex;
            terrainInfo.terrainHeights = myTerrainHeights;
            terrainInfo.meterPositions = meterPositions;
            terrainInfo.ColumnCount = 1 + rightIndex - leftIndex;
            terrainInfo.RowCount = 1 + bottomIndex - topIndex;

            terrainInfo.terrainBBox = new BBox();
            terrainInfo.terrainBBox.left = left;
            terrainInfo.terrainBBox.top = top;
            terrainInfo.terrainBBox.bottom = bottom;
            terrainInfo.terrainBBox.right = right;
            Vector2 bottmleft = Geography.LatLontoMeters(bottom, left);
            Vector2 topright = Geography.LatLontoMeters(top, right);
            terrainInfo.terrainBBox.meterBottom = bottmleft.x;
            terrainInfo.terrainBBox.meterLeft = bottmleft.y;
            terrainInfo.terrainBBox.meterTop = topright.x;
            terrainInfo.terrainBBox.meterRight = topright.y;

            terrainInfo.shiftX = scenebbox.meterLeft;
            terrainInfo.shiftZ = scenebbox.meterBottom;
            terrainInfo.grids = new List<GridInfo>();

            Debug.Log("<color=yellow>TERRAIN:</color> ColumnCount:" + terrainInfo.ColumnCount + " RowCount:" + terrainInfo.RowCount);


            texturehandler = new TerrainTextureHandler();
            // drawBoundsforDebug();

            //CreateGrids();
            CreateTerrain();
        }

        public void CreateTerrain()
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            //yield return texturehandler.LoadAllTextures(terrainInfo.terrainBBox, textureType);

            for (int i = 0; i < terrainInfo.RowCount - 1; i += 2)
            {
                for (int j = 0; j < terrainInfo.ColumnCount - 1; j += 2)
                    createGrid(i, j);
            }

            drawUnderPlates();

            stopwatch.Stop();
            Debug.Log("<color=blue>TERRAIN RENDER TIME:</color>" + stopwatch.ElapsedMilliseconds);
        }

        //Creates 2x2 subgrid of Terrain
        private void createGrid(int i, int j)
        {

            GridInfo info = new GridInfo();
            terrainInfo.grids.Add(info);

            BBox bbox = new BBox();
            bbox.meterLeft = terrainInfo.meterPositions[i, j].y;
            bbox.meterTop = terrainInfo.meterPositions[i, j].x;
            bbox.meterRight = terrainInfo.meterPositions[i, j + 2].y;
            bbox.meterBottom = terrainInfo.meterPositions[i + 2, j].x;

            info.bbox = bbox;
            info.indexX = i;
            info.indexY = j;
            Debug.Log("<color=yellow>INDEX</color> i:" + i + " j:" + j);
            Debug.Log("<color=red>Grid BBOX:</color> left:" + bbox.meterLeft + " right:" + bbox.meterRight +
                     " bottom:" + bbox.meterBottom + " top:" + bbox.meterTop);

            string gridname = "Grid" + i + "_" + j;
            info.name = gridname;

            GameObject grid = new GameObject(gridname, typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider));
            grid.transform.parent = terrainObject.transform;
            grid.tag = "Terrain";
            //grid.AddComponent<TerrainMouseHandler>();

            Mesh mesh = new Mesh();

            Vector3[] Vertices = new Vector3[9];

            for (int m = i, itr = 0; m <= i + 2; m++)
            {
                for (int n = j; n <= j + 2; n++)
                {
                    Vertices[itr] = new Vector3(terrainInfo.meterPositions[m, n].y - terrainInfo.shiftX,
                                               terrainInfo.terrainHeights[m, n],
                                               terrainInfo.meterPositions[m, n].x - terrainInfo.shiftZ);
                    itr++;
                }
            }


            int[] Triangles = new int[] { 0, 1, 3, 1, 4, 3, //Quad1
                                          1, 2, 4, 2, 5, 4, //Quad2
                                          3, 4, 6, 4, 7, 6, //Quad3
                                          4, 5, 7, 5, 8, 7  //Quad4
                                         };

            Vector2[] UVCoords = new Vector2[] {
                                                new Vector2(0.0f,1.0f), new Vector2(0.5f,1.0f), new Vector2(1.0f,1.0f),
                                                new Vector2(0.0f,0.5f), new Vector2(0.5f,0.5f), new Vector2(1.0f,0.5f),
                                                new Vector2(0.0f,0.0f), new Vector2(0.5f,0.0f), new Vector2(1.0f,0.0f)
                                               };


            Vector3[] Normals = calculateNormals(i, j);


            mesh.vertices = Vertices;
            mesh.triangles = Triangles;
            mesh.uv = UVCoords;
            mesh.normals = Normals;


            MeshFilter meshfilter = grid.GetComponent<MeshFilter>();
            meshfilter.mesh = mesh;

            Material matTerrain = (Material)Resources.Load("Materials/Terrain/Mat_Terrain", typeof(Material));
            Material mat = new Material(matTerrain);
            //mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;



            mat.mainTexture = texturehandler.generateTexture(textureType, bbox, i, j, "test");//OSMfileName);
            MeshRenderer meshrenderer = grid.GetComponent<MeshRenderer>();
            meshrenderer.material = mat;

            MeshCollider meshCollider = grid.GetComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;

            gridList.Add(grid);
        }

        //Get the height of terrain given lat/lon value
        public float getTerrainHeight(float lat, float lon)
        {
            return HighPrecisionTerrainHeight(lat, lon, true);
        }

        //Get the height of terrain given meter coordinate
        public float getTerrainHeight2(float meterx, float meterz)
        {
            return HighPrecisionTerrainHeight(meterx, meterz, false);
        }


        /// <summary>
        /// interpolates triangle for obtain inner height value in triangle
        /// </summary>
        [Obsolete("This method is deprecated, please use HighPrecisionTerrainHeight instead.")]
        private float getTerrainTriangleHeight(float meterx, float meterz)
        {
            float finalvalue = 0.0f;

            Geography proj = new Geography();
            Vector2 c = proj.meterstoLatLon(meterx, meterz);

            float ratioX = (float)Math.Ceiling(scenebbox.top) - c.x;
            float ratioZ = c.y - (float)Math.Floor(scenebbox.left);

            float Xindex = (1200.0f * ratioX);
            float Zindex = (1200.0f * ratioZ);

            //Upper Triangle
            if ((Xindex - Math.Floor(Xindex)) + (Zindex - Math.Floor(Zindex)) < 1.0f)
            {
                int n = heightmap.heightmap[(int)Math.Floor(Xindex), (int)Math.Ceiling(Zindex)];
                int m = heightmap.heightmap[(int)Math.Floor(Xindex), (int)Math.Floor(Zindex)];
                int k = heightmap.heightmap[(int)Math.Ceiling(Xindex), (int)Math.Floor(Zindex)];
                float interpolationUp = m + (n - m) * (Zindex - (float)Math.Floor(Zindex));
                float interpolationHypothenus = k + (n - k) * (Zindex - (float)Math.Floor(Zindex));

                float a = Xindex - (float)Math.Floor(Xindex);
                float aplusb = (float)Math.Ceiling(Zindex) - Zindex;
                finalvalue = interpolationUp + (interpolationHypothenus - interpolationUp) * (a / aplusb);

            }

            //Lower Triangle
            else if ((Xindex - Math.Floor(Xindex)) + (Zindex - Math.Floor(Zindex)) >= 1.0f)
            {

                int n = heightmap.heightmap[(int)Math.Ceiling(Xindex), (int)Math.Ceiling(Zindex)];
                int m = heightmap.heightmap[(int)Math.Ceiling(Xindex), (int)Math.Floor(Zindex)];
                int k = heightmap.heightmap[(int)Math.Floor(Xindex), (int)Math.Ceiling(Zindex)];
                float interpolationDown = m + (n - m) * (Zindex - (float)Math.Floor(Zindex));
                float interpolationHypothenus = m + (k - m) * (Zindex - (float)Math.Floor(Zindex));

                float a = (float)Math.Ceiling(Xindex) - Xindex;
                float aplusb = Zindex - (float)Math.Floor(Zindex);
                finalvalue = interpolationDown + (interpolationHypothenus - interpolationDown) * (a / aplusb);

                if ((Xindex - Math.Floor(Xindex)) + (Zindex - Math.Floor(Zindex)) > 0.98f && (Xindex - Math.Floor(Xindex)) + (Zindex - Math.Floor(Zindex)) < 1.02f)
                    return interpolationHypothenus;

            }

            float final2 = HighPrecisionTerrainHeight(meterx, meterz, false);

            if (final2 - finalvalue != 0.0f)
                Debug.Log("<color=red>PRECISION ERROR VAR</color>" + (final2 - finalvalue).ToString("0.000"));

            return final2;



        }

        private float HighPrecisionTerrainHeight(float meterx, float meterz, bool isLatlon)
        {
            double finalvalue = 0.0;

            Vector2 c;
            if (!isLatlon)
            {
                Geography proj = new Geography();
                c = proj.meterstoLatLonDouble(meterx, meterz);
            }
            else
            {
                c = new Vector2(meterx, meterz);
            }

            double ratioX = Math.Ceiling(scenebbox.top) - c.x;
            double ratioZ = c.y - Math.Floor(scenebbox.left);

            double Xindex = (1200.0 * ratioX);
            double Zindex = (1200.0 * ratioZ);

            try
            {
                //Upper Triangle
                if ((Xindex - Math.Floor(Xindex)) + (Zindex - Math.Floor(Zindex)) < 1.0f)
                {
                    int n = heightmap.heightmap[(int)Math.Floor(Xindex), (int)Math.Ceiling(Zindex)];
                    int m = heightmap.heightmap[(int)Math.Floor(Xindex), (int)Math.Floor(Zindex)];
                    int k = heightmap.heightmap[(int)Math.Ceiling(Xindex), (int)Math.Floor(Zindex)];
                    double interpolationUp = m + (n - m) * (Zindex - Math.Floor(Zindex));
                    double interpolationHypothenus = k + (n - k) * (Zindex - Math.Floor(Zindex));

                    double a = Xindex - Math.Floor(Xindex);
                    double aplusb = Math.Ceiling(Zindex) - Zindex;
                    finalvalue = interpolationUp + (interpolationHypothenus - interpolationUp) * (a / aplusb);

                }

                //Lower Triangle
                else if ((Xindex - Math.Floor(Xindex)) + (Zindex - Math.Floor(Zindex)) >= 1.0f)
                {

                    int n = heightmap.heightmap[(int)Math.Ceiling(Xindex), (int)Math.Ceiling(Zindex)];
                    int m = heightmap.heightmap[(int)Math.Ceiling(Xindex), (int)Math.Floor(Zindex)];
                    int k = heightmap.heightmap[(int)Math.Floor(Xindex), (int)Math.Ceiling(Zindex)];
                    double interpolationDown = m + (n - m) * (Zindex - Math.Floor(Zindex));
                    double interpolationHypothenus = m + (k - m) * (Zindex - Math.Floor(Zindex));

                    double a = Math.Ceiling(Xindex) - Xindex;
                    double aplusb = Zindex - Math.Floor(Zindex);
                    finalvalue = interpolationDown + (interpolationHypothenus - interpolationDown) * (a / aplusb);

                    if ((Xindex - Math.Floor(Xindex)) + (Zindex - Math.Floor(Zindex)) > 0.99 && (Xindex - Math.Floor(Xindex)) + (Zindex - Math.Floor(Zindex)) < 1.01)
                        return (float)interpolationHypothenus;

                }
            }
            catch (Exception ex)
            {
                if(Xindex < 0 )
                {
                    Xindex = 0;
                }
                if (Zindex < 0 )
                    Zindex = 0;
                if (heightmap.heightmap.GetLength(0) < Math.Floor(Xindex) && heightmap.heightmap.GetLength(1) < Math.Floor(Zindex))
                {
                    finalvalue = heightmap.heightmap[(int)Math.Floor(Xindex), (int)Math.Floor(Zindex)];
                }
                
            }
            

            return (float)finalvalue;
        }

        private Vector3[] calculateNormals(int i, int j)
        {
            Vector3[] normals = new Vector3[9];

            for (int m = i, itr = 0; m <= i + 2; m++)
            {
                for (int n = j; n <= j + 2; n++)
                {
                    // read neightbor heights using an arbitrary small offset
                    Vector3 off = new Vector3(5.0f, 5.0f, 5.0f);
                    float hL = getTerrainHeight2(terrainInfo.meterPositions[m, n].x - off.x, terrainInfo.meterPositions[m, n].y);
                    float hR = getTerrainHeight2(terrainInfo.meterPositions[m, n].x + off.x, terrainInfo.meterPositions[m, n].y);
                    float hD = getTerrainHeight2(terrainInfo.meterPositions[m, n].x, terrainInfo.meterPositions[m, n].y + off.z);
                    float hU = getTerrainHeight2(terrainInfo.meterPositions[m, n].x, terrainInfo.meterPositions[m, n].y - off.z);

                    // deduce terrain normal
                    Vector3 N = new Vector3();
                    N.z = hL - hR;
                    N.y = 2.0f;
                    N.x = hU - hD;
                    N = Vector3.Normalize(N);
                    normals[itr++] = N;
                }
            }

            return normals;

        }

        private void drawBoundsforDebug()
        {

            for (int i = 0; i < terrainInfo.RowCount; i++)
            {
                GameObject debugLine = new GameObject("debugLineHor" + i, typeof(LineRenderer));
                LineRenderer linerender = debugLine.GetComponent<LineRenderer>();
                linerender.SetWidth(0.3f, 0.3f);
                linerender.SetColors(Color.blue, Color.blue);
                linerender.SetVertexCount(terrainInfo.ColumnCount);
                int ctr = 0;

                for (int j = 0; j < terrainInfo.ColumnCount; j++)
                {
                    Vector3 vec = new Vector3(terrainInfo.meterPositions[i, j].y, terrainInfo.terrainHeights[i, j], terrainInfo.meterPositions[i, j].x);
                    linerender.SetPosition(ctr++, vec - new Vector3(terrainInfo.shiftX, 0, terrainInfo.shiftZ));
                }
            }

            for (int i = 0; i < terrainInfo.ColumnCount; i++)
            {
                GameObject debugLine = new GameObject("debugLineVert" + i, typeof(LineRenderer));
                LineRenderer linerender = debugLine.GetComponent<LineRenderer>();
                linerender.SetWidth(1, 1);
                linerender.SetColors(Color.blue, Color.blue);
                linerender.SetVertexCount(terrainInfo.RowCount);
                int ctr = 0;

                for (int j = 0; j < terrainInfo.RowCount; j++)
                {
                    Vector3 vec = new Vector3(terrainInfo.meterPositions[j, i].y, terrainInfo.terrainHeights[j, i], terrainInfo.meterPositions[j, i].x);
                    linerender.SetPosition(ctr++, vec - new Vector3(terrainInfo.shiftX, 0, terrainInfo.shiftZ));
                }
            }



        }

        private void drawUnderPlates()
        {

            Mesh mesh = new Mesh();

            int length1 = terrainInfo.meterPositions.GetLength(0);
            int length2 = terrainInfo.meterPositions.GetLength(1);

            Vector3[] vertices = new Vector3[length1 * 4 + length2 * 4];
            int iterator = 0;


            //LEFT WALL
            for (int i = 0; i < terrainInfo.meterPositions.GetLength(0); i++)
            {
                vertices[iterator++] = new Vector3(terrainInfo.meterPositions[i, 0].y - terrainInfo.shiftX,
                                            terrainInfo.terrainHeights[i, 0],
                                            terrainInfo.meterPositions[i, 0].x - terrainInfo.shiftZ);
            }

            for (int i = 0; i < terrainInfo.meterPositions.GetLength(0); i++)
            {
                vertices[iterator++] = new Vector3(terrainInfo.meterPositions[i, 0].y - terrainInfo.shiftX,
                                            0,
                                            terrainInfo.meterPositions[i, 0].x - terrainInfo.shiftZ);
            }

            //RIGHT WALL
            for (int i = 0; i < terrainInfo.meterPositions.GetLength(0); i++)
            {
                vertices[iterator++] = new Vector3(terrainInfo.meterPositions[i, length2 - 1].y - terrainInfo.shiftX,
                                            terrainInfo.terrainHeights[i, length2 - 1],
                                            terrainInfo.meterPositions[i, length2 - 1].x - terrainInfo.shiftZ);
            }

            for (int i = 0; i < terrainInfo.meterPositions.GetLength(0); i++)
            {
                vertices[iterator++] = new Vector3(terrainInfo.meterPositions[i, length2 - 1].y - terrainInfo.shiftX,
                                            0,
                                            terrainInfo.meterPositions[i, length2 - 1].x - terrainInfo.shiftZ);
            }


            //TOP WALL
            for (int i = 0; i < terrainInfo.meterPositions.GetLength(1); i++)
            {
                vertices[iterator++] = new Vector3(terrainInfo.meterPositions[0, i].y - terrainInfo.shiftX,
                                            terrainInfo.terrainHeights[0, i],
                                            terrainInfo.meterPositions[0, i].x - terrainInfo.shiftZ);
            }

            for (int i = 0; i < terrainInfo.meterPositions.GetLength(1); i++)
            {
                vertices[iterator++] = new Vector3(terrainInfo.meterPositions[0, i].y - terrainInfo.shiftX,
                                            0,
                                            terrainInfo.meterPositions[0, i].x - terrainInfo.shiftZ);
            }

            //BOTTOM WALL
            for (int i = 0; i < terrainInfo.meterPositions.GetLength(1); i++)
            {
                vertices[iterator++] = new Vector3(terrainInfo.meterPositions[length1 - 1, i].y - terrainInfo.shiftX,
                                            terrainInfo.terrainHeights[length1 - 1, i],
                                            terrainInfo.meterPositions[length1 - 1, i].x - terrainInfo.shiftZ);
            }

            for (int i = 0; i < terrainInfo.meterPositions.GetLength(1); i++)
            {
                vertices[iterator++] = new Vector3(terrainInfo.meterPositions[length1 - 1, i].y - terrainInfo.shiftX,
                                            0,
                                            terrainInfo.meterPositions[length1 - 1, i].x - terrainInfo.shiftZ);
            }


            int[] triangles = new int[(length1 + length2 - 2) * 12];
            iterator = 0;

            //LEFT WALL
            for (int i = 0; i < length1 - 1; i++)
            {
                triangles[iterator++] = i;
                triangles[iterator++] = i + 1;
                triangles[iterator++] = length1 + i + 1;
                triangles[iterator++] = length1 + i + 1;
                triangles[iterator++] = length1 + i;
                triangles[iterator++] = i;
            }
            //RIGHT WALL
            for (int i = 2 * length1; i < 2 * length1 + length1 - 1; i++)
            {
                triangles[iterator++] = i + 1;
                triangles[iterator++] = i;
                triangles[iterator++] = length1 + i;
                triangles[iterator++] = length1 + i;
                triangles[iterator++] = length1 + i + 1;
                triangles[iterator++] = i + 1;
            }
            //TOP WALL
            for (int i = 4 * length1; i < 4 * length1 + length2 - 1; i++)
            {
                triangles[iterator++] = i + 1;
                triangles[iterator++] = i;
                triangles[iterator++] = length2 + i;
                triangles[iterator++] = length2 + i;
                triangles[iterator++] = length2 + i + 1;
                triangles[iterator++] = i + 1;
            }

            //BOTTOM WALL
            for (int i = 4 * length1 + 2 * length2; i < 4 * length1 + 2 * length2 + length2 - 1; i++)
            {
                triangles[iterator++] = i;
                triangles[iterator++] = i + 1;
                triangles[iterator++] = length2 + i + 1;
                triangles[iterator++] = length2 + i + 1;
                triangles[iterator++] = length2 + i;
                triangles[iterator++] = i;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;

            GameObject downsideWalls = new GameObject("Walls", typeof(MeshRenderer), typeof(MeshFilter));
            downsideWalls.transform.parent = terrainObject.transform;

            MeshRenderer meshRenderer = downsideWalls.GetComponent<MeshRenderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }
            meshRenderer.material = new Material(shader);

            MeshFilter meshFilter = downsideWalls.GetComponent<MeshFilter>();
            meshFilter.mesh = mesh;

        }

        internal void UpdateTexture()
        {
            for (int i = 0; i < terrainInfo.grids.Count; i++)
            {
                var grid = terrainInfo.grids[i];
                var go = gridList[i];
                MeshRenderer meshrenderer = go.GetComponent<MeshRenderer>();
                meshrenderer.material.mainTexture = texturehandler.generateTexture(textureType, grid.bbox, grid.indexX, grid.indexY, "test");//OSMfileName);
            }

            //var texture = texturehandler.GetFinalTexture(scenebbox);

            //foreach (var grid in gridList)
            //{
            //    MeshRenderer meshrenderer = grid.GetComponent<MeshRenderer>();
            //    meshrenderer.material.mainTexture = texture;
            //}
        }
    }
}
