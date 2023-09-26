using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    public UIDocument document;
    public Button createProjectBtn;
    public Button loadProjectBtn;
    public Button saveProjectBtn;

    public UIDocument createObjectPanel;
    public UIDocument loadObjectPanel;


    public int state = 1;
    private Button exitBtn;
    private Button cameraBtn;

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

        createProjectBtn = document.rootVisualElement.Q<Button>("createproject");
        if (createProjectBtn != null)
        {
            createProjectBtn.RegisterCallback<ClickEvent>(ToogleCreatePanel);
        }

        loadProjectBtn = document.rootVisualElement.Q<Button>("loadproject");
        if (loadProjectBtn != null)
        {
            //register a callback for the button
            loadProjectBtn.RegisterCallback<ClickEvent>(ToogleLoadPanel);
        }

        saveProjectBtn = document.rootVisualElement.Q<Button>("saveproject");
        if (saveProjectBtn != null)
        {
            saveProjectBtn.RegisterCallback<ClickEvent>(OpenSavePanel);
        }


        exitBtn = document.rootVisualElement.Q<Button>("exit");
        if (exitBtn != null)
        {
            exitBtn.RegisterCallback<ClickEvent>(Exit);
        }

        cameraBtn = document.rootVisualElement.Q<Button>("cameraBtn");
        if (cameraBtn != null)
        {
            cameraBtn.RegisterCallback<ClickEvent>(ChangeView);
        }
    }

    private void OnDisable()
    {
        //createObjectPanel.gameObject.SetActive(false);
    }


    private void Exit(ClickEvent evt)
    {
    }

    private void OpenSavePanel(ClickEvent evt)
    {
    }

    private void ToogleLoadPanel(ClickEvent evt)
    {
    }

    public void ToogleCreatePanel(ClickEvent clickEvent)
    {
        if (createObjectPanel.gameObject.activeSelf)
        {
            HideAllPanel();
        }
        else
        {
            ShowCreatePanel();
        }
    }

    public void ShowCreatePanel()
    {
        createObjectPanel.gameObject.SetActive(true);
    }

    public void HideAllPanel()
    {
        createObjectPanel.gameObject.SetActive(false);
    }

    public void SetDisabledSave(bool disabled)
    {
        saveProjectBtn.SetEnabled(!disabled);  
    }

    private void ChangeView(ClickEvent evt)
    {
        GamePlayManager manager = GamePlayManager.Instance;
        if (manager.cameraType == CameraBehaviour.PersonCamera)
        {
            cameraBtn.RemoveFromClassList("person-btn");
            cameraBtn.AddToClassList("fly-btn");
            manager.SetFlyCamera();
        }
        else
        {
            cameraBtn.RemoveFromClassList("fly-btn");
            cameraBtn.AddToClassList("person-btn");
            manager.SetPersonCamera();
        }
    }
}
