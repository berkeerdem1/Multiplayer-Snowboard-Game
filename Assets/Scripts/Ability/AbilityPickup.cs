using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class AbilityPickup : NetworkBehaviour
{
    [SerializeField] private AbilitySO abilitySO;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private GameObject sphere;
    [SerializeField] private AudioClip pickUpAudio;

    Color originalColor;

    private AudioSource _audo;
    
    public override void OnNetworkSpawn()
    {
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
            Debug.LogError("NetworkObject not found!");
            return;
        }
        else
        {
            Debug.Log("NetworkObject found.");
        }

        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager not found!");
        }
        else if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogError("Pickup should be controlled by the server only!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {

            var networkObject = other.GetComponentInParent<NetworkObject>();
            if (networkObject != null)
            {
                Debug.Log("There is a netwkObject in the player");
                int abilityID = abilitySO.number;
                if (abilityID >= 0)
                {
                    ulong clientId = networkObject.NetworkObjectId;
                    RequestAbilityPickupServerRpc(clientId, abilityID);
                }
            }
            _audo.PlayOneShot(pickUpAudio);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestAbilityPickupServerRpc(ulong clientId, int abilityID)
    {
        Debug.Log("RequestAbilityPickupServerRpc");

        Debug.Log($"ServerRpc. ClientId: {clientId}, AbilityID: {abilityID}");


        NetworkObject playerNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[clientId];
        if (playerNetworkObject != null)
        {
            RequestAbilityPickupClientRpc(clientId, abilityID);
        }
        else
        {
            Debug.LogError("PlayerObject not found!");
        }
    }

    [ClientRpc]
    private void RequestAbilityPickupClientRpc(ulong playerId, int abilityID)
    {
        Debug.Log($"ClientRpc. PlayerId: {playerId}, AbilityID: {abilityID}");

        // Adding skills to a player
        NetworkObject playerNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerId];
        if (playerNetworkObject != null)
        {

            //Disable pickup object
            GrantAbilityToPlayer(playerNetworkObject, abilityID);
            Debug.Log("GrantAbilityToPlayer");

        }
        else
        {
            Debug.LogError("PlayerObject not found!");
        }
    }

    private void GrantAbilityToPlayer(NetworkObject player, int abilityID)
    {
        Debug.Log("GrantAbilityToPlayer");

        var playerAbilities = player.GetComponentInParent<PlayerAbilities>();
        if (playerAbilities != null)
        {
            Debug.Log($"Player received the ability with ID {abilityID}.");
            playerAbilities.AddAbility(abilityID);
            DisablePickupServerRpc(); // Disable pickup object.
        }

        if (abilitySO.abilityName == "Dash")
        {
            var abilityController = player.GetComponentInParent<Ability_Controller>();
            if (abilityController != null)
            {
                abilityController.Dash();
                Debug.Log("Dash Ability implemented.");
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
            originalColor = sprite.color;
            sprite.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        }

        sphere.SetActive(false);
        gameObject.GetComponent<Collider>().enabled = false;

        // Only the server starts the pickup re-enable timer.
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
            originalColor = sprite.color;
            sprite.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
        }

        sphere.SetActive(true);
        gameObject.GetComponent<Collider>().enabled = true;
    }
}
