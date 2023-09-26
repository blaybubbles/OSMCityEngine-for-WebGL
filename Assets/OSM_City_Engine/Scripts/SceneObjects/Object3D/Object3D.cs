using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.SceneObjects
{
    [Serializable]
    public enum ObjectType
    {
        Car,
        Tree,
        TrafficSign,
        Wall,
        Default,
        External
    }

    [Serializable]
    public class Object3D
    {
        public string id, name,resourcePath;
        public ObjectType type;
        public GameObject object3D;
    }
}
