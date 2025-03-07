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

    public bool groundGenerated;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        groundGenerated = false;
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

        if (Input.GetKey(KeyCode.A)) x = -1f; // Move left
        if (Input.GetKey(KeyCode.D)) x = 1f;  // Move right
        if (Input.GetKey(KeyCode.W)) z = 1f;  // Move forward
        if (Input.GetKey(KeyCode.S)) z = -1f; // Move backward

        if (PlayerUI.instance.inUI)
        {
            x = 0;
            z = 0;
        }


        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded && !PlayerUI.instance.inUI)
        {
            velocity.y = Mathf.Sqrt(jump * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
}
