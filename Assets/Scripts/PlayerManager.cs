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
            // Oyuncular listeye eklendiðinde client'lara bilgi gönder
            playerNicknames.OnListChanged += OnPlayerListChanged;
        }
    }

    public void AddPlayerNickname(string nickname)
    {
        if (IsServer)
        {
            playerNicknames.Add(nickname); // Sunucu tarafýnda nickname'i listeye ekle
        }
    }

    private void OnPlayerListChanged(NetworkListEvent<FixedString32Bytes> changeEvent)
    {
        // Client tarafýnda liste deðiþikliklerini iþleyin
        UI_Manager.Instance.UpdatePlayerListUI(playerNicknames);
    }

    // Mevcut oyuncularýn listesini döndür
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
