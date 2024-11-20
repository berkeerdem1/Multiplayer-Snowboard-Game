using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
public class LobbyUI : MonoBehaviour
{
    public LobbyManager lobbyManager;
    public Button createLobbyButton;
    public Button joinLobbyButton;
    public InputField lobbyNameInput;
    public InputField lobbyIdInput;
    public Text lobbyListText;

    private void Start()
    {


        createLobbyButton.onClick.AddListener(() =>
        {
            lobbyManager.CreateLobby(lobbyNameInput.text, 4); // Maksimum 4 oyunculu lobi oluþtur
        });

        joinLobbyButton.onClick.AddListener(() =>
        {
            lobbyManager.JoinLobby(lobbyIdInput.text);
        });
    }

    public async void ListLobbies()
    {
        try
        {
            var lobbies = await LobbyService.Instance.QueryLobbiesAsync();
            lobbyListText.text = "";

            foreach (var lobby in lobbies.Results)
            {
                lobbyListText.text += $"Lobby: {lobby.Name}, Players: {lobby.Players.Count}/{lobby.MaxPlayers}\n";
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Lobby listing failed: {e}");
        }
    }

    private async Task CreateLobby()
    {
        string lobbyName = "Test Lobby"; // Lobi adý
        int maxPlayers = 4; // Maksimum oyuncu sayýsý

        await lobbyManager.CreateLobby(lobbyName, maxPlayers);
    }
}
