using Unity.Netcode;
using UnityEngine;

public class PlayerAbilityUsage : NetworkBehaviour
{
    // Ability limits
    [SerializeField] private int bulletCount = 6;
    [SerializeField] private int highJumpCount = 8;

    private PlayerAbilities _playerAbilities;
    private Ability_Controller _myAbilityController;
    private SnowboardController _mySnowboard;

    private void Start()
    {
        _playerAbilities = GetComponent<PlayerAbilities>();
        _myAbilityController = GetComponent<Ability_Controller>();
        _mySnowboard = GetComponent<SnowboardController>();
    }

    private void Update()
    {
        if (!IsOwner) return; // Only apply local player controls

        // Use of abilities

        if (Input.GetMouseButtonDown(0)) // Shoot 
        {
            UseAbility(1);
        }

        if (Input.GetKeyDown(KeyCode.C)) // Shield
        {
            UseAbility(2);
        }

        if (Input.GetKeyDown(KeyCode.Space) && _mySnowboard.CheckGround()) // High jump
        {
            UseAbility(3);
        }
    }

    private void UseAbility(int abilityNumber)
    {
        if (_playerAbilities.HasAbility(abilityNumber))
        {
            Debug.Log($"{abilityNumber} used, Player ID: {OwnerClientId}");
            UseAbilityManager(abilityNumber);
        }
        else
        {
            Debug.Log($"Player {OwnerClientId} no ability: {abilityNumber}");
        }
    }

    private void UseAbilityManager(int abilityNumb)
    {
        if (!_playerAbilities.HasAbility(abilityNumb))
        {
            Debug.LogWarning($"Player {OwnerClientId}, {abilityNumb} does not have this ability!");
            return;
        }

        Debug.Log($"Server: {abilityNumb} ability activated, Player ID: {OwnerClientId}");

        // Yetenekler üzerinde iþlem yap
        switch (abilityNumb)
        {
            case 1:
                HandleBulletAbility();
                break;

            case 2:
                HandleShieldAbility();
                break;

            case 3:
                HandleHighJumpAbility();
                break;

            default:
                Debug.LogWarning($"Unknown ability: {abilityNumb}");
                break;
        }
    }

    // "Bullet" ability process
    private void HandleBulletAbility()
    {
        if (bulletCount > 0)
        {
            _myAbilityController.Shoot();
            bulletCount--;
        }

        if (bulletCount == 0)
        {
            Debug.Log("Bullets end, Shoot ability is being removed.");
            _playerAbilities.RemoveAbility(1);
            bulletCount = 3; 
        }
    }

    // "Shield" ability process
    private void HandleShieldAbility()
    {
        _myAbilityController.Shield();
        Debug.Log("Shield activated.");
        _playerAbilities.RemoveAbility(2);
    }

    // "HighJump"  ability process
    private void HandleHighJumpAbility()
    {
        if (highJumpCount > 0)
        {
            _myAbilityController.HighJump();
            highJumpCount--;
        }

        if (highJumpCount == 0)
        {
            Debug.Log("High jump end, ability is being removed.");
            _myAbilityController.InitialJump();
            _playerAbilities.RemoveAbility(3);
            highJumpCount = 5;
        }
    }
}
