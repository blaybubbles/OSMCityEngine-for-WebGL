using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace Assets.Scripts.Utils
{
    public class FileDownloader : Singleton<FileDownloader>, ICreateMe
    {
        public IEnumerator Loading(string url, string id, Action<bool, string, byte[]> callback)
        {
            var www = UnityWebRequest.Get(url);
            www.timeout = 120;
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                callback(false, id, null);
            }
            else
            {
                var data = www.downloadHandler.data;
                if (callback != null)
                    callback(true, id, data);
            }
        }

        public void DownloadfromURL(string url, string id, Action<bool, string, byte[]> callback)
        {
            StartCoroutine(Loading(url, id, callback));// downloadfunction(obj);
        }

    }


}
