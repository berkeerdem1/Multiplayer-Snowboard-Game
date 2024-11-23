using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAbilities : NetworkBehaviour
{
    [SerializeField] private List<string> abilities = new List<string>();

    [ServerRpc(RequireOwnership = false)]
    public void AddAbilityServerRpc(string abilityName)
    {
        if (!abilities.Contains(abilityName))
        {
            abilities.Add(abilityName);
            Debug.Log($"Ability {abilityName} added to {OwnerClientId}");
            NotifyAbilityAddedClientRpc(abilityName);
        }
    }

    [ClientRpc]
    private void NotifyAbilityAddedClientRpc(string abilityName)
    {
        Debug.Log($"Client: Ability {abilityName} added.");
        // UI güncellemesi buraya eklenebilir.
    }

    public bool HasAbility(string abilityName)
    {
        return abilities.Contains(abilityName);
    }

    public void RemoveAbility(string abilityName)
    {
        abilities.Remove(abilityName);
    }
}
