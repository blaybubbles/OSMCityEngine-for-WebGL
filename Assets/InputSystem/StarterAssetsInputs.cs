using Assets.Scripts.UnitySideScripts.MouseScripts;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using static UnityEngine.EventSystems.PointerEventData;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour, IReciveInputs
    {
        private const int DEFAULT_GUI_LAYERS = 1 << 5;

        [Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;
		public bool canLock = false;
        public GameObject CinemachineCameraTarget;

        private void OnEnable()
        {
            CinemachineCameraTarget.gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            CinemachineCameraTarget.gameObject.SetActive(false);
        }

#if ENABLE_INPUT_SYSTEM
        public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}

        public void OnViewChange(InputValue value)
        {
            if (value.isPressed)
            {
                GamePlayManager.Instance.SetFlyCamera();
            }
        }
#endif


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
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked && canLock);
		}

		private void SetCursorState(bool newState)
		{
			//if (canLock)
			{
                Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;

            }
        }

        public void OnPointerClick(InputValue value)
        {
			Debug.Log("Pointer");

			Vector3 screenPosition = Mouse.current.position.ReadValue();
            if(RaycastGui(screenPosition, (LayerMask)DEFAULT_GUI_LAYERS).Count > 0) return;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(screenPosition), out RaycastHit hit))
			{
				var clicked = hit.collider.GetComponent<IClicked>();
				if (clicked != null)
				{
					clicked.onClick();
				}
			}

		}
        private static List<RaycastResult> tempRaycastResults = new List<RaycastResult>(10);
        private static EventSystem tempEventSystem;
        private static PointerEventData tempPointerEventData;

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
                            tempRaycastResults.RemoveAt(i);
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
    }
	
}