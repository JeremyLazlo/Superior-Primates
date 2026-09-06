using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    public float playerSpeed = 20f;
    private CharacterController myCC;

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
    }

    void GetInput() // Get input from the keyboard and calculate the movement vector
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current.aKey.isPressed)
            horizontal -= 1f;

        if (Keyboard.current.dKey.isPressed)
            horizontal += 1f;

        if (Keyboard.current.sKey.isPressed)
            vertical -= 1f;

        if (Keyboard.current.wKey.isPressed)
            vertical += 1f;

        inputVector = new Vector3(horizontal, 0f, vertical);

        inputVector.Normalize();
        inputVector = transform.TransformDirection(inputVector);

        movementVector = (inputVector * playerSpeed) + (Vector3.up * myGravity);
    }

    void MovePlayer()
    {
        myCC.Move(movementVector * Time.deltaTime); // Move the player based on the calculated movement vector and deltaTime
    }
}
