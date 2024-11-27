using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using Unity.Services.Authentication;

public class PlayersNickname_Controller : NetworkBehaviour
{
    private NetworkList<FixedString32Bytes> playerNicknames = new NetworkList<FixedString32Bytes>();

    public static PlayersNickname_Controller Instance;

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

    private void Start()
    {
        if (IsServer)
        {
            playerNicknames.OnListChanged += OnPlayerNicknamesChanged;
            InvokeRepeating(nameof(CheckForNewPlayers), 0f, 5f); // 5 saniyede bir oyuncular� kontrol et
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
        }
    }

    private void OnPlayerNicknamesChanged(NetworkListEvent<FixedString32Bytes> changeEvent)
    {
        // Nickname de�i�imlerini UI_Manager'a bildir
        if (UI_Manager.Instance != null)
        {
            UI_Manager.Instance.UpdateNicknamePanel();
        }
    }

    public void AddPlayerNickname(string nickname)
    {
        if (IsServer)
        {
            if (!playerNicknames.Contains(nickname))
            {
                playerNicknames.Add(nickname); // Sunucu taraf�nda listeye ekle
                Debug.Log($"Nickname eklendi: {nickname}"); // Nickname ekleme i�lemini kontrol et
            }
            else
            {
                Debug.Log($"Nickname zaten mevcut: {nickname}"); // E�er zaten eklenmi�se bilgi ver
            }
        }
    }

    private void CheckForNewPlayers()
    {
        // Oyundaki t�m oyuncular� kontrol et
        foreach (var player in FindObjectsOfType<PlayerNicknameDisplay>())
        {
            if (!playerNicknames.Contains(player.GetNickname()))
            {
                // E�er oyuncu listede yoksa ekle
                playerNicknames.Add(player.GetNickname());
            }
        }
    }

    public IEnumerable<string> GetAllNicknames()
    {
        // T�m oyuncu nicknamelerini d�nd�r
        List<string> nicknames = new List<string>();
        foreach (var nickname in playerNicknames)
        {
            nicknames.Add(nickname.ToString());
        }

        Debug.Log("T�m nickname'ler:");
        foreach (var name in nicknames)
        {
            Debug.Log(name); // Listeye eklenen t�m nickname'leri yazd�r
        }

        return nicknames;
    }

    public void RemoveNickName(FixedString32Bytes nickname)
    {
        playerNicknames.Remove(nickname);
    }
}
