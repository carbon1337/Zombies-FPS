using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeaponManager : MonoBehaviour
{
    [Header("Weapons")]
    [SerializeField] private GunController[] weapons;
    [SerializeField] private int startingWeaponIndex = 0;

    private GunController currentWeapon;
    private int currentWeaponIndex;

    private void Start()
    {
        EquipWeapon(startingWeaponIndex);
    }

    public void OnShoot(InputValue value)
    {
        Debug.Log("Shoot input received: " + value.isPressed);

        if (currentWeapon == null)
        {
            Debug.LogWarning("No current weapon assigned.");
            return;
        }

        currentWeapon.SetShooting(value.isPressed);
    }

    public void OnReload(InputValue value)
    {
        Debug.Log("Reload input received.");

        if (!value.isPressed)
            return;

        if (currentWeapon == null)
        {
            Debug.LogWarning("No current weapon assigned.");
            return;
        }

        currentWeapon.Reload();
    }

    public void EquipWeapon(int weaponIndex)
    {
        if (weapons.Length == 0)
        {
            Debug.LogWarning("No weapons in weapon manager.");
            return;
        }

        if (weaponIndex < 0 || weaponIndex >= weapons.Length)
        {
            Debug.LogWarning("Weapon index out of range.");
            return;
        }

        if (currentWeapon != null)
            currentWeapon.Unequip();

        currentWeaponIndex = weaponIndex;
        currentWeapon = weapons[currentWeaponIndex];
        currentWeapon.Equip();

        Debug.Log("Equipped weapon: " + currentWeapon.name);
    }
}