using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    public float sensitivity = 1.5f;
    public float smoothing = 1.5f;

    private float xMousePos;
    private float smoothMousePos;

    private float currentLookPos;

    void Start() // lock and hide cursor
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        GetInput();
        ModifyInput(); 
        MovePlayer();
    }

    void GetInput()
    {
        xMousePos = Mouse.current.delta.x.ReadValue();
    }

    void ModifyInput()
    {
        xMousePos *= sensitivity * smoothing;
        smoothMousePos = Mathf.Lerp(smoothMousePos, xMousePos, 1f/smoothing);
    }

    void MovePlayer()
    {
        currentLookPos += smoothMousePos * sensitivity;
        transform.localRotation = Quaternion.AngleAxis(currentLookPos, Vector3.up);
    }
}