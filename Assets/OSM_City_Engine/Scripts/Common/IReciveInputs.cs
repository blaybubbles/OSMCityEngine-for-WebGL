using UnityEngine;

public interface IReciveInputs
{
    void JumpInput(bool virtualJumpState);
    void LookInput(Vector2 virtualLookDirection);
    void MoveInput(Vector2 virtualMoveDirection);
    void SprintInput(bool virtualSprintState);
}
