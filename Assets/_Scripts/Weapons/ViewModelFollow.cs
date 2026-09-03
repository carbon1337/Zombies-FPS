using UnityEngine;

public class ViewModelFollow : MonoBehaviour
{
    [SerializeField] private Transform playerCamera;

    [Range(0f, 1f)]
    [SerializeField] private float pitchFollow = 0.15f;

    private Quaternion initialLocalRotation;

    private void Start()
    {
        initialLocalRotation = transform.localRotation;
    }

    private void LateUpdate()
    {
        float pitch = playerCamera.localEulerAngles.x;

        if (pitch > 180f)
            pitch -= 360f;

        transform.localRotation =
            initialLocalRotation *
            Quaternion.Euler(pitch * pitchFollow, 0f, 0f);
    }
}