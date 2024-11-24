using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAbility", menuName = "Abilities/Ability")]
public class AbilitySO : ScriptableObject
{
    public string abilityName;
    public Sprite abilityIcon;
    public float cooldownTime;
    public string description;
    public int number;

    public void Activate(GameObject player)
    {
        // Yetenek aktivasyon mantýðý burada iþlenebilir.
        Debug.Log($"{abilityName} is activated by {player.name}");
    }
}
