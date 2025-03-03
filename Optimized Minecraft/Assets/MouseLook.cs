using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public short selectedCube = 56;
    public float mouseSensitivity = 100f;
    public Transform playerBody;
    float xRotation = 0f;
    Vector3 pos;

    // Start is called before the first frame update
    void Start()
    {
        LockMouse();
    }

    public void LockMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void UnLockMouse()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(pos, .25f);
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerUI.instance.inUI) { return; }
        float MouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float MouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= MouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * MouseX);

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, 5f))
            {
                pos = hit.point;
                Chunk targetChunk = hit.collider.gameObject.GetComponent<Chunk>();
                if (targetChunk != null)
                {
                    targetChunk.ProcessRayCast(hit, false);
                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, 5f))
            {
                pos = hit.point;
                Chunk targetChunk = hit.collider.gameObject.GetComponent<Chunk>();
                if (targetChunk != null)
                {
                    targetChunk.ProcessRayCast(hit, true);
                }
            }
        }
    }
}
