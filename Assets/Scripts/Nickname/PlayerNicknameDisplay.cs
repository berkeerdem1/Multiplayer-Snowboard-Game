using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Collections;

public class PlayerNicknameDisplay : NetworkBehaviour
{
    public Text nicknameText;  // Nickname on the player
    public Transform player;   // Player's Transform
    public Vector3 offset;     // Position offset of text

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
            nicknameText.gameObject.SetActive(false); // Hide your own nickname
            SetNickname(); // Set your own nickname

            string nickname = Nickname_Manager.Instance.nickname; // Get player nickname
            PlayersNickname_Controller.Instance.AddPlayerNickname(nickname);   // List add

            // Set your nickname as "(You)"
            nicknameText.text = $"{nickname} (You)";

            SubmitNicknameToServerRpc(nickname); // Send to server
        }

        playerNickname.OnValueChanged += OnNicknameChanged;

        // Set nickname even if initial value change has not occurred
        if (!string.IsNullOrEmpty(playerNickname.Value.ToString()) && !IsOwner)
        {
            nicknameText.text = playerNickname.Value.ToString();
        }
    }

    private void SetNickname()
    {
        // Get player's nickname and pass it to `NetworkVariable`
        string nickname = Nickname_Manager.Instance.nickname; // Get nickname from Nickname_Manager
        playerNickname.Value = nickname;
    }

    public FixedString32Bytes GetNickname()
    {
        return playerNickname.Value; // Player's nickname return
    }

    private void OnNicknameChanged(FixedString32Bytes oldValue, FixedString32Bytes newValue)
    {
        // If it's not yours nickname just show the name
        if (!IsOwner)
        {
            nicknameText.text = newValue.ToString();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitNicknameToServerRpc(string nickname)
    {
        // Add to PlayerManager on server side
        if (PlayersNickname_Controller.Instance != null)
        {
            PlayersNickname_Controller.Instance.AddPlayerNickname(nickname);
        }
    }

    private void OnDestroy()
    {
        if (IsOwner)
        {
            FixedString32Bytes nickname = Nickname_Manager.Instance.nickname;
            UI_Manager.Instance.RemovePlayerNickName(gameObject);
        }
    }
}
