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
        if (!IsOwner) return; // Sadece yerel oyuncu kontrolleri uygulas�n

        // Yeteneklerin kullan�m�
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
            Debug.Log($"{abilityNumber} kullan�ld�, Player ID: {OwnerClientId}");
            UseAbilityManager(abilityNumber);
        }
        else
        {
            Debug.Log($"Player {OwnerClientId} yetene�i yok: {abilityNumber}");
        }
    }
    private void UseAbilityManager(int abilityNumb)
    {
        if (!playerAbilities.HasAbility(abilityNumb))
        {
            Debug.LogWarning($"Player {OwnerClientId}, {abilityNumb} yetene�ine sahip de�il!");
            return;
        }

        Debug.Log($"Server: {abilityNumb} yetene�i aktif edildi, Player ID: {OwnerClientId}");

        // Yetenekler �zerinde i�lem yap
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

    // "Bullet" yetene�i i�lemleri
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
            Debug.Log("Mermiler bitti, yetenek kald�r�l�yor.");
            playerAbilities.RemoveAbility(1);
            bulletCount = 3; // Yeniden dolum i�in
        }
    }

    // "Shield" yetene�i i�lemleri
    private void HandleShieldAbility()
    {
        snowboard.Shield();
        Debug.Log("Shield etkinle�tirildi.");
        playerAbilities.RemoveAbility(2);
    }

    // "HighJump" yetene�i i�lemleri
    private void HandleHighJumpAbility()
    {
        if (highJumpCount > 0)
        {
            snowboard.HighJump();
            highJumpCount--;

            Debug.Log($"Kalan z�plama: {highJumpCount}");
        }

        if (highJumpCount == 0)
        {
            Debug.Log("Z�plama hakk� bitti, yetenek kald�r�l�yor.");
            snowboard.InitialJump();
            playerAbilities.RemoveAbility(3);
            highJumpCount = 5; // Yeniden dolum i�in
        }
    }
}
