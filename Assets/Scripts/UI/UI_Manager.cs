using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UI_Manager : MonoBehaviour
{
    public static UI_Manager Instance;

    public GameObject nicknamePanel;   // Nicknames panel
    public GameObject nicknamePrefab; // Text prefab to be created for each player

    [SerializeField] private GameObject _playButton;
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private GameObject _leaveServerButton;
    [SerializeField] private GameObject _controlsPanel;
    [SerializeField] private GameObject _title; // Game title
    [SerializeField] private RectTransform _titleObject;
    [SerializeField] private Transform _titleTargetPos; // Game title
    [SerializeField] private float _duration = 1f;

    private List<GameObject> _nicknameObjects = new List<GameObject>();
    
    GameObject newNickname;

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
        UIObjectsDisabled();

        _playButton.SetActive(true); // To turn the play button back on after all UI objects are closed

        InvokeRepeating(nameof(UpdateNicknamePanel), 0f, 5f); // Update panel every 5 seconds
        MoveTitle();
    }

    private void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) && GameManager.Instance.isInGame)
        {
            TogglePausePanel();
        }
    }

    void MoveTitle()
    {
        Vector2 targetAnchorPos = _titleObject.parent.InverseTransformPoint(_titleTargetPos.position);
        _titleObject.DOAnchorPos(targetAnchorPos, _duration).SetEase(Ease.InOutElastic);
    }

    public void ToggleTitle()
    {
        _title.SetActive(!_title.activeSelf);

    }
    public void ControlsButton() // Controls button
    {
        ToggleContolsPanel();
    }

    public void ReturnPausePanelButton() // Return button
    {
        ToggleContolsPanel();
    }

    private void ToggleContolsPanel()
    {
        _controlsPanel.SetActive(!_controlsPanel.activeSelf);
    }

    private void TogglePausePanel()
    {
        _pausePanel.SetActive(!_pausePanel.activeSelf);
    }

    public void PlayGame()
    {
        _playButton.SetActive(false);
        Nickname_Manager.Instance.PlayGameUISet();
    }

    public void LeaveServerButton() // Main Menu (leave thr server) button
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            // Disconnect player

            Debug.Log("Player left the server.");
            _pausePanel.SetActive(false);
            Nickname_Manager.Instance.ResetPanels();
            ToggleTitle();
            GameManager.Instance.isInGame = false;
        }
        else
        {
            Debug.LogError("NetworkManager not available!");
        }
    }

    public void QuitButton() // Quit button
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            GameManager.Instance.isInGame = false;

            Debug.Log("Player left the GAME.");

            _pausePanel.SetActive(false);
            Application.Quit();
        }
        else
        {
            Debug.LogError("NetworkManager not available!");
        }
    }

    public void ToggleNicknamePanel()
    {
        nicknamePanel.SetActive(!nicknamePanel.activeSelf);
    }

    public void RemovePlayerNickName(GameObject player)
    {
        _nicknameObjects.Remove(player);
        Destroy(newNickname);
    }

    public void UpdateNicknamePanel()
    {
        if (PlayersNickname_Controller.Instance == null)
        {
            Debug.Log("PlayerManager not available, panel update failed!");
            return; 
        }

        var nicknames = PlayersNickname_Controller.Instance.GetAllNicknames();

        Debug.Log("Updating the panel. Nicknames:");
        foreach (var nickname in nicknames)
        {
            Debug.Log(nickname); // Control all nicknames to be displayed on the panel
        }

        // Clear old nicknames
        foreach (var obj in _nicknameObjects)
        {
            Destroy(obj);
        }
        _nicknameObjects.Clear();

        // New nicknames add
        foreach (var nickname in nicknames)
        {
            if (newNickname != null)
            {
                GameObject newNickname;
            }
            newNickname = Instantiate(nicknamePrefab, nicknamePanel.transform);

            // If this nickname belongs to the player, add "(You)"
            if (nickname == Nickname_Manager.Instance.nickname)
            {
                newNickname.GetComponent<Text>().text = $"{nickname} (You)";
            }
            else
            {
                newNickname.GetComponent<Text>().text = nickname;
            }

            _nicknameObjects.Add(newNickname);
        }

        Debug.Log($"The panel shows a total of {_nicknameObjects.Count} players."); // Print total number of nicknames in panel
    }

    public void UIObjectsDisabled()
    {
        nicknamePanel.SetActive(false);
        _pausePanel.SetActive(false);
        _controlsPanel.SetActive(false);
        _playButton.SetActive(false);
    }
}
