using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAbilities : NetworkBehaviour
{
    public AbilitySO[] allAbilities;
    [SerializeField] private List<int> abilityNumbs = new List<int>();
    [SerializeField] private List<int> abilityIDs = new List<int>(); // Synchronizing talent IDs.//

    private void Awake()
    {
        if (IsServer)
        {
            abilityIDs.Clear();
        }
    }

    public void AddAbility(int abilityID)
    {
        if (!abilityIDs.Contains(abilityID))
        {
            abilityIDs.Add(abilityID);
            Debug.Log($"Ability ID {abilityID} added to player.");
        }
        else
        {
            Debug.Log($"Ability ID {abilityID} has already been added.");
        }
    }

    public bool HasAbility(int abilityID)
    {
        return abilityIDs.Contains(abilityID);
    }

    public AbilitySO GetAbility(int abilityID)
    {
        if (abilityID >= 0 && abilityID < allAbilities.Length)
        {
            return allAbilities[abilityID];
        }

        Debug.LogWarning("Invalid skill ID!");
        return null;
    }

    public void RemoveAbility(int abilityID)
    {
        if (abilityIDs.Contains(abilityID))
        {
            abilityIDs.Remove(abilityID);
            
            Debug.Log($"Ability ID {abilityID} has been removed from player.");
        }
        abilityNumbs.Remove(abilityID);
    }
}
