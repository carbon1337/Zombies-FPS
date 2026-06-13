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

    private void Start()
    {
        currentAmmo = gunData.magazineSize;
        reserveAmmo = gunData.maxReserveAmmo;
    }

    private void Update()
    {
        if (Input.GetButton("Fire1"))
            TryShoot();

        if (Input.GetKeyDown(KeyCode.R))
            TryReload();
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
        if (gunData.shootSound != null)
            audioSource.PlayOneShot(gunData.shootSound);

        if (gunData.muzzleFlashPrefab != null && muzzlePoint != null)
            Instantiate(gunData.muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation);

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, gunData.range))
        {
            if (hit.collider.TryGetComponent(out IDamageable damageable))
                damageable.TakeDamage(gunData.damage);

            if (gunData.hitEffectPrefab != null)
                Instantiate(gunData.hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
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

        if (gunData.reloadSound != null)
            audioSource.PlayOneShot(gunData.reloadSound);

        yield return new WaitForSeconds(gunData.reloadTime);

        int ammoNeeded = gunData.magazineSize - currentAmmo;
        int ammoToLoad = Mathf.Min(ammoNeeded, reserveAmmo);

        currentAmmo += ammoToLoad;
        reserveAmmo -= ammoToLoad;

        isReloading = false;
    }
}