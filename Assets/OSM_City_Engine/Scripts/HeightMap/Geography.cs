using System;

using UnityEngine;

namespace Assets.Scripts.HeightMap
{
    class Geography
    {
        public static readonly float originShift = (float)Math.PI * 6378137.0f;
        public static float circuitLen = 2.0f * Mathf.PI * 6378137.0f;
        private int tileSize;
        private float initialResolution;
        public Geography()
        {
            tileSize = 256;
            initialResolution = 2.0f * (float)Math.PI * 6378137.0f / (float)tileSize;
        }

        //"Converts given lat/lon in WGS84 Datum to XY in Spherical Mercator EPSG:900913"
        public static Vector2 LatLontoMeters(float lat, float lon)
        {

            float meterx, meterz;
            meterz = lon * originShift / 180.0f;
            meterx = (float)(Math.Log(Math.Tan((90.0f + lat) * Math.PI / 360.0f)) / (Math.PI / 180.0f));
            meterx = meterx * originShift / 180.0f;

            return new Vector2(meterx, meterz);

        }
        //"Converts XY point from Spherical Mercator EPSG:900913 to lat/lon in WGS84 Datum" 
        public Vector2 meterstoLatLon(float meterx, float meterz)
        {

            float lat, lon;

            lon = (meterz / originShift) * 180.0f;
            lat = (meterx / originShift) * 180.0f;
            lat = 180.0f / (float)Math.PI * (float)(2.0f * Math.Atan(Math.Exp(lat * Math.PI / 180.0f)) - (float)Math.PI / 2.0f);

            return new Vector2(lat, lon);
        }

        public Vector2 meterstoLatLonDouble(float meterx, float meterz)
        {

            double lat, lon;

            lon = (meterz / originShift) * 180.0f;
            lat = (meterx / originShift) * 180.0f;
            lat = 180.0 / Math.PI * (2.0 * Math.Atan(Math.Exp(lat * Math.PI / 180.0)) - Math.PI / 2.0);

            return new Vector2((float)lat, (float)lon);
        }

        //"Converts EPSG:900913 to pyramid pixel Vector2s in given zoom level"
        public Vector2 metertoPixel(float meterx, float meterz, int zoom)
        {
            float res = Resolution(zoom);
            float pixelX, pixelY;

            pixelY = (-meterx + originShift) / res;
            pixelX = (meterz + originShift) / res;

            return new Vector2(pixelX, pixelY);

        }

        //Converts pixel Vector2s in given zoom level of pyramid to EPSG:900913 
        public Vector2 pixeltoMeter(float pixelX, float pixelY, int zoom)
        {
            float res = Resolution(zoom);
            float meterX = (pixelY * res) - originShift;
            float meterZ = (pixelX * res) - originShift;
            return new Vector2(meterX, meterZ);
        }

        //"Returns a tile covering region in given pixel Vector2s"
        public Vector2 pixeltoTile(float pixelX, float pixelY)
        {
            int Tilex = (int)(Math.Ceiling(pixelX / (double)tileSize)) - 1;
            int Tiley = (int)(Math.Ceiling(pixelY / (double)tileSize)) - 1;

            return new Vector2(Tilex, Tiley);
        }

        //"Returns tile for given mercator coordinates"
        public Vector2 MetersToTile(float meterx, float meterz, int zoom)
        {

            Vector2 tempResult = metertoPixel(meterx, meterz, zoom);
            return pixeltoTile(tempResult.x, tempResult.y);
        }

        //"Resolution (meters/pixel) for given zoom level (measured at Equator)"
        public float Resolution(int zoom)
        {
            return initialResolution / (float)Math.Pow(2, zoom);
        }

        public static float MercatorYToTile(float mercatorY, int z)
        {
            var tileY = (originShift - mercatorY) / (circuitLen / ((float)(1 << z)));
            return tileY;
        }

        public static float MercatorXToTile(float mercatorX, int z)
        {
            var tileX = (originShift + mercatorX) / (circuitLen / ((float)(1 << z)));
            return tileX;
        }

        internal static int Long2tilex(float lon, int z)
        {
            return (int)(Math.Floor((lon + 180.0) / 360.0 * (1 << z)));
        }

        internal static int Lat2tiley(float lat, int z)
        {
            var rad = lat * Mathf.Deg2Rad;
            return (int)Mathf.Floor((1 - Mathf.Log(Mathf.Tan(rad) + 1 / Mathf.Cos(rad)) / Mathf.PI) / 2 * (1 << z));
        }

        public static (Vector2, Vector2) TileRange(float minlat, float minlon, float maxlat, float maxlon, int zoom)
        {
            var minx = Long2tilex(minlon, zoom);
            var miny = Lat2tiley(minlat, zoom);
            var maxx = Long2tilex(maxlon, zoom);
            var maxy = Lat2tiley(maxlat, zoom);
            return (new Vector2(minx, miny), new Vector2(maxx, maxy));
        }

        public static (Vector2, Vector2) MetersToTileRange(float left, float bottom, float right, float top, int zoom)
        {
            var minX = MercatorXToTile(left, zoom);
            var minY = MercatorYToTile(bottom, zoom);
            var maxX = MercatorXToTile(right, zoom);
            var maxY = MercatorYToTile(top, zoom);
            return (new Vector2(minX, minY), new Vector2(maxX, maxY));
        }
        //returns bounds of the given tile in EPSG:900913 coordinates
        public float[] tileMetersBounds(float tx, float ty, int zoom)
        {
            var min = pixeltoMeter(tx * tileSize, ty * tileSize, zoom);
            var max = pixeltoMeter((tx + 1) * tileSize, (ty + 1) * tileSize, zoom);
            return new float[] { min[0], min[1], max[0], max[1] };
        }


    }
}
