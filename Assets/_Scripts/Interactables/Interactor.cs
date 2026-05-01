using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IInteractable  
{
    public void Interact();
}

public class Interactor : MonoBehaviour
{
    public Transform interactionSource;
    public float interactRange;

    void Awake()
    {
        interactionSource = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            Ray r = new Ray(interactionSource.position, interactionSource.forward);
            if(Physics.Raycast(r, out RaycastHit hitInfo, interactRange))
            {
                if(hitInfo.collider.gameObject.TryGetComponent(out IInteractable interactObj))
                {
                    Debug.Log("hitraycast on obj");
                    interactObj.Interact();
                }
            }
        }
    }
}
