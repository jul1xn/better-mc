using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement instance;
    public MouseLook mouseLook;
    public CharacterController controller;

    public float speed = 12f;
    public float gravity = -9.81f;
    public float jump = 1f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    public Vector3 velocity;
    bool isGrounded;
    bool sprinting;
    bool flymode;

    public bool groundGenerated;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        groundGenerated = false;
        flymode = false;
    }

    public void ToggleFly()
    {
        flymode = !flymode;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
    }

    public void TeleportToPosition(Vector3 targetPosition)
    {
        controller.enabled = false;
        transform.position = targetPosition;
        controller.enabled = true;
    }

    void Update()
    {
        if (flymode)
        {
            Fly();
        }
        else
        {
            Movement();
        }
    }

    private void Fly()
    {
        float x = 0f;
        float z = 0f;
        float y = 0f;
        float multiplier = 3f;

        if (Input.GetKey(KeyCode.A)) x = -1f;
        if (Input.GetKey(KeyCode.D)) x = 1f;
        if (Input.GetKey(KeyCode.W)) z = 1f;
        if (Input.GetKey(KeyCode.S)) z = -1f;

        if (Input.GetKey(KeyCode.Space)) y = 1f;      // Fly up
        if (Input.GetKey(KeyCode.LeftControl)) y = -1f; // Fly down

        sprinting = Input.GetKey(KeyCode.LeftShift);
        if (sprinting)
        {
            multiplier = 4.5f;
        }

        if (PlayerUI.instance.inUI || PlayerUI.instance.gamePaused)
        {
            x = 0;
            z = 0;
            y = 0;
        }

        Vector3 move = transform.right * x + transform.forward * z + transform.up * y;

        if (move.magnitude > 1)
        {
            move.Normalize();
        }

        controller.Move(move * speed * multiplier * Time.deltaTime);
    }


    private void Movement()
    {
        if (!groundGenerated)
        {
            if (Physics.Raycast(transform.position, Vector3.down))
            {
                groundGenerated = true;
            }

            return;
        }

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = 0f;
        float z = 0f;
        float multiplier = 1;

        if (Input.GetKey(KeyCode.A)) x = -1f; // Move left
        if (Input.GetKey(KeyCode.D)) x = 1f;  // Move right
        if (Input.GetKey(KeyCode.W)) z = 1f;  // Move forward
        if (Input.GetKey(KeyCode.S)) z = -1f; // Move backward
        sprinting = Input.GetKey(KeyCode.LeftShift);

        if (sprinting)
        {
            multiplier = 1.3f;
        }

        if (PlayerUI.instance.inUI || PlayerUI.instance.gamePaused)
        {
            x = 0;
            z = 0;
        }


        Vector3 move = transform.right * x + transform.forward * z;

        if (move.magnitude > 1)
        {
            move.Normalize();
        }

        controller.Move(move * (speed * multiplier) * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded && !PlayerUI.instance.inUI && !PlayerUI.instance.gamePaused)
        {
            velocity.y = Mathf.Sqrt(jump * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
}
