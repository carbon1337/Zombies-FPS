using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractDoor : MonoBehaviour, IInteractable
{
    public bool isInteractable = true;
    public int doorCost = 500;

    private Animator animator;
    private CanvasGroup interactCanvas;
    private TMP_Text interactText;
    public float maxDistance = 12f;
    public bool isDestroyable = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
        interactCanvas = GetComponentInChildren<CanvasGroup>();
        interactText = GetComponentInChildren<TMP_Text>();
        animator.enabled = false;
    }

    void Start()
    {
        interactText.text = "E to purchase: $" + doorCost.ToString();
    }

    void Update()
    {
        float distance = Mathf.Min(Vector3.Distance(Camera.main.transform.position, transform.position), maxDistance);

        if(isInteractable)
        {
            interactCanvas.alpha = 1 - (distance/maxDistance);
        }
    }

    public void Interact()
    {
        if(isInteractable)
        {
            Debug.Log("Interacting " + this.ToString());

            //check if you have enough money
            if(GameManager.Instance.currentMoney >= doorCost)
            {
                isInteractable = false;

                //play door animation
                animator.enabled = true;


                //subtract money
                GameManager.Instance.SpendMoney(doorCost);
                interactCanvas.alpha = 0;

                if(isDestroyable)
                {
                    Destroy(gameObject, 2f);
                }

            }
        }
    }
}
