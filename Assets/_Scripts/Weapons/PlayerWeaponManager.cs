using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerWeaponManager : MonoBehaviour
{
    [Header("Weapons")]
    [SerializeField] private GunController[] weapons;
    [SerializeField] private int startingWeaponIndex = 0;

    private PlayerInput playerInput;
    private InputAction shootAction;
    private InputAction reloadAction;
    private InputAction previousWeaponAction;
    private InputAction nextWeaponAction;

    private GunController currentWeapon;
    private int currentWeaponIndex;

    #region Initialization
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        shootAction = playerInput.actions["Shoot"];
        reloadAction = playerInput.actions["Reload"];
        previousWeaponAction = playerInput.actions["Previous"];
        nextWeaponAction = playerInput.actions["Next"];
    }

    private void Start()
    {
        EquipWeapon(startingWeaponIndex);
    }
    #endregion

    #region Update Loop
    private void Update()
    {
        if (currentWeapon == null)
            return;

        HandleShooting();
        HandleReload();
        HandleWeaponSwitching();
    }
    #endregion

    #region Shooting
    private void HandleShooting()
    {
        if (shootAction == null)
            return;

        currentWeapon.SetShooting(shootAction.IsPressed());
    }
    #endregion

    #region Reload
    private void HandleReload()
    {
        if (reloadAction == null)
            return;

        if (reloadAction.WasPressedThisFrame())
            currentWeapon.Reload();
    }
    #endregion

    #region Weapon Switching
    private void HandleWeaponSwitching()
    {
        if (previousWeaponAction != null && previousWeaponAction.WasPressedThisFrame())
            EquipPreviousWeapon();

        if (nextWeaponAction != null && nextWeaponAction.WasPressedThisFrame())
            EquipNextWeapon();
    }

    private void EquipPreviousWeapon()
    {
        int newIndex = currentWeaponIndex - 1;

        if (newIndex < 0)
            newIndex = weapons.Length - 1;

        EquipWeapon(newIndex);
    }

    private void EquipNextWeapon()
    {
        int newIndex = currentWeaponIndex + 1;

        if (newIndex >= weapons.Length)
            newIndex = 0;

        EquipWeapon(newIndex);
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
    #endregion
}