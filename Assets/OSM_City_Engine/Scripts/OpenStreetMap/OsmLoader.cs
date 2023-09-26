using Assets.Scripts.OpenStreetMap;
using Newtonsoft.Json;
using System.Collections.Generic;


namespace OsmCityEngine.OpenStreetMap
{
    public class OsmLoader
    {
        public const string STREETS_PREFIX = "Streets_";
        public const string BUILDINGS_PREFIX = "Buildings_";
        public const string GREEN_PREFIX = "Green_";
        private BBox scenebbox;
        private LayerSettings settings;

        public List<string> loading = new List<string>();
        public OsmLoader(BBox scenebbox, LayerSettings layers)
        {
            this.scenebbox = scenebbox;

            layers ??= LayerSettings.GetDefaultSettings();
            settings = layers;

            LoadData();
        }

        private void LoadData()
        {
            var overpass = new OverpassHandler();
            string str_box = overpass.GetBox(scenebbox);

            if (settings.streets != null && !settings.streets.tags.IsNullOrEmpty() )
            {
                var id = STREETS_PREFIX + str_box.Replace('.', '_');
                var query = overpass.CreateNetworkQuery(str_box);
                //var query = overpass.CreateOverpassQuery(settings.streets.tags, str_box);

                OsmConnector.Instance.AddToQueue(id, query, ParseData);
                loading.Add(id);
            }

            if (settings.building != null && !settings.building.tags.IsNullOrEmpty())
            {
                var id = "Buildings_" + str_box.Replace('.', '_');
                var query = overpass.CreateOverpassQuery(settings.building.tags, str_box);

                OsmConnector.Instance.AddToQueue(id, query, ParseData);
                loading.Add(id);
            }

            if (settings.green != null && !settings.green.tags.IsNullOrEmpty())
            {
                var id = GREEN_PREFIX + str_box.Replace('.', '_');
                var query = overpass.CreateOverpassQuery(settings.green.tags, str_box);

                OsmConnector.Instance.AddToQueue(id, query, ParseData);
                loading.Add(id);
            }
        }


        public void ParseData(string id, byte[] data)
        {

        }
        
    }

    public class LayerSettings
    {
        [JsonProperty("streets")]
        public StreetsLayer streets;

        [JsonProperty("building")]
        public BuildingLayer building;

        [JsonProperty("green")]
        public GreenLayer green;

        public static LayerSettings CreateFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<LayerSettings>(jsonString);
        }

        public static LayerSettings GetDefaultSettings()
        {
            /*
             "streets": {
                "width": {
                    "primary": 5,
                    "secondary": 4,
                    "tertiary": 3,
                    "residential": 2,
                    "footway": 1
                }
            },
            "building": {
                "tags": {
                    "building": true
                }
            },
            "green": {
                "tags": {
                    "landuse": [
                        "grass",
                        "village_green"
                    ],
                    "leisure": "park"
                }
            }
             */
            return new LayerSettings
            {
                streets = new StreetsLayer
                {
                    tags = new List<LayerTag>
                {
                    new LayerTag("highway", new [] {"primary" }),
                    new LayerTag("highway", new [] {"secondary" }),
                    new LayerTag("highway", new [] {"tertiary" }),
                    new LayerTag("highway", new [] {"residential" }),
                    new LayerTag("highway", new [] {"footway" }),
                }
                },
                building = new BuildingLayer
                {
                    tags = new List<LayerTag>
                {
                    new LayerTag("building", null),
                }
                },
                green = new GreenLayer
                {
                    tags = new List<LayerTag>
                {
                    new LayerTag("landuse",  new [] {"grass","village_green" }),
                    new LayerTag("leisure", new [] {"park" }),
                    new LayerTag("natural",  new [] {"tree", "island", "wood" }),
                    new LayerTag("amenity",  new [] {"fountain", "bench","drinking_water", "waste_basket" } ),
                    new LayerTag("natural", new [] { "water", "bay" }),
                }
                }
            };
        }
    }

    public class GreenLayer
    {
        public List<LayerTag> tags;
    }

    public class BuildingLayer
    {
        public List<LayerTag> tags;
    }

    public class StreetsLayer
    {
        public List<LayerTag> tags;
    }

    [System.Serializable]
    public class LayerTag
    {
        public string Key;
        public string[] Value; // bool or string

        public LayerTag(string key, string[] value)
        {
            Key = key;
            Value = value;
        }

        public bool TryGetBool(out bool boolvalue)
        {
            boolvalue = false;
            if (Value.IsNullOrEmpty())
            {
                return true;
                //string str = Value.Trim().ToLower();
                //boolvalue = str == "true" || str == "yes";
                //return boolvalue || str == "false" || str == "no";
            }

            return false;
        }
    }
}