using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraRotation : MonoBehaviour
{
    public float sensetivity;

    private float yRotation;
    private float xRotation;

    public Transform orientation;
    void Start() 
    {

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }

    // Update is called once per frame
    void Update()
    {
        Look();
    }

    public void Look()
    {
        float MouseX = Input.GetAxisRaw("Mouse X") * Time.fixedDeltaTime * sensetivity;
        float MouseY = Input.GetAxisRaw("Mouse Y") * Time.fixedDeltaTime * sensetivity;

        xRotation -= MouseY;
        yRotation += MouseX;
        xRotation = Mathf.Clamp(xRotation,-90f,90f);

        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);

    }
}
