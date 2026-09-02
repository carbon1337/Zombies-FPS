using UnityEngine;

[CreateAssetMenu(fileName = "New Gun", menuName = "Weapons/Gun Data")]
public class GunData : ScriptableObject
{
    [Header("Info")]
    public string gunName;

    [Header("Fire Mode")]
    public FireMode fireMode;

    [Header("Damage")]
    public float damage = 25f;
    public float range = 100f;
    public float fireRate = 0.15f;

    [Header("Ammo")]
    public int magazineSize = 8;
    public int maxReserveAmmo = 80;
    public float reloadTime = 1.5f;

    [Header("Effects")]
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public GameObject muzzleFlashPrefab;
    public GameObject hitEffectPrefab;
}