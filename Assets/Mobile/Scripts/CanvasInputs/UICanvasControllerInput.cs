using UnityEngine;

namespace StarterAssets
{
    public class UICanvasControllerInput : MonoBehaviour
    {

        [Header("Output")]
        public StarterAssetsInputs starterAssetsInputs;
        public IReciveInputs output;



        public RectTransform jumpButton;



        public void VirtualMoveInput(Vector2 virtualMoveDirection)
        {
            if (output != null)
            {
                output.MoveInput(virtualMoveDirection);
            }
            else
            {
                starterAssetsInputs.MoveInput(virtualMoveDirection);
            }
        }

        public void VirtualLookInput(Vector2 virtualLookDirection)
        {
            if (output != null)
            {
                output.LookInput(virtualLookDirection);
            }
            else
                starterAssetsInputs.LookInput(virtualLookDirection);
        }

        public void VirtualJumpInput(bool virtualJumpState)
        {
            if (output != null)
            {
                output.JumpInput(virtualJumpState);
            }
            else
                starterAssetsInputs.JumpInput(virtualJumpState);
        }

        public void VirtualSprintInput(bool virtualSprintState)
        {
            if (output != null)
            {
                output.SprintInput(virtualSprintState);
            }
            else
                starterAssetsInputs.SprintInput(virtualSprintState);
        }
        
        public void ToggleJump(bool visible)
        {
            if(jumpButton != null)
            {
                jumpButton.gameObject.SetActive(visible);
            }
        }
    }

}
