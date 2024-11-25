using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class AbilityPickup : NetworkBehaviour
{
    [SerializeField] private AbilitySO abilitySO;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private GameObject sphere;
    Color originalColor;

    private AudioSource _audo;
    [SerializeField] private AudioClip pickUpAudio;
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Debug.Log("Sunucu bu objeyi y�netiyor.");
        }
        else if (IsClient)
        {
            Debug.Log("�stemci bu objeyi y�netiyor.");
        }
        else
        {
            Debug.Log("Bu obje herhangi bir taraf taraf�ndan y�netilmiyor.");
        }
    }

    private void Awake()
    {
        _audo = GetComponent<AudioSource>();
        sprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            originalColor = sprite.color;
        }
    }
    private void Start()
    {
        if (!IsServer)
        {
            return;
        }

        if (!TryGetComponent(out NetworkObject networkObject))
        {
            Debug.LogError("NetworkObject bulunamad�!");
            return;
        }
        else
        {
            Debug.Log("NetworkObject bulundu.");
        }

        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager bulunamad�!");
        }
        else if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogError("Pickup yaln�zca sunucu taraf�ndan kontrol edilmelidir!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player degdi");

            var networkObject = other.GetComponentInParent<NetworkObject>();
            if (networkObject != null)
            {
                Debug.Log("Player'da netwkObject var");
                int abilityID = abilitySO.number;
                if (abilityID >= 0)
                {
                    ulong clientId = networkObject.NetworkObjectId;
                    RequestAbilityPickupServerRpc(clientId, abilityID);
                    Debug.Log("RequestAbilityPickupServerRpc cagirildi");
                }
            }
            _audo.PlayOneShot(pickUpAudio);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestAbilityPickupServerRpc(ulong clientId, int abilityID)
    {
        Debug.Log("RequestAbilityPickupServerRpc calisti");

        Debug.Log($"ServerRpc �a�r�ld�. ClientId: {clientId}, AbilityID: {abilityID}");


        NetworkObject playerNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[clientId];
        if (playerNetworkObject != null)
        {
            RequestAbilityPickupClientRpc(clientId, abilityID);
            Debug.Log("RequestAbilityPickupClientRpc calisti");
        }
        else
        {
            Debug.LogError("Oyuncu PlayerObject bulunamad�!");
        }
    }

    [ClientRpc]
    private void RequestAbilityPickupClientRpc(ulong playerId, int abilityID)
    {
        Debug.Log($"ClientRpc �a�r�ld�. PlayerId: {playerId}, AbilityID: {abilityID}");

        // Oyuncuya yetenek ekleme i�lemi
        NetworkObject playerNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerId];
        if (playerNetworkObject != null)
        {

            // Pickup nesnesini devre d��� b�rak
            GrantAbilityToPlayer(playerNetworkObject, abilityID);
            Debug.Log("GrantAbilityToPlayer cagirildi");

        }
        else
        {
            Debug.LogError("Oyuncu bulunamad�!");
        }
    }

    private void GrantAbilityToPlayer(NetworkObject player, int abilityID)
    {
        //if (!IsServer) return;

        Debug.Log("GrantAbilityToPlayer calisti");

        var playerAbilities = player.GetComponentInParent<PlayerAbilities>();
        if (playerAbilities != null)
        {
            Debug.Log($"Player {abilityID} ID'li yetene�i ald�.");
            playerAbilities.AddAbility(abilityID);
            DisablePickupServerRpc(); // Pickup nesnesini devre d��� b�rak.
        }

        if (abilitySO.abilityName == "Dash")
        {
            var snowboardController = player.GetComponentInParent<SnowboardController>();
            if (snowboardController != null)
            {
                snowboardController.Dash();
                Debug.Log("Dash yetene�i uyguland�.");
                playerAbilities.RemoveAbility(abilityID);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DisablePickupServerRpc()
    {
        DisablePickupClientRpc();
    }

    [ClientRpc]
    private void DisablePickupClientRpc()
    {
        if (sprite != null)
        {
            // Mevcut renk de�erini al�yoruz
            originalColor = sprite.color;
            sprite.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        }

        sphere.SetActive(false);
        gameObject.GetComponent<Collider>().enabled = false;

        // Pickup'u geri etkinle�tirme zamanlay�c�s�n� yaln�zca server ba�lat�r.
        if (IsServer)
        {
            StartCoroutine(ReEnablePickupCoroutine());
        }
    }

    IEnumerator ReEnablePickupCoroutine()
    {
        yield return new WaitForSeconds(5f);
        EnablePickupServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void EnablePickupServerRpc()
    {
        EnablePickupClientRpc();
    }

    [ClientRpc]
    private void EnablePickupClientRpc()
    {
        if (sprite != null)
        {
            // Mevcut renk de�erini al�yoruz
            originalColor = sprite.color;
            sprite.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
        }

        sphere.SetActive(true);
        gameObject.GetComponent<Collider>().enabled = true;
    }
}
