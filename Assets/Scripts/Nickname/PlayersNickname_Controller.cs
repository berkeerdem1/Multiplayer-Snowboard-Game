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
            InvokeRepeating(nameof(CheckForNewPlayers), 0f, 5f); // Check players every 5 seconds
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
        // Notify UI_Manager of nickname changes
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
                playerNicknames.Add(nickname); //Add to list on server side
                Debug.Log($"Nickname added: {nickname}"); // Check the nickname addition process
            }
            else
            {
                Debug.Log($"Nickname already exists: {nickname}"); // If already added, let me know
            }
        }
    }

    private void CheckForNewPlayers()
    {
        // Control all players in the game
        foreach (var player in FindObjectsOfType<PlayerNicknameDisplay>())
        {
            if (!playerNicknames.Contains(player.GetNickname()))
            {
                // If the player is not on the list, add player
                playerNicknames.Add(player.GetNickname());
            }
        }
    }

    public IEnumerable<string> GetAllNicknames()
    {
        // Return all player nicknames
        List<string> nicknames = new List<string>();
        foreach (var nickname in playerNicknames)
        {
            nicknames.Add(nickname.ToString());
        }

        Debug.Log("All nicknames:");
        foreach (var name in nicknames)
        {
            Debug.Log(name); // Print all nicknames added to the list
        }

        return nicknames;
    }

    public void RemoveNickName(FixedString32Bytes nickname)
    {
        playerNicknames.Remove(nickname);
    }
}
