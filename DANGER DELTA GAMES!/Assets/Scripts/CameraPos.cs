using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPos : MonoBehaviour
{
    public Transform camPosition;
    void LateUpdate()
    {
        transform.position = camPosition.position;
    }
}
