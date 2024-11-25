using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAbilities : NetworkBehaviour
{
    public AbilitySO[] allAbilities; // Tüm yeteneklerin ScriptableObject referanslarý
    [SerializeField] private List<int> abilityNumbs = new List<int>();
    [SerializeField] private List<int> abilityIDs = new List<int>(); // Yetenek ID'lerini senkronize ediyoruz. //

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
            Debug.Log($"Yetenek ID {abilityID} oyuncuya eklendi.");
        }
        else
        {
            Debug.Log($"Yetenek ID {abilityID} zaten eklenmiþti.");
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

        Debug.LogWarning("Geçersiz yetenek ID'si!");
        return null;
    }

    public void RemoveAbility(int abilityID)
    {
        if (abilityIDs.Contains(abilityID))
        {
            abilityIDs.Remove(abilityID);
            
            Debug.Log($"Yetenek ID {abilityID} oyuncudan kaldýrýldý.");
        }
        abilityNumbs.Remove(abilityID);
    }
}
