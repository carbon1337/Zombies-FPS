using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    private Camera playerCam;
    public float cameraYOffset = 4f;
    public float cameraZOffset = 0.8f;

    public float walkSpeed = 6f;
    public float runMaxSpeed = 12f;
    public float jumpPower = 7f;
    private float jumpDamping;
    public float gravity = 10f;

    public float accelSpeed;
    public float deccelSpeed;

    private float maxJumpStamina = 100f;
    [SerializeField]
    public float currentJumpStamina;
    public float jumpStaminaCooldown = 2f;

    public float lookSens = 2f;
    public float xLookClamp = 45f;

    public Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    public bool canMove = true;
    public bool isRunning = false;

    public CharacterController characterController;
    Animator animator;

    private bool canPlayLandAudio = true;
    
    public List<AudioClip> m_FootstepSounds = new List<AudioClip>();
    public List<AudioClip> m_jumpVoices = new List<AudioClip>();
    public List<AudioClip> m_landVoices = new List<AudioClip>();
    private AudioClip m_jumpSound;
    private AudioClip m_landSound;
    private AudioSource m_actionAudioSource;
    private AudioSource m_voiceAudioSource;
    private FootstepSwapper swapper;
    private float timeSinceLastFootstep = 0f;
    public float timeBetweenFootsteps = 0.6f; // Adjust this value to control the frequency of footsteps
    private bool wasGrounded;

    // Start is called before the first frame update
    void Awake() 
    {
        playerCam = Camera.main;
        playerCam.transform.position = new Vector3(transform.position.x, transform.position.y + cameraYOffset, transform.position.z + cameraZOffset);
        playerCam.transform.SetParent(transform);
        characterController = GetComponent<CharacterController>();
        // = transform.Find("PlayerBody").GetComponentInChildren<Animator>();

        m_actionAudioSource = GetComponent<AudioSource>();
        m_voiceAudioSource = GetComponentInChildren<AudioSource>();
        swapper = GetComponent<FootstepSwapper>();
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        accelSpeed = walkSpeed;
        currentJumpStamina = maxJumpStamina;
    }

    public void PlayFootstepsAudio()
    {
        swapper.CheckLayers();
        if (!characterController.isGrounded)
        {
            return;
        }

        float currentTimeBetweenFootsteps = timeBetweenFootsteps;

        if(isRunning)
        {
            currentTimeBetweenFootsteps = 0.35f;
        }
        else
        {
            currentTimeBetweenFootsteps = timeBetweenFootsteps;
        }

        // Check if enough time has passed since the last footstep
        if (Time.time - timeSinceLastFootstep > currentTimeBetweenFootsteps)
        {
            int n = Random.Range(1, m_FootstepSounds.Count);
            m_actionAudioSource.clip = m_FootstepSounds[n];
            m_actionAudioSource.PlayOneShot(m_actionAudioSource.clip);

            // Swap the audio clips for variation
            m_FootstepSounds[n] = m_FootstepSounds[0];
            m_FootstepSounds[0] = m_actionAudioSource.clip;

            // Update the time of the last footstep
            timeSinceLastFootstep = Time.time;
        }
    }

    public void PlayJumpAudio()
    {
        swapper.CheckLayers();

        m_actionAudioSource.clip = m_jumpSound;
        m_actionAudioSource.PlayOneShot(m_jumpSound);
    }

    public void PlayJumpVoicesAudio()
    {
        int n = Random.Range(1, m_jumpVoices.Count);

        m_voiceAudioSource.clip = m_jumpVoices[n];
        m_voiceAudioSource.PlayOneShot(m_voiceAudioSource.clip);

        // Swap the audio clips for variation
        m_jumpVoices[n] = m_jumpVoices[0];
        m_jumpVoices[0] = m_voiceAudioSource.clip;
    }

    public void PlayLandVoicesAudio()
    {
        int n = Random.Range(1, m_landVoices.Count);

        m_voiceAudioSource.clip = m_landVoices[n];
        m_voiceAudioSource.PlayOneShot(m_voiceAudioSource.clip);

        // Swap the audio clips for variation
        m_landVoices[n] = m_landVoices[0];
        m_landVoices[0] = m_voiceAudioSource.clip;
    }

    public void PlayLandAudio()
    {
        Debug.Log("Landed");
        swapper.CheckLayers();

        if(!canPlayLandAudio)
        {
            return;
        }

        m_actionAudioSource.clip = m_landSound;
        m_actionAudioSource.PlayOneShot(m_landSound);
        StartCoroutine("LandAudioCooldown");

    }

    public void SwapFootsteps(FootstepCollection collection)
    {
        m_FootstepSounds.Clear();

        for(int i = 0; i < collection.footstepSounds.Count; i++)
        {
            m_FootstepSounds.Add(collection.footstepSounds[i]);
        }

        m_jumpSound = collection.jumpSound;
        m_landSound = collection.landSound;
    }


    void Update()
    {
        // Movement logic
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        isRunning = Input.GetKey(KeyCode.LeftShift); //Old Input system 1

        if (isRunning && currentJumpStamina > 0f)
        {
            if (accelSpeed <= runMaxSpeed)
            {
                accelSpeed *= 1.006f;
            }
        }
        if (characterController.isGrounded)
        {
            StartCoroutine("ResetStamina");

            if ((!isRunning || currentJumpStamina <= 0f))
            {
                accelSpeed = walkSpeed; // Reset acceleration speed to walk speed
            }
        }

        if (!wasGrounded && characterController.isGrounded)
        {
            PlayLandVoicesAudio();
            PlayLandAudio();
        }

        wasGrounded = characterController.isGrounded;

        float inputX = Input.GetAxis("Vertical"); //Old input 2
        float inputY = Input.GetAxis("Horizontal"); //Old input 3

        //.SetFloat("horizontal", inputY);
        //.SetFloat("vertical", inputX);

        // Normalize the input vector to ensure consistent speed in all directions
        Vector2 inputVector = new Vector2(inputX, inputY).normalized;

        float curSpeedX = canMove ? accelSpeed * inputVector.x : 0;
        float curSpeedY = canMove ? accelSpeed * inputVector.y : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        // Jumping logic
        jumpDamping = currentJumpStamina / 100;

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded) //Old input 4
        {
            if (currentJumpStamina > 0)
            {
                PlayJumpVoicesAudio();
                PlayJumpAudio();
                StopCoroutine("ResetStamina");
                SubtractStamina(20f);
            }

            float currentJumpPower = jumpDamping * jumpPower;
            moveDirection.y = currentJumpPower;
        }
        else
        {
            moveDirection.y = movementDirectionY;
            //.SetBool("isJumping", true);
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

                // Call footstep audio method when moving and grounded
        if (characterController.isGrounded)
        {
            //.SetBool("isJumping", false);
            if(Mathf.Abs(curSpeedX) > 0 || Mathf.Abs(curSpeedY) > 0)
            {
                PlayFootstepsAudio();
            }

        }

        // Rotation logic
        characterController.Move(moveDirection * Time.deltaTime);

        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSens; //Old input 5
            rotationX = Mathf.Clamp(rotationX, -xLookClamp, xLookClamp);

            if (playerCam != null)
            {
                playerCam.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            }

            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSens, 0);
        }
    }

    private IEnumerator ResetStamina() 
    {
        yield return new WaitForSeconds(jumpStaminaCooldown);

        currentJumpStamina = maxJumpStamina;
        jumpDamping = 1f;
    }

    private IEnumerator LandAudioCooldown()
    {
        canPlayLandAudio = false;
        yield return new WaitForSeconds(0.2f);
        canPlayLandAudio = true;
    }

    private void SubtractStamina(float staminaToSubtract)
    {
        currentJumpStamina -= staminaToSubtract;
    }
}

