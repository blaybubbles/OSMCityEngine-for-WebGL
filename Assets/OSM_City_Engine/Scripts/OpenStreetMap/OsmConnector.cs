using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class OsmConnector : MonoBehaviour
{
    // singleton members
    public static OsmConnector instance;

    public string overpassMainEndpoints = "https://overpass-api.de/api/interpreter";

    public Action<string, string> OnLoaded;
    public static OsmConnector Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<OsmConnector>(true);
            }
            if (instance == null )
            {
                var go = new GameObject(nameof(OsmConnector), typeof(OsmConnector));
                instance = go.GetComponent<OsmConnector>();
            }

            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.Log("Delete " + gameObject.name);
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    public List<LoadingQueueItem> queue = new List<LoadingQueueItem>();
    public List<LoadingQueueItem> complete = new List<LoadingQueueItem>();
    private bool loading;
    private bool searching;

    public void StartLoading()
    {
        if (queue.Count > 0 && !loading)
        {
            StartCoroutine(LoadingCoroutine());
        }
    }

    public void AddToQueue(string id, string query, Action<string, byte[]> parseData)
    {
        var item = new LoadingQueueItem();
        item.query = query;
        item.id = id;
        item.status = LoadingStatus.NotStarted;
        queue.Add(item);
        StartLoading();
    }

    private IEnumerator LoadingCoroutine()
    {
        loading = true;
        if (queue.Count == 0) { yield break; }
        while (queue.Count > 0)
        {
            var item = queue[0];
            item.status = LoadingStatus.Loading;

            queue.RemoveAt(0);
            complete.Add(item);
            yield return LoadingItem(item);
        }

        loading = false;
    }

    private IEnumerator LoadingItem(LoadingQueueItem item)
    {
        Debug.Log("Loading osm " + item.id);
        var url = overpassMainEndpoints;
        var www = UnityWebRequest.Get(url + "?data=" +  item.query);
        yield return www.SendWebRequest();

        //if (www.responseCode == 429 || www.responseCode == 504)
        //{
        //    if (item.tryCount < 3)
        //    {
        //        queue.Add(item);
        //    }
        //    //Debug.Log("Waiting for " + error_pause + " seconds before retrying.");
        //    //yield return new WaitForSeconds(error_pause);
        //    //www = UnityWebRequest.Get(url);
        //}

        if (www.result != UnityWebRequest.Result.Success)
        {
            item.status = LoadingStatus.Error;
            item.tryCount++;
            Debug.Log(www.error);
            //if (item.tryCount < 3)
            //{
            //    queue.Add(item);
            //}
            Debug.Log("Error Loading osm " + item.id );
            Debug.Log("Error Loading osm url " + url );

        }
        else
        {
            item.result = www.downloadHandler.text;
            item.status = LoadingStatus.Finished;
            Debug.Log("Loaded osm" + item.id);

            OnLoaded?.Invoke(item.id, item.result);
        }
    }

    public void Search(string name, Action<string> callback = null, Action errorCallback = null)
    {
        if(searching) return;
        StartCoroutine(NominatimRequest("q=" + name + "&format=json&addressdetails=1&limit=10", "search", 1, 60, callback, errorCallback));
    }
    public IEnumerator NominatimRequest(string query, string request_type = "search", int pause = 1, int error_pause = 60, Action<string> callback = null, Action errorCallback = null)
    {
        searching = true;
        //Send a HTTP GET request to the Nominatim API and return response.
        string url = OsmSettings.nominatimEndpoint.TrimEnd('/') + "/" + request_type + "?" + query;
        var www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.responseCode == 429 || www.responseCode == 504)
        {
            Debug.Log("Waiting for " + error_pause + " seconds before retrying.");
            yield return new WaitForSeconds(error_pause);
            www = UnityWebRequest.Get(url);
        }

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
           
            {
                errorCallback();
            }
        }
        else
        {
            callback(www.downloadHandler.text);
        }
        searching = false;

    }
}

[Serializable]
public class LoadingQueueItem
{
    public string query;
    public LoadingStatus status;

    public string result;
    internal int tryCount;
    public string id;
}

[Serializable]
public enum LoadingStatus
{
    NotStarted,
    Loading,
    Finished,
    Error,
    Parsed
}
