using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UnitySideScripts.MouseScripts
{
    class HighwayMouseHandler : MonoBehaviour, IPointerClickHandler, IClicked
    {
        public MouseActions actionhandler;

        public void Start()
        {
            actionhandler = FindObjectOfType<MouseActions>();// GameObject.Find("MouseAction").GetComponent<MouseActions>();
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                string highwayID = transform.name.Substring("Highway".Length);
                actionhandler.clickAction(MouseActions.objectType.highway, transform.gameObject, highwayID);
            }
        }

        public void onClick()
        {
            string highwayID = transform.name.Substring("Highway".Length);
            actionhandler.clickAction(MouseActions.objectType.highway, transform.gameObject, highwayID);
        }
    }
}
