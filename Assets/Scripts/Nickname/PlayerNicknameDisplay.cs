using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Collections;

public class PlayerNicknameDisplay : NetworkBehaviour
{
    public Text nicknameText;  // Oyuncunun �zerindeki nickname yaz�s�
    public Transform player;   // Oyuncunun Transform'u
    public Vector3 offset;     // Yaz�n�n pozisyon ofseti

    private NetworkVariable<FixedString32Bytes> playerNickname = new NetworkVariable<FixedString32Bytes>(
        "", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private Nickname_Manager _nickname_Manager;
    private void Awake()
    {
        if (!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
        {
            Debug.LogError("NetworkManager not running!");
        }

        _nickname_Manager = FindFirstObjectByType<Nickname_Manager>();
    }

    private void Start()
    {
        if (IsOwner)
        {
            nicknameText.gameObject.SetActive(false); // Kendi nickname'ini gizle
            SetNickname(); // Kendi nickname'ini ayarla

            string nickname = Nickname_Manager.Instance.nickname; // Oyuncunun nickname'ini al
            PlayersNickname_Controller.Instance.AddPlayerNickname(nickname);   // Listeye ekle

            // Kendi nickname'ini "(You)" olarak ayarla
            nicknameText.text = $"{nickname} (You)";

            SubmitNicknameToServerRpc(nickname); // Sunucuya g�nder
        }

        playerNickname.OnValueChanged += OnNicknameChanged;

        // �lk de�er de�i�ikli�i ger�ekle�memi�se bile nickname'i ayarla
        if (!string.IsNullOrEmpty(playerNickname.Value.ToString()) && !IsOwner)
        {
            nicknameText.text = playerNickname.Value.ToString();
        }
    }

    private void SetNickname()
    {
        // Oyuncunun nickname'ini al ve `NetworkVariable`'e aktar
        string nickname = Nickname_Manager.Instance.nickname; // Nickname_Manager'dan nickname al
        playerNickname.Value = nickname;
    }

    public FixedString32Bytes GetNickname()
    {
        return playerNickname.Value; // Oyuncunun nickname'ini d�nd�r
    }

    private void OnNicknameChanged(FixedString32Bytes oldValue, FixedString32Bytes newValue)
    {
        // E�er kendinin de�ilse sadece ismi g�ster
        if (!IsOwner)
        {
            nicknameText.text = newValue.ToString();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitNicknameToServerRpc(string nickname)
    {
        // Sunucu taraf�nda PlayerManager'a ekle
        if (PlayersNickname_Controller.Instance != null)
        {
            PlayersNickname_Controller.Instance.AddPlayerNickname(nickname);
        }
    }
}
