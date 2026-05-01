using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardFX : MonoBehaviour
{
	private Transform camTransform;

	Quaternion originalRotation;

    void Awake()
    {
        camTransform = Camera.main.transform;
    }

    void Start()
    {
        originalRotation = transform.rotation;
    }

    void Update()
    {
     	transform.rotation = camTransform.rotation * originalRotation;   
    }
}