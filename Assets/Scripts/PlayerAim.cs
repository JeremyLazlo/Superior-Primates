using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAim : MonoBehaviour
{
    public Animator shotgunAnimator;
    public Camera playerCamera;
    public MouseLook mouseLook;

    // FOV and sensitivity settings for aiming
    public float normalFOV = 60f;
    public float aimFOV = 45f;

    // Sensitivity settings for aiming
    public float normalSensitivity = 1f;
    public float aimSensitivity = 0.5f;

    public float zoomSpeed = 10f;

    private bool isAiming;

    void Update()
    {
        GetInput();
        Aim();
    }

    void GetInput()
    {
        if (Mouse.current == null)
            return;

        isAiming = Mouse.current.rightButton.isPressed;
    }

    // Aim method to handle aiming logic
    void Aim()
    {
        shotgunAnimator.SetBool("Aim", isAiming);

        // Adjust the camera's field of view and mouse sensitivity based on whether the player is aiming or not
        if (isAiming)
        {
            playerCamera.fieldOfView = Mathf.Lerp(
                playerCamera.fieldOfView,
                aimFOV,
                zoomSpeed * Time.deltaTime
            );

            mouseLook.sensitivity = aimSensitivity;
        }
        else
        {
            playerCamera.fieldOfView = Mathf.Lerp(
                playerCamera.fieldOfView,
                normalFOV,
                zoomSpeed * Time.deltaTime
            );

            mouseLook.sensitivity = normalSensitivity;
        }
    }
}