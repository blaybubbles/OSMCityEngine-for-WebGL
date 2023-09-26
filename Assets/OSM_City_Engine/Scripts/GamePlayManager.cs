using StarterAssets;
using UnityEngine;

public class GamePlayManager : Singleton<GamePlayManager>
{
    public MainMenuController mainMenuController;
    public LoadingDataUIController loadingDataUI;
    public GameObject personUI;

    public CityConstructor constructor;

    public CameraBehaviour cameraType = CameraBehaviour.FreeCamera;
    public StarterAssetsInputs person;
    public FreeFlyTarget freeCamera;

    public IReciveInputs currentCamera;


    public GameState gameState;

    public UICanvasControllerInput uiControllerInput;

    public bool terrainCreated;
    public bool CityExist => terrainCreated;
    private void OnEnable()
    {
        if (constructor.city == null)
        {
            mainMenuController.ShowCreatePanel();
        }

        if (uiControllerInput == null)
            uiControllerInput = FindObjectOfType<UICanvasControllerInput>();
        ChangeCamera();

    }


    public void CreateCity(float lat, float lng, float size, string preset = "default")
    {
        mainMenuController.HideAllPanel();
        CityConstructor.Instance.CreateScene(lat, lng, size);
        if(size >= 2)
        {
            freeCamera.VirtualCamera.m_Lens.FarClipPlane += 1000*size/2; 
        }
        loadingDataUI.gameObject.SetActive(true);

        loadingDataUI.SetText("Load data");

        gameState = GameState.Creating_City;
    }

    public void ChangeConstructionStatus(string text)
    {
        loadingDataUI.SetText(text);
    }

    public void OnTerrainCreated()
    {
        loadingDataUI.SetText("Terrain created");
        var center = constructor.CenterPointOnTerrain();

        person.gameObject.SetActive(false);
        person.transform.position = center;

        FlyCameraLookAt(center, new Vector3(500, -500, 500));
        SetFlyCamera();
        terrainCreated = true;
      
    }

    private void FlyCameraLookAt(Vector3 center, Vector3 offset)
    {
        freeCamera.transform.position = center - offset;
        // calculate quaternion rotation to look at point
        var q = Quaternion.LookRotation(center - freeCamera.transform.position);

        freeCamera.SetRotation(q);
    }

    private Vector3 GetFlyCameraLookAt()
    {
        if (Physics.Raycast(freeCamera.transform.position, freeCamera.transform.forward, out RaycastHit hit, 10000f))
        {
            return hit.point;
        }
        else
        {
            return freeCamera.transform.position;
        }
    }

    public void CityCreated()
    {
        loadingDataUI.gameObject.SetActive(true);
        loadingDataUI.SetText("City completed");
        gameState = GameState.Creating_City;
    }

    public void DestroyCity()
    {
        CityConstructor.Instance.CleanCityData();
    }

    [ContextMenu("Set Fly Camera")]
    public void SetFlyCamera()
    {
        cameraType = CameraBehaviour.FreeCamera;
        ChangeCamera();
    }

    [ContextMenu("Set Person Camera")]
    public void SetPersonCamera()
    {
        cameraType = CameraBehaviour.PersonCamera;
        ChangeCamera();
    }

    public void ChangeCamera()
    {
        person.gameObject.SetActive(false);
        freeCamera.gameObject.SetActive(false);
        person.canLock = false;
        personUI.SetActive(false);
        mainMenuController.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;

        if (cameraType == CameraBehaviour.FreeCamera)
        {
            currentCamera = freeCamera;
            if (CityExist)
            {
                FlyCameraLookAt(person.transform.position, new Vector3(200, -200, 200));
            }

            freeCamera.gameObject.SetActive(true);
        }
        else
        {
            currentCamera = person;

            if (CityExist)
            {
                person.transform.position = GetFlyCameraLookAt();
                person.canLock = true;
                Cursor.lockState = CursorLockMode.Locked;
                personUI.SetActive(true);
                mainMenuController.gameObject.SetActive(false);
            }


            person.gameObject.SetActive(true);
        }

        if (uiControllerInput != null)
            uiControllerInput.output = currentCamera;
    }

    private void SetCameraPosition(Vector3 vector3)
    {

        if (cameraType == CameraBehaviour.FreeCamera)
        {
            if (freeCamera != null)
            {
                freeCamera.gameObject.SetActive(false);

                freeCamera.transform.position = vector3 - new Vector3(10, 10, 10);
                // set rotation to look at point
                freeCamera.transform.LookAt(vector3);

                freeCamera.gameObject.SetActive(true);
            }
        }
        else
        {
            if (person != null)
            {
                person.gameObject.SetActive(false);
                person.transform.position = vector3;
                person.gameObject.SetActive(true);
            }
        }
    }
}


public enum GameState
{
    Empty,
    Creating_City,
    City,
    Saving_City,
}

public enum CameraBehaviour
{
    FreeCamera,
    PersonCamera
}