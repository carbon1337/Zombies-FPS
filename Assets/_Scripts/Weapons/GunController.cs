using System.Collections;
using UnityEngine;

public class GunController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GunData gunData;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform muzzlePoint;
    [SerializeField] private AudioSource audioSource;

    private int currentAmmo;
    private int reserveAmmo;
    private float nextFireTime;
    private bool isReloading;
    private bool isShooting;

    private void Start()
    {
        currentAmmo = gunData.magazineSize;
        reserveAmmo = gunData.maxReserveAmmo;
    }

    private void Update()
    {
        if (isShooting)
            TryShoot();
    }

    public void SetShooting(bool shooting)
    {
        isShooting = shooting;
    }

    public void Reload()
    {
        TryReload();
    }

    public void Equip()
    {
        gameObject.SetActive(true);
        isShooting = false;
    }

    public void Unequip()
    {
        isShooting = false;
        gameObject.SetActive(false);
    }

    private void TryShoot()
    {
        if (isReloading)
            return;

        if (Time.time < nextFireTime)
            return;

        if (currentAmmo <= 0)
        {
            TryReload();
            return;
        }

        nextFireTime = Time.time + gunData.fireRate;
        currentAmmo--;

        Shoot();
    }

    private void Shoot()
    {
        Debug.Log("Shot fired. Ammo left: " + currentAmmo);

        if (gunData.shootSound != null && audioSource != null)
            audioSource.PlayOneShot(gunData.shootSound);

        if (gunData.muzzleFlashPrefab != null && muzzlePoint != null)
            Instantiate(gunData.muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation);

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, gunData.range))
        {
            Debug.Log("Hit: " + hit.collider.name);

            IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(gunData.damage);
                Debug.Log("Damage dealt: " + gunData.damage);
            }
            else
            {
                Debug.LogWarning("Hit object does not have IDamageable on itself or parent.");
            }

            if (gunData.hitEffectPrefab != null)
                Instantiate(gunData.hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        }
        else
        {
            Debug.Log("Shot missed.");
        }
    }

    private void TryReload()
    {
        if (isReloading)
            return;

        if (currentAmmo == gunData.magazineSize)
            return;

        if (reserveAmmo <= 0)
            return;

        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        isShooting = false;

        if (gunData.reloadSound != null && audioSource != null)
            audioSource.PlayOneShot(gunData.reloadSound);

        yield return new WaitForSeconds(gunData.reloadTime);

        int ammoNeeded = gunData.magazineSize - currentAmmo;
        int ammoToLoad = Mathf.Min(ammoNeeded, reserveAmmo);

        currentAmmo += ammoToLoad;
        reserveAmmo -= ammoToLoad;

        isReloading = false;
    }
}