using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Assets.Scripts.OpenStreetMap;
using System.IO;
using Assets.Scripts.Utils;
using System.IO.Compression;

namespace Assets.Scripts.HeightMap
{

    public struct HeightmapInfo
    {
        public int Lat, Lon;
        public Vector2 coordmin, coordmax;
        public float sizeX, sizeZ;  //deltaX, deltaZ -- Sample size in meter
        public float height;
        public float width;
    };


    public enum HeightmapContinent
    {
        North_America,
        South_America,
        Africa,
        Eurasia,
        Australia
    }

    [Serializable]
    public class HeightmapLoader
    {

        public HeightmapContinent continent;

        //Base url of NASA srtm3 heightmap 
        //public const string baseURL = "https://dds.cr.usgs.gov/srtm/version2_1/SRTM3/";
        
        // New url with detection region
        public const string baseURL ="";

        //Heightmap Array 1201x1201 
        public short[,] heightmap;
        public string currentFilename = null;

        //Constructor create heightmap object
        public HeightmapLoader(BBox bbox)//, HeightmapContinent _continent)
        {

            if (!Directory.Exists(Path.Combine(Application.streamingAssetsPath, "HeightmapFiles/")))
                Directory.CreateDirectory(Path.Combine(Application.streamingAssetsPath, "HeightmapFiles/"));

            heightmap = new short[1201, 1201];
            //continent = _continent;

            if ((Math.Floor(bbox.left) != Math.Floor(bbox.right)) || (Math.Floor(bbox.bottom) != Math.Floor(bbox.top)))
            {
                Debug.Log("<color=red>HEIGHTMAP ERROR:</color> Specified area requires multiple heightmap files!");
                return;
            }

            string filename = "";

            if (Math.Floor(bbox.bottom) >= 0.0f)
                filename = filename + "N" + Math.Floor(bbox.bottom).ToString("00");
            else
                filename = filename + "S" + Math.Abs(Math.Floor(bbox.bottom)).ToString("00");

            if (Math.Floor(bbox.left) >= 0.0f)
                filename = filename + "E" + Math.Floor(bbox.left).ToString("000");
            else
                filename = filename + "W" + Math.Abs(Math.Floor(bbox.left)).ToString("000");

            string savedFilename = filename + ".hgt";
            filename = filename + ".hgt.zip";

            Debug.Log("<color=blue>HEIGHTMAP</color> Filename: " + filename);

            string fullURL = baseURL +   filename;
            //string fullURL = baseURL + continent.ToString("G") + "/" + filename;


            string savePath = Path.Combine(Application.streamingAssetsPath, "HeightmapFiles/" + filename);

            Debug.Log("path: " + savePath + " url: " + fullURL);

            currentFilename = savedFilename;

            string extractPath = Path.Combine(Application.streamingAssetsPath, "HeightmapFiles", savedFilename + ".zip");
            if (File.Exists(extractPath))
            {
                //file://

                FileDownloader.Instance.DownloadfromURL("file://"+ extractPath.Replace('\\', '/'), savedFilename, Loaded);
                //Loaded(true, savedFilename, File.ReadAllBytes(extractPath));
            }
            else
            {
                FileDownloader.Instance.DownloadfromURL(fullURL, savedFilename, Loaded);
            }

            //Debug.Log("<color=blue>HEIGHTMAP</color> Download Complete!!");

            //string extractPath = Path.Combine(Application.streamingAssetsPath, "HeightmapFiles");
            //if (!File.Exists(extractPath))
            //    UniZip.Unzip(savePath, extractPath);

            //Debug.Log("<color=blue>HEIGHTMAP</color> Filemap Uncompress Complete!!");

            //fillHeightmap(extractPath + "/" + savedFilename);

            //Debug.Log("<color=blue>HEIGHTMAP</color> Filemap Loading Complete!!");


        }

        public void Loaded(bool ok, string savedFilename, byte[] bytes)
        {
            if(!ok)
            {
                Debug.Log("Error downloading heightmap file");
                return;
            }
            string extractPath = Path.Combine(Application.streamingAssetsPath, "HeightmapFiles", savedFilename + ".zip");

            if (!File.Exists(extractPath))
            {
                File.WriteAllBytes(extractPath, bytes);
            }

            var unzipped = UnzipData(bytes);
            Debug.Log("<color=blue>HEIGHTMAP</color> Filemap Uncompress Complete!!");

            fillHeightmap(unzipped);

            Debug.Log("<color=blue>HEIGHTMAP</color> Filemap Loading Complete!!");
            CityConstructor.Instance.OnHeightMapLoaded();

        }

        //Read raw data from .hgt file write it into heightmap array
        private void fillHeightmap(byte[] bytebuffer)
        {
            try
            {
                //using (var stream = new FileStream(savePath, FileMode.Open))
                {

                    //byte[] bytebuffer = new byte[stream.Length];
                    //bytebuffer = stream.GetBuffer();// ReadFully(stream);

                    byte[] buffer = new byte[2];
                    int it = 0;
                    for (int i = 0; i < 1201; i++)
                    {

                        for (int j = 0; j < 1201; j++)
                        {
                            buffer[0] = bytebuffer[it + 1];
                            buffer[1] = bytebuffer[it];
                            short number = BitConverter.ToInt16(buffer, 0);

                            if (number < -1000 && j > 0)
                                number = heightmap[i, j];
                            if (number < -1000 && j == 0)
                                number = heightmap[i - 1, j];

                            heightmap[i, j] = number;
                            it += 2;
                        }
                    }

                }
            }
            catch (FileNotFoundException)
            {
                Debug.Log("<color=red>ERROR</color> Heightmap is not exist");
            }

        }

        //Converts binary raw data to byteArray
        private byte[] ReadFully(FileStream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }


        /// <summary>
        /// Unzip file
        /// it works in web
        /// </summary>
        /// <param name="data"></param>
        /// <param name="fileName"></param>
        /// <param name="action"></param>
        internal byte[] UnzipData(byte[] data)
        {
            Stream arhiveStream = new MemoryStream(data);
            Stream unzippedEntryStream; // Unzipped data from a file in the archive
            MemoryStream ms = new MemoryStream();
            ZipArchive archive = new ZipArchive(arhiveStream);
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (entry.FullName.ToLower().Contains(".hgt"))
                {
                    unzippedEntryStream = entry.Open(); // .Open will return a stream
                    //action(unzippedEntryStream);
                    unzippedEntryStream.CopyTo(ms);
                }
            }

            return ms.ToArray();
        }
    }
}
