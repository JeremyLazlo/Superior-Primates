using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    public float sensitivity = 1f;
    public float smoothing = 1.5f;

    private float xMousePos;
    private float yMousePos;

    private float xSmoothMousePos;
    private float ySmoothMousePos;

    private float verticalLookPosition;
    private Transform playerBody;

    void Start()
    {
        // The Main Camera's parent should be the Player.
        playerBody = transform.parent;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        GetInput();
        ModifyInput();
        MovePlayer();
    }

    void GetInput() // Get input from the mouse and calculate the movement vector
    {
        if (Mouse.current == null)
            return;

        xMousePos = Mouse.current.delta.x.ReadValue();
        yMousePos = Mouse.current.delta.y.ReadValue();
    }

    void ModifyInput() // Modify the input based on sensitivity and smoothing
    {
        xMousePos *= sensitivity;
        yMousePos *= sensitivity;

        xSmoothMousePos = Mathf.Lerp(
            xSmoothMousePos,
            xMousePos,
            1f / smoothing
        );

        ySmoothMousePos = Mathf.Lerp(
            ySmoothMousePos,
            yMousePos,
            1f / smoothing
        );
    }

    void MovePlayer()
    {
        // Turn the Player left and right.
        playerBody.Rotate(Vector3.up * xSmoothMousePos);

        // Look up and down with the camera.
        verticalLookPosition -= ySmoothMousePos;

        // Clamp the vertical look position to prevent the camera from flipping over.
        verticalLookPosition = Mathf.Clamp(
            verticalLookPosition,
            -90f,
            90f
        );

        // Apply the vertical look position to the camera's local rotation.
        transform.localRotation =
            Quaternion.Euler(verticalLookPosition, 0f, 0f);
    }
}