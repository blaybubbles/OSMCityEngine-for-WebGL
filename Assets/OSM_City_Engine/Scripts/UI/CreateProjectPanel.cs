using Newtonsoft.Json.Linq;
using OsmCityEngine.OpenStreetMap;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CreateProjectPanel : MonoBehaviour
{
    public UIDocument document;
    public Label label;
    private TextField placeInput;
    private bool searching = false;
    private float lat;
    private float lng;
    private bool found = false;
    private bool startingCreation = false;
    public Button renderButton;
    public string lastSearch;
    public Slider slider;
    public Label labelslider;


    private void OnEnable()
    {
        if (document == null)
        {
            document = GetComponent<UIDocument>();
        }

        if (document == null)
        {
            Debug.LogError("UIDocument not found");
            return;
        }
        label = document.rootVisualElement.Q<Label>("searchresult");

        placeInput = document.rootVisualElement.Q<TextField>("placeInput");

        if (placeInput != null)
        {
            placeInput.RegisterCallback<KeyDownEvent>(InputSubmit, TrickleDown.TrickleDown);
        }


        renderButton = document.rootVisualElement.Q<Button>("renderButton");

        if (renderButton != null)
        {

            //register a callback for the button
            renderButton.RegisterCallback<ClickEvent>(CreateScene);
        }

        slider = document.rootVisualElement.Q<Slider>("terrainsize");

        slider.RegisterValueChangedCallback(SizeValueChanged);

        labelslider = document.rootVisualElement.Q<Label>("terrainsizelabel");
    }

    private void SizeValueChanged(ChangeEvent<float> evt)
    {
        labelslider.text = evt.newValue.ToString();
    }

    private void InputSubmit(KeyDownEvent evt)
    {

        if (evt.keyCode == KeyCode.Return || evt.character == '\n')
        {
            var value = (evt.currentTarget as TextField)?.value;
            // Submit logic
            evt.StopPropagation();
            evt.PreventDefault();
            SearchPlace(value);
        }

        if (evt.modifiers == EventModifiers.Shift && (evt.keyCode == KeyCode.Tab || evt.character == '\t'))
        {
            // Focus logic
            evt.StopPropagation();
            evt.PreventDefault();
        }
    }

    private void SearchPlace(string value)
    {

        if (!string.IsNullOrEmpty(value) && (!searching || value.Trim() != lastSearch))
        {
            lastSearch = value.Trim();

            // try parse string like [41.638776366, 12.546644646] to lat lon coordinate
            var split = lastSearch.Trim('[').Trim(']').Split(',');
            if (split.Length == 2)
            {
                if (float.TryParse(split[0], System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out float latitude))
                    if (float.TryParse(split[1], System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out float longitude))
                    {
                        if(latitude > 90 || latitude < -90 || longitude > 180 || longitude < -180)
                        {
                            ErrorSearch();
                            return;
                        }
                        //ShowSearchResult($"[{{\"lat\":{latitude},\"lon\":{longitude}}}]");
                        SelLatLon(latitude, longitude);
                        found = true;
                        return;
                    }
            }

            searching = true;
            OsmConnector.Instance.Search(value, ShowSearchResult);
        }
    }

    public void ShowSearchResult(string text)
    {
        StopSearch();

        var response_json = JArray.Parse(text);
        if (response_json.Count == 0) return;
        float latitude = (float)(response_json[0]["lat"]);
        float longitude = (float)(response_json[0]["lon"]);

        SelLatLon(latitude, longitude);
        found = true;
        if (startingCreation) RunCreation();
    }

    private void SelLatLon(float latitude, float longitude)
    {
        lat = latitude;
        lng = longitude;
        Debug.Log("lat:" + lat + " lng:" + lng);
        if (label != null)
        {
            label.text = "lat:" + lat + " lng:" + lng;
        }
    }

    public void ErrorSearch()
    {
        StopSearch();
    }

    public void StopSearch()
    {
        searching = false;

    }
    public void ClosePopup(ClickEvent ev)
    {
        this.gameObject.SetActive(false);

    }

    public void ExitGame(ClickEvent ev)
    {
        Debug.Log("Exit button clicked");
    }

    public void CreateScene(ClickEvent ev)
    {
        if (startingCreation) return;
        startingCreation = true;
        if (found && lastSearch == placeInput.value)
        {
            RunCreation();
        }
        else
        {
            if (!searching) OsmConnector.Instance.Search(placeInput.value, ShowSearchResult, ErrorSearch);
        }
    }

    public void RunCreation()
    {

        var size = (int)(slider.value*100)/100f;
        GamePlayManager.Instance.CreateCity(lat, lng, size);
        this.gameObject.SetActive(false);
        startingCreation = false;

    }
}
