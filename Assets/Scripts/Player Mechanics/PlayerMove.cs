using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    public float playerSpeed = 4f;
public float momentumDamping = 5f; // Damping factor for momentum

    private CharacterController myCC;
    public Animator camAnim; // Reference to the Animator component for the camera
    private bool isWalking;

    private Vector3 inputVector;
    private Vector3 movementVector;
    private float myGravity = -10f;

    void Start()
    {
        myCC = GetComponent<CharacterController>();
    }

    void Update()
    {
        GetInput();
        MovePlayer();

        camAnim.SetBool("isWalking", isWalking); // Set the isWalking parameter in the Animator based on the isWalking variable
    }

    void GetInput() // Get input from the keyboard and calculate the movement vector
    {
        if (Keyboard.current == null) // Check if the keyboard is available
            return;

        float horizontal = 0f;
        float vertical = 0f;

        // Check for keyboard input and adjust the horizontal and vertical values accordingly
        if (Keyboard.current.aKey.isPressed)
            horizontal -= 1f;

        if (Keyboard.current.dKey.isPressed)
            horizontal += 1f;

        if (Keyboard.current.sKey.isPressed)
            vertical -= 1f;

        if (Keyboard.current.wKey.isPressed)
            vertical += 1f;

        Vector3 keyboardInput = new Vector3(horizontal, 0f, vertical); // Create a vector based on the keyboard input

        if (keyboardInput.sqrMagnitude > 0f) // If there is any input detected, normalize the vector and transform it to world space
        {
            keyboardInput.Normalize();
            inputVector = transform.TransformDirection(keyboardInput);
            isWalking = true;
        }
        else // If no input is detected, apply momentum damping to gradually reduce the movement vector
        {
            inputVector = Vector3.Lerp(inputVector, Vector3.zero, momentumDamping * Time.deltaTime);
            isWalking = false; // Set isWalking to false when no movement keys are pressed
        }

        movementVector = (inputVector * playerSpeed) + (Vector3.up * myGravity);
    }

    void MovePlayer()
    {
        myCC.Move(movementVector * Time.deltaTime); // Move the player based on the calculated movement vector and deltaTime
    }
}