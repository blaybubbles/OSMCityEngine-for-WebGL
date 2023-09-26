
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Assets.Scripts.HeightMap;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using UnityEngine;

public partial class OsmJson
{
    [JsonProperty("version")]
    public double Version { get; set; }

    [JsonProperty("generator")]
    public string Generator { get; set; }

    [JsonProperty("osm3s")]
    public OsmCopyright Osm3S { get; set; }

    [JsonProperty("elements")]

    public OsmElement[] Elements { get; set; }


    //[JsonIgnore]
    //public List<NodeElement> Nodes = new List<NodeElement>();
}

[Serializable]
public class OsmElement
{
    public TypeEnum osm_type;

    [JsonProperty("type"), JsonConverter(typeof(TypeEnumConverter))]
    public TypeEnum Type { get => osm_type; set => osm_type = value; }


    public string osm_id;

    [JsonProperty("id")]
    public string id { get => osm_id; set => osm_id = value; }

    public string[] osm_nodes;
    [JsonProperty("nodes", NullValueHandling = NullValueHandling.Ignore)]
    public string[] Nodes { get => osm_nodes; set => osm_nodes = value; }

    public List<TagItem> tagList;

    public Dictionary<string, string> osm_tags;

    [JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, string> Tags
    {
        get
        {
            if (osm_tags == null)
            {
                if (tagList == null)
                {
                    osm_tags = new Dictionary<string, string>();
                }
                else
                {
                    osm_tags = tagList.ToDictionary(x => x.TagName, x => x.TagValue);
                }
            }
            return osm_tags;
        }
        set
        {
            osm_tags = value;
            if (tagList == null)
            {
                tagList = osm_tags.Select(x => new TagItem { TagName = x.Key, TagValue = x.Value }).ToList();
            }
        }
    }

    [SerializeField]
    public Member[] membersList;

    [JsonProperty("members", NullValueHandling = NullValueHandling.Ignore)]
    public Member[] Members { get => membersList; set => membersList = value; }
    public string Id => osm_id;

}

[Serializable]
public class TagItem
{
    public string TagName;
    public string TagValue;
    public string K=> TagName;
    public string V => TagValue;
}

public class Member
{
    [JsonProperty("type")]
    public TypeEnum Type { get; set; }

    [JsonProperty("ref")]
    public string Ref { get; set; }

    [JsonProperty("role")]
    public string Role { get; set; }
}

public partial class OsmCopyright
{
    [JsonProperty("timestamp_osm_base")]
    public DateTimeOffset TimestampOsmBase { get; set; }

    [JsonProperty("copyright")]
    public string Copyright { get; set; }
}

public enum TypeEnum
{
    Node, Relation, Way, Area,
    Unknown
}
internal static class Converter
{
    public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
    {
        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
        DateParseHandling = DateParseHandling.None,
        Converters =
            {
                //TypeEnumConverter.Singleton,
                OsmElementConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
    };
}

public class OsmNodeIdsConverter : JsonConverter
{
    public override bool CanConvert(Type t) => t == typeof(string[]);

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null) return null;
        var value = serializer.Deserialize<string[]>(reader);
        return value;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();

    }
}

internal class TypeEnumConverter : JsonConverter
{
    public override bool CanConvert(Type t) => t == typeof(TypeEnum) || t == typeof(TypeEnum?);

    public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null) return null;
        var value = serializer.Deserialize<string>(reader);
        switch (value)
        {
            case "node":
                return TypeEnum.Node;
            case "relation":
                return TypeEnum.Relation;
            case "way":
                return TypeEnum.Way;
            case "area":
                return TypeEnum.Area;
            default:
                return TypeEnum.Unknown;
        }
        throw new Exception("Cannot unmarshal type TypeEnum");
    }

    public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
    {
        if (untypedValue == null)
        {
            serializer.Serialize(writer, null);
            return;
        }
        var value = (TypeEnum)untypedValue;
        switch (value)
        {
            case TypeEnum.Node:
                serializer.Serialize(writer, "node");
                return;
            case TypeEnum.Relation:
                serializer.Serialize(writer, "relation");
                return;
            case TypeEnum.Way:
                serializer.Serialize(writer, "way");
                return;
            case TypeEnum.Area:
                serializer.Serialize(writer, "area");
                return;
        }
        throw new Exception("Cannot marshal type TypeEnum");
    }

}

internal class OsmElementConverter : JsonConverter
{
    public override bool CanConvert(Type t) => t == typeof(OsmElement);

    public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null) return null;
        var o = JObject.Load(reader);
        var value = (string)o["type"];
        switch (value)
        {
            case "node":
                return (OsmElement)o.ToObject<OsmNodeElement>();
            case "relation":
                return (OsmElement)o.ToObject<OsmRelationElement>();
            case "way":
                return (OsmElement)o.ToObject<OsmWayElement>();
            default:
                return o.ToObject<OsmDefaultElement>();
        }
        throw new Exception("Cannot unmarshal type TypeEnum");
    }

    public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
    {
        serializer.Serialize(writer, untypedValue);
        //if (untypedValue == null)
        //{
        //    serializer.Serialize(writer, null);
        //    return;
        //}
        //var value = (TypeEnum)untypedValue;
        //switch (value)
        //{
        //    case TypeEnum.Node:
        //        serializer.Serialize(writer, "node");
        //        return;
        //    case TypeEnum.Relation:
        //        serializer.Serialize(writer, "relation");
        //        return;
        //    case TypeEnum.Way:
        //        serializer.Serialize(writer, "way");
        //        return;
        //}
        //throw new Exception("Cannot marshal type TypeEnum");
    }

    public static readonly OsmElementConverter Singleton = new OsmElementConverter();
}

[Serializable]
internal class OsmDefaultElement : OsmElement
{
}

[Serializable]
public class OsmWayElement : OsmElement
{
}

[Serializable]
public class OsmRelationElement : OsmElement
{
}

[Serializable]
public class OsmNodeElement : OsmElement
{
    [JsonProperty("lat")]
    public float Lat { get; set; }

    [JsonProperty("lon")]
    public float Lon { get; set; }

    [JsonIgnore]
    private Vector2 mercatorPosition;
    [JsonIgnore]
    private Vector3 meterPosition;

    [JsonIgnore]
    public bool positionCalculated = false;
    [JsonIgnore]
    public bool positionMetersCalculated = false;

    [JsonIgnore]
    public Vector2 MercatorPosition
    {
        get
        {
            if (!this.positionCalculated)
            {
                mercatorPosition = Geography.LatLontoMeters((float)Lat, (float)Lon);
                positionCalculated = true;
            }

            return mercatorPosition;
        }
    }

    [JsonIgnore]
    public Vector3 ScenePosition
    {
        get
        {
            if (!positionMetersCalculated)
            {
                throw new Exception("Not calculated " + id);
            }
            return meterPosition;
        }
        set
        {
            meterPosition = value;
            positionMetersCalculated = true;
        }
    }

    public void CalculateScenePosition(Vector2 initPosition, float height)
    {
        var localPosition = mercatorPosition - initPosition;
        ScenePosition = new Vector3(localPosition.y, height, localPosition.x);
    }

}

