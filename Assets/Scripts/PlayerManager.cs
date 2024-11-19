using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class PlayerManager : NetworkBehaviour
{
    private NetworkList<FixedString32Bytes> playerNicknames = new NetworkList<FixedString32Bytes>();

    public static PlayerManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Oyuncular listeye eklendi�inde client'lara bilgi g�nder
            playerNicknames.OnListChanged += OnPlayerListChanged;
        }
    }

    public void AddPlayerNickname(string nickname)
    {
        if (IsServer)
        {
            playerNicknames.Add(nickname); // Sunucu taraf�nda nickname'i listeye ekle
        }
    }

    private void OnPlayerListChanged(NetworkListEvent<FixedString32Bytes> changeEvent)
    {
        // Client taraf�nda liste de�i�ikliklerini i�leyin
        UI_Manager.Instance.UpdatePlayerListUI(playerNicknames);
    }

    // Mevcut oyuncular�n listesini d�nd�r
    public IEnumerable<string> GetAllNicknames()
    {
        List<string> nicknames = new List<string>();
        foreach (var nickname in playerNicknames)
        {
            nicknames.Add(nickname.ToString());
        }
        return nicknames;
    }
}
