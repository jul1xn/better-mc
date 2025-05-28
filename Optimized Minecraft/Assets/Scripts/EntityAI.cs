using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EntityAI : MonoBehaviour
{
    public State currentState;
    public float maxStandingTime;
    public float randomPosRange = 5f;
    public float destinationRange = 0.1f;
    public float forwardCheckRange = 0.5f;
    public float headHeight = 0.5f;
    [Space]
    public float movementSpeed = 3f;
    public float gravity = -9.81f;
    public float jump = 1f;
    public Transform gcTransform;
    public LayerMask gcLayer;
    public float gcDistance = 0.4f;
    private bool isGrounded;
    private Vector3 velocity;

    private CharacterController controller;
    private Vector3 targetPosition;
    private bool isWalking;
    private float tickTime;
    private bool wantsToJump;

    private void Start()
    {
        currentState = State.Standing;
        wantsToJump = false;
        controller = GetComponent<CharacterController>();
        tickTime = Time.time + Random.Range(0, maxStandingTime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Vector3 targetPos = transform.position;
        targetPos.y += headHeight;

        Gizmos.DrawRay(targetPos, transform.forward * forwardCheckRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(targetPosition, 0.5f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(gcTransform.position, gcDistance);
    }

    private void Update()
    {
        Physic();

        if (isWalking)
        {
            if (Vector3.Distance(transform.position, targetPosition) < destinationRange)
            {
                isWalking = false;
                return;
            }

            Vector3 direction = targetPosition - transform.position;
            direction.y = 0f;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = targetRotation;
            }

            Vector3 move = transform.forward * movementSpeed * Time.deltaTime;
            controller.Move(move);
        }

        if (currentState == State.Standing && Time.time > tickTime)
        {
            tickTime = Time.time + Random.Range(0, maxStandingTime);
            float x = Mathf.RoundToInt(Random.Range(-randomPosRange, randomPosRange) + transform.position.x);
            float z = Mathf.RoundToInt(Random.Range(-randomPosRange, randomPosRange) + transform.position.z);

            float noiseValue = WorldGen.GetNoise(WorldGen.instance.seed, x * WorldGen.instance.noiseScale, z * WorldGen.instance.noiseScale);
            int height = Mathf.RoundToInt(noiseValue * WorldGen.maxHeight);

            Vector3 target = new Vector3(x, height + 1, z);

            WalkToPosition(target);
        }
    }

    private void Physic()
    {
        Vector3 wawPos = transform.forward * forwardCheckRange;
        wawPos.y += headHeight;

        bool walkingAgainstWall = Physics.CheckSphere(wawPos, 0.2f, gcLayer);
        if (walkingAgainstWall && !wantsToJump)
        {
            wantsToJump = true;
        }

        isGrounded = Physics.CheckSphere(gcTransform.position, gcDistance, gcLayer);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (wantsToJump && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jump * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void WalkToPosition(Vector3 pos)
    {
        targetPosition = pos;
        isWalking = true;
    }

    public enum State { Standing, Wandering, TargetingPlayer, Panic };
}