using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Nickname_Manager : MonoBehaviour
{
    public static Nickname_Manager Instance { get; private set; }

    public string nickname;

    [SerializeField] private InputField nicknameInputField;
    [SerializeField] private GameObject nicknamePanel;      
    [SerializeField] private GameObject otherButtonsPanel;
    [SerializeField] private Text ownerNicknameTestPos;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        nicknamePanel.SetActive(false);
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

    public void ResetPanels()
    {
        otherButtonsPanel.SetActive(true);
        nicknamePanel.SetActive(false);
        UI_Manager.Instance.UIObjectsDisabled();
    }

    public void PlayGameUISet()
    {
        nicknamePanel.SetActive(true);
    }
}
