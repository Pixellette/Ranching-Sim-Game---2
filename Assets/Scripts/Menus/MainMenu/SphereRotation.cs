using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereRotation : MonoBehaviour
{
    public float rotationSpeed = 1f;
    void Update()
    {
    float rotationAmount = rotationSpeed * Time.deltaTime;
    transform.Rotate(Vector3.up, rotationAmount, Space.Self);
    }
}
