using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class FreeFlyTarget : MonoBehaviour, IReciveInputs
{
    private const int DEFAULT_GUI_LAYERS = 1 << 5;
    private const float _threshold = 0.01f;

    public Vector2 move;
    public Vector2 look;
    public bool sprint;

    [Header("Mouse Cursor Settings")]
    public bool cursorLocked = true;
    public bool cursorInputForLook = true;

    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;

    [Tooltip("For locking the camera position on all axis")]
    public bool LockCameraPosition = false;


    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;

    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    public PlayerInput _playerInput;

    public bool Clamp = false;
    public Vector3 boundsCenter = Vector3.zero;
    public Vector3 boundsSize = Vector3.one * 100f;

    public CinemachineVirtualCamera VirtualCamera;
    public GameObject CinemachineCameraTarget;

    private static List<RaycastResult> tempRaycastResults = new List<RaycastResult>(10);
    private static EventSystem tempEventSystem;
    private static PointerEventData tempPointerEventData;
    public float MoveSpeed = 2.0f;
    public float SprintSpeed = 100f;
    public float distanceScale = 0.1f;
    public bool lookWithRightClick = true;
    public bool rightClick;

    private void OnEnable()
    {
        CinemachineCameraTarget.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        CinemachineCameraTarget.gameObject.SetActive(false);
    }

    private void Start()
    {
        _playerInput = GetComponent<PlayerInput>();
    }
    public void OnMove(InputValue value)
    {
        MoveInput(value.Get<Vector2>());
    }

    public void OnLook(InputValue value)
    {
        if (cursorInputForLook)
        {
            LookInput(value.Get<Vector2>());
        }
    }
    public void OnSprint(InputValue value)
    {
        SprintInput(value.isPressed);
    }
    public void OnRightClick(InputValue value)
    {
        rightClick = value.isPressed;
        //Debug.Log(value.isPressed ? "pressed" : "not pressed");
    }
    public void OnPointerClick(InputValue value)
    {
        //Debug.Log("Pointer");

        Vector3 screenPosition = Mouse.current.position.ReadValue();
        if (RaycastGui(screenPosition, (LayerMask)DEFAULT_GUI_LAYERS).Count > 0) return;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(screenPosition), out RaycastHit hit))
        {
            var clicked = hit.collider.GetComponent<IClicked>();
            if (clicked != null)
            {
                clicked.onClick();
            }
        }

    }

    public void MoveInput(Vector2 newMoveDirection)
    {
        move = newMoveDirection;
    }

    public void LookInput(Vector2 newLookDirection)
    {
        look = newLookDirection;
    }

    public void JumpInput(bool newJumpState)
    {
        //jump = newJumpState;
    }

    public void SprintInput(bool newSprintState)
    {
        sprint = newSprintState;
    }

    public void OnWheel(InputValue value)
    {
        //Debug.Log($"wheel {value.Get<Vector2>()}");
        Zoom(value.Get<Vector2>().y);
    }


    public static List<RaycastResult> RaycastGui(Vector2 screenPosition, LayerMask layerMask)
    {
        tempRaycastResults.Clear();

        var currentEventSystem = EventSystem.current ?? FindObjectOfType<EventSystem>();

        if (currentEventSystem != null)
        {
            // Create point event data for this event system?
            if (currentEventSystem != tempEventSystem)
            {
                tempEventSystem = currentEventSystem;

                if (tempPointerEventData == null)
                {
                    tempPointerEventData = new PointerEventData(tempEventSystem);
                }
                else
                {
                    tempPointerEventData.Reset();
                }
            }

            // Raycast event system at the specified point
            tempPointerEventData.position = screenPosition;

            currentEventSystem.RaycastAll(tempPointerEventData, tempRaycastResults);

            // Loop through all results and remove any that don't match the layer mask
            if (tempRaycastResults.Count > 0)
            {
                for (var i = tempRaycastResults.Count - 1; i >= 0; i--)
                {
                    var raycastResult = tempRaycastResults[i];
                    var raycastLayer = 1 << raycastResult.gameObject.layer;

                    if ((raycastLayer & layerMask) == 0)
                    {
                        //tempRaycastResults.RemoveAt(i);
                    }
                }
            }


        }
        else
        {
            Debug.LogError("Failed to RaycastGui because your scene doesn't have an event system! To add one, go to: GameObject/UI/EventSystem");
        }

        return tempRaycastResults;
    }




    private void Move()
    {
        // set target speed based on move speed, sprint speed and if sprint is pressed
        float targetSpeed = sprint ? SprintSpeed : MoveSpeed;

        if (move == Vector2.zero)
        {
            return;
        }
        var direction = move.normalized;

        // var _targetRotation = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg + transform.eulerAngles.y;
        //Vector3 vector3 = Quaternion.Euler(0, _targetRotation, 0) * Vector3.forward * targetSpeed;
        //Debug.Log(" Move " + move * 100f);
        //Debug.Log($" Move {targetSpeed} " + vector3 * 100f);

        //transform.position += vector3 * Time.deltaTime;

        var dir = transform.TransformDirection(new Vector3(move.x, 0, move.y));//Vector3.forward* move.y);
        Vector3 delta = dir * targetSpeed * Time.deltaTime;
        var newposition = transform.position + delta;
        //transform.position += delta;

        var min = boundsCenter - boundsSize / 2;
        var max = boundsCenter + boundsSize / 2;

        if (Clamp)
        {
            newposition = new Vector3(Mathf.Clamp(newposition.x, min.x, max.x),
            Mathf.Clamp(newposition.y, min.y, max.y),
            Mathf.Clamp(newposition.z, min.z, max.z));
        }

        transform.position = newposition;

    }

    private void Zoom(float y)
    {
        return;
        //if(VirtualCamera == null) return;


        var dir = transform.TransformDirection(Vector3.back);
        var screen = Camera.main.WorldToScreenPoint(transform.position);

        var ray = Camera.main.ScreenPointToRay(screen);
        dir = ray.direction;
        //dir = (transform.position -Camera.main.transform.position).normalized;
        //transform.position += dir*y* distanceScale*Time.deltaTime;
        transform.position += Vector3.up * y * distanceScale * Time.deltaTime;
        //    CinemachineComponentBase componentBase = VirtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
        //if (componentBase is Cinemachine3rdPersonFollow)
        //{
        //    (componentBase as Cinemachine3rdPersonFollow).CameraDistance += distanceScale* y; // your value
        //}

        //if (componentBase is CinemachineFramingTransposer)
        //{
        //    (componentBase as CinemachineFramingTransposer).m_CameraDistance += distanceScale * y; // your value
        //}
    }

    private void Update()
    {
        Move();
    }
    private void LateUpdate()
    {
        if (look.sqrMagnitude >= _threshold && !LockCameraPosition && (rightClick || !lookWithRightClick))
        {
            //Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _cinemachineTargetYaw += look.x * deltaTimeMultiplier;
            _cinemachineTargetPitch += look.y * deltaTimeMultiplier;


        }

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
            _cinemachineTargetYaw, 0.0f);
    }
    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    internal void SetRotation(Quaternion q)
    {
        _cinemachineTargetYaw = q.eulerAngles.y;
        _cinemachineTargetPitch = q.eulerAngles.x;
    }

    private bool IsCurrentDeviceMouse
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
        }
    }
}
