using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JoinRelay_UI : MonoBehaviour
{
    [SerializeField] private InputField joinCodeInputField;
    [SerializeField] private Button joinButton;
    [SerializeField] private RelayManager relayManager; // GameObject'teki bir script, Relay yönetimi için.

    private void Start()
    {
        joinButton.onClick.AddListener(OnJoinButtonClicked);
    }

    private void OnJoinButtonClicked()
    {
        string joinCode = joinCodeInputField.text.Trim();

        if (string.IsNullOrEmpty(joinCode))
        {
            Debug.Log("Join code cannot be empty!");
            return;
        }

        relayManager.JoinRelay(joinCode);
    }
}
