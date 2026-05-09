/*

Basic player health manager handling damage, regeneration,
UI health overlays, red screen tint, and death checks.

*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthManager : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    public float currentHealth;

    [Header("Healing Settings")]
    public float healingTimer = 3f;
    public float healingCooldown = 1f;

    private float healingCooldownRemaining;
    private bool recentlyDamaged = false;
    private Coroutine regenCoroutine;

    [Header("UI References")]
    public Image redTint;
    public List<CanvasGroup> healthImages = new List<CanvasGroup>();
    public List<float> healthThresholds = new List<float>();

    private Coroutine fadeCoroutine;
    private List<Coroutine> fadeImageCoroutines = new List<Coroutine>();

    public static PlayerHealthManager Instance { get; private set; }

    #region Initialization
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        //Create a matching coroutine slot for each health image overlay
        fadeImageCoroutines = new List<Coroutine>(new Coroutine[healthImages.Count]);
    }

    private void Start()
    {
        //Start the player at full health
        currentHealth = maxHealth;
    }
    #endregion

    #region Update Loop
    private void Update()
    {
        HandleDeathCheck();
        HandleHealthClamp();
        HandleHealingCooldown();
        UpdateHealthDisplay();
    }
    #endregion

    #region Health Logic
    public void TakeDamage(float damage)
    {
        //Reset healing cooldown whenever damage is taken
        healingCooldownRemaining = healingCooldown;
        recentlyDamaged = true;

        currentHealth -= damage;

        Debug.Log("Current Health: " + currentHealth + " | Tint Alpha: " + redTint.color.a);

        //Restart regeneration timer so multiple coroutines do not stack
        if (regenCoroutine != null)
            StopCoroutine(regenCoroutine);

        regenCoroutine = StartCoroutine(RegenerateHealth());
    }

    void HandleHealingCooldown()
    {
        //Count down the damage cooldown timer
        if (healingCooldownRemaining > 0f)
        {
            healingCooldownRemaining -= Time.deltaTime;
            recentlyDamaged = true;
        }
        else
        {
            healingCooldownRemaining = 0f;
            recentlyDamaged = false;
        }
    }

    IEnumerator RegenerateHealth()
    {
        //Wait before attempting to heal
        yield return new WaitForSeconds(healingTimer);

        //Cancel healing if the player was damaged again recently
        if (recentlyDamaged)
            yield break;

        //Continue healing until full health is reached
        while (currentHealth < maxHealth)
        {
            currentHealth += 20f;

            //Prevent overhealing
            if (currentHealth > maxHealth)
                currentHealth = maxHealth;

            yield return new WaitForSeconds(1f);
        }
    }

    void HandleHealthClamp()
    {
        //Prevent health from going above max
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
    }

    void HandleDeathCheck()
    {
        //Trigger death when health reaches zero
        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Die();
        }
    }

    public void Die()
    {

    }
    #endregion

    #region UI
    void UpdateHealthDisplay()
    {
        if (healthImages.Count != healthThresholds.Count)
        {
            Debug.Log("Error: Unequal amount of health images and thresholds");
            return;
        }

        //Convert current health into a 0-1 alpha value for the red tint
        float targetAlpha = 1f - (currentHealth / maxHealth);

        //Fade the red tint overlay based on health
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeRedTintAlpha(targetAlpha));

        //Fade threshold-based health warning images in or out
        for (int i = 0; i < healthThresholds.Count; i++)
        {
            float targetImageAlpha = currentHealth < healthThresholds[i] ? 1f : 0f;

            if (fadeImageCoroutines[i] != null)
                StopCoroutine(fadeImageCoroutines[i]);

            fadeImageCoroutines[i] = StartCoroutine(FadeImageAlpha(healthImages[i], targetImageAlpha));
        }
    }

    IEnumerator FadeRedTintAlpha(float targetAlpha)
    {
        if (redTint == null) yield break;

        float startAlpha = redTint.color.a;
        float duration = 0.2f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);

            Color tintColor = redTint.color;
            tintColor.a = newAlpha;
            redTint.color = tintColor;

            yield return null;
        }

        //Ensure the final alpha is set exactly
        Color finalTintColor = redTint.color;
        finalTintColor.a = targetAlpha;
        redTint.color = finalTintColor;
    }

    IEnumerator FadeImageAlpha(CanvasGroup canvasGroup, float targetAlpha)
    {
        if (canvasGroup == null) yield break;

        float startAlpha = canvasGroup.alpha;
        float duration = 0.2f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            yield return null;
        }

        //Ensure the final alpha is set exactly
        canvasGroup.alpha = targetAlpha;
    }
    #endregion
}