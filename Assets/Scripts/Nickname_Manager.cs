using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class Nickname_Manager : MonoBehaviour
{
    public static Nickname_Manager Instance { get; private set; }

    [SerializeField] private InputField nicknameInputField;
    [SerializeField] private GameObject nicknamePanel;      
    [SerializeField] private GameObject otherButtonsPanel;
    public string nickname;

    [SerializeField]
    private Text ownerNicknameTestPos;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        otherButtonsPanel.SetActive(false);
    }

    public void SaveNickname()
    {
        nickname = nicknameInputField.text;
        if (!string.IsNullOrEmpty(nickname))
        {
            SetNickname(nickname);

            nicknamePanel.SetActive(false);                  
            otherButtonsPanel.SetActive(true);

            ownerNicknameTestPos.text = nickname;
        }
    }

    public void SetNickname(string newNickname)
    {
        nickname = newNickname;
    }
}
