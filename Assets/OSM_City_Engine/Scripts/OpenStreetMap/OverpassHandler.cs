using Assets.Scripts.OpenStreetMap;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OsmCityEngine.OpenStreetMap
{
    public class OverpassHandler
    {
        public static CultureInfo culture = CultureInfo.InvariantCulture;

        public string GetBox(BBox bbox)
        {
            return string.Format(culture.NumberFormat, @"{0},{1},{2},{3}", bbox.bottom, bbox.left, bbox.top, bbox.right);
        }
        internal string GetQuery(List<LayerTag> tags, BBox scenebbox)
        {
            return CreateOverpassQuery(tags, GetBox(scenebbox));
        }

        public string CreateOverpassQuery(List<LayerTag> tags, string bbox)
        {
            var query = "";
            var tags_str = new List<string>();
            foreach (var tag in tags)
            {
                if (tag.TryGetBool(out bool value))
                {
                    if (value == true)
                    {
                        tags_str.Add($"['{tag.Key}']");
                    }
                    else
                    {
                        //~
                        tags_str.Add($"['{tag.Key}'~'{tag.Value}']");
                    }
                }
                else if (tag.Value.Length == 1)
                {
                    tags_str.Add($"['{tag.Key}'='{tag.Value}']");
                }
                //else if (string.IsNullOrEmpty(tag.Value))
                //{
                //    tags_str.Add($"['{tag.Key}']");
                //}
                else
                {
                    tags_str.Add($"['{tag.Key}'~'{string.Join("|", tag.Value.Select(t => t))}']");
                }

            }

            /*
              rel({{bbox}})["natural"="water"]->.rw1;
                rel({{bbox}})["waterway"]->.rw2; 

             */

            foreach (var item in new[] { "node", "way", "relation" })
            {
                query += string.Join(';', tags_str.Select(t => $"{item}{t}({bbox})")) + ";";
                //query += $"{item}{string.Join("", tags_str)}({bbox});";
            }

            // to do setings
            //var maxsize = $"[maxsize:{memory}]";
            //var timeout = $"[timeout:{timeout}]";
            var overpass_settings = "[out:json][timeout:25]";

            return $"{overpass_settings};({query});(._;>;);out;";
            // $"{overpass_settings};({query});(._;>;);out;";
            //"{overpass_settings};(way{osm_filter}(poly:{polygon_coord_str!r});>;);out;"
        }

        public string CreateNetworkQuery(string bbox)
        {
            var query = $"way['highway']['area'!~'yes']['highway'!~'abandoned|construction|no|planned|platform|proposed|raceway|razed']({bbox});way['railway'][!'layer']['railway'!~'abandoned|construction|no|planned|platform|proposed|raceway|razed']({bbox});";
            var overpass_settings = "[out:json][timeout:25]";
            return $"{overpass_settings};({query});(._;>;);out;";
        }
    }
}