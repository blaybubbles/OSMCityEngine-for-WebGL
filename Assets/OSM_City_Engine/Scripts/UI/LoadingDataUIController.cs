using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class LoadingDataUIController : MonoBehaviour
{
    public Label label;
    public UIDocument document;

    private void OnEnable()
    {
        if (document == null)
        {
            document = GetComponent<UIDocument>();
        }

        if (document == null)
        {
            Debug.LogError("No UIDocument found");
            return;
        }
        label = document.rootVisualElement.Q<Label>("info");
    }
    public void SetText(string text)
    {
        if(label != null)
        {
            label.text = text;
        }
    }
}
