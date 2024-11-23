using Unity.Netcode;
using UnityEngine;

public class PlayerAbilityUsage : NetworkBehaviour
{
    private PlayerAbilities playerAbilities;
    private SnowboardController snowboard;

    int bulletCount = 3;
    int highJumpCount = 5;

    private void Start()
    {
        playerAbilities = GetComponent<PlayerAbilities>();
        snowboard = GetComponent<SnowboardController>();
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetMouseButtonDown(0)) 
        {
            UseAbility("Bullet");
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            UseAbility("Shield");
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            UseAbility("HighJump");
        }
    }

    private void UseAbility(string abilityName)
    {
        if (playerAbilities.HasAbility(abilityName))
        {
            Debug.Log($"{abilityName} used by player {OwnerClientId}");
            UseAbilityServerRpc(abilityName);
        }
        else
        {
            Debug.Log($"Player {OwnerClientId} does not have ability {abilityName}");
        }
    }

    [ServerRpc]
    private void UseAbilityServerRpc(string abilityName)
    {
        Debug.Log($"Server: {abilityName} activated by {OwnerClientId}");

        if(abilityName == "Bullet")
        {
            if (bulletCount > 0)
            {
                snowboard.ShootServerRpc();
                bulletCount -= 1;
            }

            if (bulletCount == 0) 
            { 
                playerAbilities.RemoveAbility(abilityName);
                bulletCount = 3;
            }
        }
        if (abilityName == "Shield")
        {
            snowboard.ShieldServerRpc();
            playerAbilities.RemoveAbility(abilityName);
        }

        if (abilityName == "HighJump")
        {
            if (highJumpCount > 0)
            {
                snowboard.HighJumpServerRpc();
                highJumpCount -= 1;
            }

            if (highJumpCount == 0)
            {
                snowboard.InitialJumpServerRpc();
                playerAbilities.RemoveAbility(abilityName);
                highJumpCount = 5;
            }
        }
    }
}
