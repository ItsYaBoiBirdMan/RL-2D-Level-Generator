using UnityEngine;
using UnityEngine.InputSystem;

public class InputTest : MonoBehaviour
{
    public InputActionReference jumpAction;

    private void OnEnable()
    {
        jumpAction.action.Enable();
        jumpAction.action.performed += ctx => Debug.Log("Jump pressed!");
    }

    private void OnDisable()
    {
        jumpAction.action.performed -= ctx => Debug.Log("Jump pressed!");
        jumpAction.action.Disable();
    }
}