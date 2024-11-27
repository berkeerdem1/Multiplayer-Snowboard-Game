using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using DG.Tweening;
using TMPro;


public class UI_Manager : MonoBehaviour
{
    public static UI_Manager Instance;

    public GameObject nicknamePanel;   // Nickname'lerin gösterileceði panel
    public GameObject nicknamePrefab; // Her oyuncu için oluþturulacak Text prefab'i

    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject leaveServerButton;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject title;
    [SerializeField] private RectTransform titleObject;
    [SerializeField] private Transform targetPos;
    [SerializeField] private float duration = 1f;

   private List<GameObject> nicknameObjects = new List<GameObject>();


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
        nicknamePanel.SetActive(false);
        pausePanel.SetActive(false);
        controlsPanel.SetActive(false);
        InvokeRepeating(nameof(UpdateNicknamePanel), 0f, 5f); // 5 saniyede bir paneli güncelle
        MoveUIObject();
    }

    private void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) && GameManager.Instance.isInGame)
        {
            TogglePausePanel();
        }
    }

    void MoveUIObject()
    {
        // Dünya pozisyonunu (world position) anchor pozisyona dönüþtür
        Vector2 targetAnchorPos = titleObject.parent.InverseTransformPoint(targetPos.position);

        // UI objesini anchor pozisyonuna taþý
        titleObject.DOAnchorPos(targetAnchorPos, duration).SetEase(Ease.InOutElastic);
    }

    public void ToggleTitle()
    {
        title.SetActive(!title.activeSelf);

    }
    public void ControlsButton()
    {
        ToggleContolsPanel();
    }

    public void ReturnPausePanelButton()
    {
        ToggleContolsPanel();
    }

    private void ToggleContolsPanel()
    {
        controlsPanel.SetActive(!controlsPanel.activeSelf);
    }

    private void TogglePausePanel()
    {
        pausePanel.SetActive(!pausePanel.activeSelf);
    }

    public void LeaveServerButton()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            // Oyuncunun baðlantýsýný kes

            Debug.Log("Oyuncu serverdan ayrýldý.");
            pausePanel.SetActive(false);
            Nickname_Manager.Instance.ResetPanels();
            ToggleTitle();
            GameManager.Instance.isInGame = false;
        }
        else
        {
            Debug.LogError("NetworkManager mevcut deðil!");
        }

        // Oyuncunun ana menüye yönlendirilmesi gibi baþka iþlemler
        // SceneManager.LoadScene("MainMenu");
    }

    public void QuitButton()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            GameManager.Instance.isInGame = false;
            Debug.Log("Oyuncu oyundan cikti.");
            pausePanel.SetActive(false);
            Application.Quit();
        }
        else
        {
            Debug.LogError("NetworkManager mevcut deðil!");
        }

        // Oyuncunun ana menüye yönlendirilmesi gibi baþka iþlemler
        // SceneManager.LoadScene("MainMenu");
    }

    public void UpdateNicknamePanel()
    {
        if (PlayersNickname_Controller.Instance == null)
        {
            Debug.Log("PlayerManager mevcut deðil, panel güncellenemedi!");
            return; 
        }

        var nicknames = PlayersNickname_Controller.Instance.GetAllNicknames();

        Debug.Log("Panel güncelleniyor. Nickname'ler:");
        foreach (var nickname in nicknames)
        {
            Debug.Log(nickname); // Panelde gösterilecek tüm nickname'leri kontrol et
        }

        // Eski nickname'leri temizle
        foreach (var obj in nicknameObjects)
        {
            Destroy(obj);
        }
        nicknameObjects.Clear();

        // Yeni nickname'leri ekle
        foreach (var nickname in nicknames)
        {
            if (newNickname != null)
            {
                GameObject newNickname;
            }
            newNickname = Instantiate(nicknamePrefab, nicknamePanel.transform);

            // Eðer bu nickname oyuncunun kendisine aitse "(You)" ekle
            if (nickname == Nickname_Manager.Instance.nickname)
            {
                newNickname.GetComponent<Text>().text = $"{nickname} (You)";
            }
            else
            {
                newNickname.GetComponent<Text>().text = nickname;
            }

            nicknameObjects.Add(newNickname);
        }

        Debug.Log($"Panelde toplam {nicknameObjects.Count} oyuncu gösteriliyor."); // Paneldeki toplam nickname sayýsýný yazdýr
    }

    public void ToggleNicknamePanel()
    {
        // Panelin aktiflik durumunu deðiþtir
        nicknamePanel.SetActive(!nicknamePanel.activeSelf);
    }

    public void RemovePlayerNickName(GameObject player)
    {
        nicknameObjects.Remove(player);
        Destroy(newNickname);
    }
}
