using Assets.Scripts.ConfigHandler;
using Assets.Scripts.OpenStreetMap;
using Assets.Scripts.SceneObjects;
using System.Collections.Generic;
using UnityEngine;

public class CityScene : MonoBehaviour
{
    public float lat;
    public float lng;
    public BBox scenebbox;
    public Vector2 centerMercator;
    public Vector2 centerScene;
    public myTerrain terrain;

    public List<Highway> highwayList = new List<Highway>();
    public List<Pavement> pavementList = new List<Pavement>();
    public List<Building> buildingList = new List<Building>();
    public List<Barrier> barrierList = new List<Barrier>();
    public List<Object3D> defaultObject3DList = new List<Object3D>();
    public List<Object3D> object3DList = new List<Object3D>();
    public InitialConfigurations config;
    internal HighwayModeller highwayModeller;

}
