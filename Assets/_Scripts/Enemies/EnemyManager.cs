using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public Transform target; // The player's transform
    private Animator animator;
    private UnityEngine.AI.NavMeshAgent agent; // Reference to the NavMeshAgent component

    public float attackRange = 4.5f;
    public float attackDamage = 10;
    
    [SerializeField]
    private float minSpeed = 1f, maxSpeed = 3f;
    private Transform groundCheck;
    private Transform attackPoint;
    public LayerMask groundLayer, playerLayer;
    private float moveSpeed;

    public List<AudioClip> attackSounds = new List<AudioClip>();
    public AudioSource vocalAudioSource;

    private AudioSource m_actionAudioSource;
    public List<AudioClip> m_FootstepSounds = new List<AudioClip>();
    private FootstepSwapper swapper;
    public float timeBetweenFootsteps = 0.6f; // Adjust this value to control the frequency of footsteps
    private float timeSinceLastFootstep = 0f;

    // Start is called before the first frame update
    void Awake()
    {
        swapper = GetComponent<FootstepSwapper>();
        m_actionAudioSource = GetComponent<AudioSource>();
        groundCheck = transform.Find("GroundCheck");
        attackPoint = transform.Find("EnemyAttackPoint");

        target = GameObject.FindWithTag("Player").GetComponent<Transform>();
        animator = GetComponent<Animator>();
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>(); // Get the NavMeshAgent component attached to this GameObject
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component not found!");
        }
    }

    void Start()
    {
        moveSpeed = Random.Range(minSpeed, maxSpeed);
        agent.speed = moveSpeed;

        animator.SetFloat("Velocity", moveSpeed - minSpeed);
    }

    public void MoveTowardsPlayer()
    {
        agent.SetDestination(target.position);

    }

    public void PlayFootstepsAudio()
    {
        swapper.CheckLayers();
        if (!IsGrounded())
        {
            return;
        }

        float currentTimeBetweenFootsteps = timeBetweenFootsteps;

        currentTimeBetweenFootsteps = timeBetweenFootsteps;

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

    public void SwapFootsteps(FootstepCollection collection)
    {
        m_FootstepSounds.Clear();

        for(int i = 0; i < collection.footstepSounds.Count; i++)
        {
            m_FootstepSounds.Add(collection.footstepSounds[i]);
        }
    }

    public bool IsInAttackRange()
    {
        float dist = Vector3.Distance(target.position, transform.position);

        if(dist <= attackRange)
        {
            return true;
        }
        else {
            return false;
        }
    }

    private bool IsGrounded()
    {
        return Physics.CheckSphere(groundCheck.position, 0.2f, groundLayer);
    }

    public bool PlayerInRange()
    {
        return Physics.CheckSphere(attackPoint.position, 3f, playerLayer);
    }

    public string RandomAttack()
    {
        int attackNumber = Random.Range(1, 4);

        return "Attack" + attackNumber.ToString();
    }

    public void PlayAttackAudio()
    {
        if (attackSounds.Count == 0) return;

        int i = Random.Range(0, attackSounds.Count);
        AudioClip currentClip = attackSounds[i];

        vocalAudioSource.pitch = Random.Range(0.75f, 1.35f);

        vocalAudioSource.PlayOneShot(currentClip);
    }
}
