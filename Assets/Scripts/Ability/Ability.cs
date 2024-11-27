using UnityEngine;

[CreateAssetMenu(fileName = "NewAbility", menuName = "Abilities/Ability")]
public class AbilitySO : ScriptableObject
{
    public string abilityName;
    public Sprite abilityIcon;
    public string description;
    public int number;

    public void Activate(GameObject player)
    {
        Debug.Log($"{abilityName} is activated by {player.name}");
    }
}
