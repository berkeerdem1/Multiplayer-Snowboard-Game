using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class AbilityPickup : NetworkBehaviour
{
    public AbilitySO abilitySO;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private GameObject sphere;
    Color originalColor;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Debug.Log("Sunucu bu objeyi yönetiyor.");
        }
        else if (IsClient)
        {
            Debug.Log("Ýstemci bu objeyi yönetiyor.");
        }
        else
        {
            Debug.Log("Bu obje herhangi bir taraf tarafýndan yönetilmiyor.");
        }
    }

    private void Awake()
    {
        sprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            originalColor = sprite.color;
        }
    }
    private void Start()
    {
        if (IsServer)
        {
            Debug.Log("Bu obje sunucu tarafýndan yönetiliyor.");
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer)
        {
            return;
        }

        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player degdi");

            var networkObject = other.gameObject.GetComponentInParent<NetworkObject>();
            if (networkObject != null && networkObject.IsOwner)
            {
                RequestAbilityPickupServerRpc(networkObject.OwnerClientId);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestAbilityPickupServerRpc(ulong clientId)
    {
        // Oyuncuyu NetworkManager üzerinden bul
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            var playerObject = client.PlayerObject;
            if (playerObject != null)
            {
                GrantAbilityToPlayer(playerObject.gameObject);
            }
        }
    }

    private void GrantAbilityToPlayer(GameObject player)
    {
        if (!IsServer) return;

        var playerAbilities = player.GetComponentInParent<PlayerAbilities>();
        if (playerAbilities != null)
        {
            Debug.Log("Player" + abilitySO.abilityName + "yetenegini aldi");
            playerAbilities.AddAbilityServerRpc(abilitySO.abilityName);
            DisablePickupServerRpc();
        }

        if (abilitySO.abilityName == "Dash")
        {
            player.GetComponentInParent<SnowboardController>().DashServerRpc();
            player.GetComponentInParent<PlayerAbilities>().RemoveAbility(abilitySO.abilityName);
            Debug.Log("Player dash yetenegini aldi");
            DisablePickupServerRpc();
        }
        
    }

    [ServerRpc]
    private void DisablePickupServerRpc()
    {
        DisablePickupClientRpc();
    }

    [ClientRpc]
    private void DisablePickupClientRpc()
    {
        if (sprite != null)
        {
            // Mevcut renk deðerini alýyoruz
            originalColor = sprite.color;
            sprite.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        }

        sphere.SetActive(false);
        gameObject.GetComponent<Collider>().enabled = false;
        StartCoroutine(Coroutine());
    }

    IEnumerator Coroutine()
    {
        yield return new WaitForSeconds(5f);
        EnabledPickupServerRpc();
    }


    [ServerRpc]
    private void EnabledPickupServerRpc()
    {
        if (sprite != null)
        {
            // Mevcut renk deðerini alýyoruz
            originalColor = sprite.color;
            sprite.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
        }

        sphere.SetActive(true);
        gameObject.GetComponent<Collider>().enabled = true;
    }
}
