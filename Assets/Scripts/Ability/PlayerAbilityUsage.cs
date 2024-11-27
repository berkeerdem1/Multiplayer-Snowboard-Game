using Unity.Netcode;
using UnityEngine;

public class PlayerAbilityUsage : NetworkBehaviour
{
    private PlayerAbilities playerAbilities;
    private SnowboardController snowboard;

    // Yetenek limitleri
    private int bulletCount = 6;
    private int highJumpCount = 8;

    private void Start()
    {
        playerAbilities = GetComponent<PlayerAbilities>();
        snowboard = GetComponent<SnowboardController>();
    }

    private void Update()
    {
        if (!IsOwner) return; // Sadece yerel oyuncu kontrolleri uygulasýn

        // Yeteneklerin kullanýmý
        if (Input.GetMouseButtonDown(0))
        {
            UseAbility(1);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            UseAbility(2);
        }

        if (Input.GetKeyDown(KeyCode.Space) && snowboard.CheckGround())
        {
            UseAbility(3);
        }
    }

    private void UseAbility(int abilityNumber)
    {
        if (playerAbilities.HasAbility(abilityNumber))
        {
            Debug.Log($"{abilityNumber} kullanýldý, Player ID: {OwnerClientId}");
            UseAbilityManager(abilityNumber);
        }
        else
        {
            Debug.Log($"Player {OwnerClientId} yeteneði yok: {abilityNumber}");
        }
    }
    private void UseAbilityManager(int abilityNumb)
    {
        if (!playerAbilities.HasAbility(abilityNumb))
        {
            Debug.LogWarning($"Player {OwnerClientId}, {abilityNumb} yeteneðine sahip deðil!");
            return;
        }

        Debug.Log($"Server: {abilityNumb} yeteneði aktif edildi, Player ID: {OwnerClientId}");

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
                Debug.LogWarning($"Bilinmeyen yetenek: {abilityNumb}");
                break;
        }
    }

    // "Bullet" yeteneði iþlemleri
    private void HandleBulletAbility()
    {
        if (bulletCount > 0)
        {
            snowboard.Shoot();
            bulletCount--;

            Debug.Log($"Kalan mermi: {bulletCount}");
        }

        if (bulletCount == 0)
        {
            Debug.Log("Mermiler bitti, yetenek kaldýrýlýyor.");
            playerAbilities.RemoveAbility(1);
            bulletCount = 3; // Yeniden dolum için
        }
    }

    // "Shield" yeteneði iþlemleri
    private void HandleShieldAbility()
    {
        snowboard.Shield();
        Debug.Log("Shield etkinleþtirildi.");
        playerAbilities.RemoveAbility(2);
    }

    // "HighJump" yeteneði iþlemleri
    private void HandleHighJumpAbility()
    {
        if (highJumpCount > 0)
        {
            snowboard.HighJump();
            highJumpCount--;

            Debug.Log($"Kalan zýplama: {highJumpCount}");
        }

        if (highJumpCount == 0)
        {
            Debug.Log("Zýplama hakký bitti, yetenek kaldýrýlýyor.");
            snowboard.InitialJump();
            playerAbilities.RemoveAbility(3);
            highJumpCount = 5; // Yeniden dolum için
        }
    }
}
