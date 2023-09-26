using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Utils;
using Assets.Scripts.OpenStreetMap;
using UnityEngine;
using System.IO;
using System.Collections;

namespace Assets.Scripts.HeightMap
{

    public enum MapProvider
    {
        BingMapAerial,
        BingMapStreet,
        MapQuest,
        OpenStreetMap,
        OpenStreetMapNoLabel
    }


    class TerrainTextureHandler
    {

        public int zoomLevel;
        public string tileFolder;
        private int tileSize = 256;
        public List<TextureItem> loadingtextures = new List<TextureItem>();

        public TerrainTextureHandler()
        {
            zoomLevel = 18;

            tileFolder = Application.streamingAssetsPath + "/Textures/Tiles/";

            if (!Directory.Exists(tileFolder))
                Directory.CreateDirectory(tileFolder);

        }
        public Texture2D generateTexture(MapProvider provider, BBox bbox, int i, int j, string OSMFileName)
        {
            string[] proj = OSMFileName.Split(new char[] { '/', '\\' });
            string projectName = proj[proj.Length - 1];

            if (File.Exists(tileFolder + "/final/" + provider.ToString("G") + "_" + projectName + "_" + i + "_" + j + ".png"))
            {
                byte[] fileData;
                fileData = File.ReadAllBytes(tileFolder + "/final/" + provider.ToString("G") + "_" + projectName + "_" + i + "_" + j + ".png");
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(fileData);
                return texture;
            }

            Texture2D uncroppedTexture = generateUncroppedTexture(bbox, provider, i,j);
            Rect cropWindow = generateCroppingRect(bbox);
            Texture2D finalTexture = CropTexture(uncroppedTexture, cropWindow);

            if (!Directory.Exists(tileFolder + "/final/"))
                Directory.CreateDirectory(tileFolder + "/final/");

            var tex = new Texture2D(finalTexture.width, finalTexture.height);
            tex.SetPixels32(finalTexture.GetPixels32());
            tex.Apply(false);
            //File.WriteAllBytes(tileFolder + "/final/" + provider.ToString("G") + "_" + projectName + "_" + i + "_" + j + ".png", tex.EncodeToPNG());

            return finalTexture;
        }
        private Texture2D generateUncroppedTexture(BBox bbox, MapProvider provider, int gridCol, int gridRow)
        {

            Vector2 mintileCoord = new Vector2();
            Vector2 maxtileCoord = new Vector2();

            FindTiles(bbox, ref mintileCoord, ref maxtileCoord);

            Debug.Log("<color=green>TEXTURE:</color> mintile X:" + mintileCoord.x + " Y:" + mintileCoord.y +
                " maxtile X:" + maxtileCoord.x + " Y:" + maxtileCoord.y);


            int ColumnCount = (int)(1 + maxtileCoord.x - mintileCoord.x);
            int RowCount = (int)(1 + mintileCoord.y - maxtileCoord.y);

            //Texture2D[,] textures = new Texture2D[RowCount, ColumnCount];
            string[,] texturefiles = new string[RowCount, ColumnCount];
            Texture2D[,] texture2Ds = new Texture2D[RowCount, ColumnCount];
            for (int i = (int)maxtileCoord.y, m = 0; i <= (int)mintileCoord.y; i++, m++)
            {
                for (int j = (int)mintileCoord.x, n = 0; j <= (int)maxtileCoord.x; j++, n++)
                {
                    texturefiles[m, n] = DownloadTile(j, i, provider);
                    var loadItem = loadingtextures.FirstOrDefault(x => x.id == texturefiles[m, n]);
                    texture2Ds[m, n] = loadItem?.texture?? new Texture2D(tileSize, tileSize);
                }
            }


            Texture2D finalTexture = ConcatTexture(texture2Ds, ColumnCount, RowCount);

            return finalTexture;
        }


        private Rect generateCroppingRect(BBox bbox)
        {
            Geography geo = new Geography();

            Vector2 mintileCoord = new Vector2();
            Vector2 maxtileCoord = new Vector2();
            FindTiles(bbox, ref mintileCoord, ref maxtileCoord);

            float left, bottom, right, top;
            double res = geo.Resolution(zoomLevel);

            left = (float)Math.Round((bbox.meterLeft + Geography.originShift) / res) - (mintileCoord.x * tileSize);
            right = (float)Math.Round((bbox.meterRight + Geography.originShift) / res) - (mintileCoord.x * tileSize);

            bottom = (float)Math.Round((-bbox.meterBottom + Geography.originShift) / res) - (maxtileCoord.y * tileSize);
            top = (float)Math.Round((-bbox.meterTop + Geography.originShift) / res) - (maxtileCoord.y * tileSize);

            Rect croppingRect = new Rect(left, top, right - left, bottom - top);

            return croppingRect;
        }

        internal void FindTiles(BBox bbox, ref Vector2 mintileCoord, ref Vector2 maxtileCoord)
        {
            Geography geo = new Geography();

            mintileCoord = geo.MetersToTile(bbox.meterBottom, bbox.meterLeft, zoomLevel);
            maxtileCoord = geo.MetersToTile(bbox.meterTop, bbox.meterRight, zoomLevel);
        }
        
        public string DownloadTile(int tilex, int tiley, MapProvider provider)
        {
            if (!Directory.Exists(tileFolder + provider.ToString("G")))
                Directory.CreateDirectory(tileFolder + provider.ToString("G"));

            string _URL = GetUrl(tilex, tiley, provider, out string savedfileName);

            if (string.IsNullOrEmpty(_URL))
            {
                return null;
            }



            savedfileName = provider.ToString("G") + "/" + savedfileName;
            var loadItem = loadingtextures.FirstOrDefault(x => x.id == savedfileName);
            if (loadItem == null)
            {
                if (File.Exists(tileFolder + savedfileName))
                {
                    var texture = new Texture2D(tileSize, tileSize);
                    texture.LoadImage(File.ReadAllBytes(tileFolder + savedfileName));
                    loadItem = new TextureItem() { id = savedfileName, col = tiley, row = tilex, texture = texture };
                    loadingtextures.Add(loadItem);
                    return savedfileName;
                }

                loadItem = new TextureItem() { id = savedfileName, col = tiley, row = tilex };
                loadingtextures.Add(loadItem);

                try
                {

                    FileDownloader.Instance.DownloadfromURL(_URL, savedfileName, LoadedTextures);//tileFolder + provider.ToString("G") + "/" + savedfileName);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.ToString());
                    if (provider == MapProvider.OpenStreetMapNoLabel)
                    {
                        _URL = "http://" + "a" + ".tile.openstreetmap.org/" + zoomLevel.ToString() + "/" + tilex.ToString() + "/" + tiley.ToString() + ".png";
                        FileDownloader.Instance.DownloadfromURL(_URL, savedfileName, LoadedTextures);
                        //tileFolder + provider.ToString("G") + "/" + savedfileName);
                    }

                }
            }
            else
            {
                return savedfileName;
            }

           
            //byte[] fileData = File.ReadAllBytes(tileFolder + provider.ToString("G") + "/" + savedfileName);

            //Texture2D texture = LoadedTextures(provider, savedfileName);
            //return texture;
            return savedfileName;
        }

        private string GetUrl(int tilex, int tiley, MapProvider provider, out string savedfileName)
        {
            savedfileName = zoomLevel + "_" + tilex + "_" + tiley;

            string _URL = "";
            switch (provider)
            {
                case MapProvider.MapQuest:
                    _URL = "https://otile1.mqcdn.com/tiles/1.0.0/osm/" + zoomLevel.ToString() + "/" + tilex.ToString() + "/" + tiley.ToString() + ".png";

                    savedfileName = savedfileName + ".png";
                    break;
                case MapProvider.OpenStreetMap:
                    _URL = "http://" + "a" + ".tile.openstreetmap.org/" + zoomLevel.ToString() + "/" + tilex.ToString() + "/" + tiley.ToString() + ".png";
                    savedfileName = savedfileName + ".png";
                    break;

                case MapProvider.OpenStreetMapNoLabel:
                    _URL = "https://a.tiles.wmflabs.org/osm-no-labels/" + zoomLevel.ToString() + "/" + tilex.ToString() + "/" + tiley.ToString() + ".png";
                    savedfileName = savedfileName + ".png";
                    break;

                case MapProvider.BingMapStreet:
                    _URL = "https://ecn.t" + ((tilex + tiley) % 7).ToString() + ".tiles.virtualearth.net/tiles/" + "r";
                    for (int i = zoomLevel - 1; i >= 0; i--)
                    {
                        _URL = _URL + (((((tiley >> i) & 1) << 1) + ((tilex >> i) & 1)));
                    }
                    _URL = _URL + ".png" + "?g=409&mkt=en-us&key=AuQuZJOwdxdhWQirn95FXRjAhVCcGPlCJ3jxoXK3ocqqyxK2WRUw65rCpKzIwCGD";
                    savedfileName = savedfileName + ".png";
                    break;

                case MapProvider.BingMapAerial:
                    _URL = "https://ecn.t" + ((tilex + tiley) % 7).ToString() + ".tiles.virtualearth.net/tiles/" + "a";
                    for (int i = zoomLevel - 1; i >= 0; i--)
                    {
                        _URL = _URL + (((((tiley >> i) & 1) << 1) + ((tilex >> i) & 1)));
                    }
                    _URL = _URL + ".jpeg" + "?g=409&mkt=en-us&key=AuQuZJOwdxdhWQirn95FXRjAhVCcGPlCJ3jxoXK3ocqqyxK2WRUw65rCpKzIwCGD";
                    savedfileName = savedfileName + ".jpg";
                    break;

                default:
                    break;
            }

            return _URL;

        }


        public void LoadedTextures(bool ok, string savedfileName, byte[] bytes)
        {
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(bytes);
            var item = loadingtextures.FirstOrDefault(x => x.id == savedfileName);
            if (item == null)
            {
                //loadingtextures.Add(new TextureItem { id = savedfileName, texture = texture });
            }
            else
            {
                item.texture = texture;
                if(texture.width == tileSize && texture.height == tileSize)
                {
                    File.WriteAllBytes(tileFolder +  "/" + savedfileName, bytes);
                }
            }
            CheckTexture();
        }

        private void CheckTexture()
        {
            if (loadingtextures.All(x => x.texture != null))
            {
                CityConstructor.Instance.OnTexturesLoaded();
            }
        }

        private Texture2D CropTexture(Texture2D originalTexture, Rect cropRect)
        {
            int bottom = originalTexture.height - (int)(cropRect.y + cropRect.height);

            Texture2D newTexture = new Texture2D((int)cropRect.width, (int)cropRect.height, TextureFormat.RGBA32, false);
            Color[] pixels = originalTexture.GetPixels((int)cropRect.x, bottom, (int)cropRect.width, (int)cropRect.height, 0);
            newTexture.SetPixels(pixels);
            newTexture.wrapMode = TextureWrapMode.Clamp;
            newTexture.Apply();
            return newTexture;
        }
        private Texture2D ConcatTexture(Texture2D[,] textures, int ColumnCount, int RowCount)
        {
            Texture2D finalTexture = new Texture2D(ColumnCount * tileSize, RowCount * tileSize);

            for (int i = 0; i < RowCount; i++)
                for (int j = 0; j < ColumnCount; j++)
                {
                    if (textures[i,j].width != tileSize || textures[i, j].height != tileSize)
                    {
                        Debug.Log($"Broken texture {textures[i,j].width} {i} {j}");
                        continue;
                    }

                    finalTexture.SetPixels(j * tileSize, (RowCount - i - 1) * tileSize, tileSize, tileSize, textures[i, j].GetPixels());
                }
            finalTexture.wrapMode = TextureWrapMode.Clamp;
            finalTexture.Apply();
            return finalTexture;
        }

        internal Texture2D GetFinalTexture(BBox bbox)
        {
            Vector2 mintileCoord = new Vector2();
            Vector2 maxtileCoord = new Vector2();


            FindTiles(bbox, ref mintileCoord, ref maxtileCoord);
            int ColumnCount = (int)(1 + maxtileCoord.x - mintileCoord.x);
            int RowCount = (int)(1 + mintileCoord.y - maxtileCoord.y);

            Texture2D[,] textures = new Texture2D[RowCount, ColumnCount];

            foreach (var item in loadingtextures)
            {
                textures[item.col, item.row] = item.texture ?? new Texture2D(tileSize, tileSize);
            }

            Texture2D uncroppedTexture = ConcatTexture(textures, ColumnCount, RowCount);
            Rect cropWindow = generateCroppingRect(bbox);
            Texture2D finalTexture = CropTexture(uncroppedTexture, cropWindow);

            if (!Directory.Exists(tileFolder + "/final/"))
                Directory.CreateDirectory(tileFolder + "/final/");

            var tex = new Texture2D(finalTexture.width, finalTexture.height);
            tex.SetPixels32(finalTexture.GetPixels32());
            tex.Apply(false);
            //File.WriteAllBytes(tileFolder + "/final/" + provider.ToString("G") + "_" + projectName + "_" + i + "_" + j + ".png", tex.EncodeToPNG());

            return finalTexture;
        }

        public IEnumerator LoadAllTextures(BBox bbox, MapProvider provider)
        {
            Vector2 mintileCoord = new();
            Vector2 maxtileCoord = new();

            FindTiles(bbox, ref mintileCoord, ref maxtileCoord);

            Debug.Log("Texture range: " + mintileCoord + " " + maxtileCoord);
            int ColumnCount = (int)(1 + maxtileCoord.x - mintileCoord.x);
            int RowCount = (int)(1 + mintileCoord.y - maxtileCoord.y);

            for (int tiley = (int)maxtileCoord.y, m = 0; tiley <= (int)mintileCoord.y; tiley++, m++)
            {
                for (int tilex = (int)mintileCoord.x, n = 0; tilex <= (int)maxtileCoord.x; tilex++, n++)
                {

                    var url = GetUrl(tilex, tiley, provider, out string savedfileName);

                    Debug.Log(url);

                    var loadItem = loadingtextures.FirstOrDefault(x => x.id == savedfileName);
                    if (loadItem == null)
                    {
                        loadItem = new TextureItem() { id = savedfileName, col = tiley, row = tilex };
                        loadingtextures.Add(loadItem);
                        FileDownloader.Instance.DownloadfromURL(url, savedfileName, LoadedTextures);
                    }
                   
                    
                }
            }
            yield return new WaitUntil(()=> loadingtextures.All(x=>x.texture != null));
        }
    }

    [Serializable]
    public class TextureItem
    {
        public string id;
        public int col;
        public int row;
        public Texture2D texture;
    }
}
