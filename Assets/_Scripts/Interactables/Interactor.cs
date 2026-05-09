using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

interface IInteractable  
{
    public void Interact();
}

public class Interactor : MonoBehaviour
{
    public Transform interactionSource;
    public float interactRange;

    private InputAction interactAction;
    private PlayerInput playerInput;

    void Awake()
    {
        interactionSource = Camera.main.transform;

        playerInput = GetComponent<PlayerInput>();

        interactAction = playerInput.actions["Interact"];
    }

    // Update is called once per frame
    void Update()
    {
        if(interactAction.IsPressed())
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
