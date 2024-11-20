using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    public static UI_Manager Instance;

    public GameObject nicknamePanel;   // Nickname'lerin g�sterilece�i panel
    public GameObject nicknamePrefab; // Her oyuncu i�in olu�turulacak Text prefab'i

    private List<GameObject> nicknameObjects = new List<GameObject>();


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
        InvokeRepeating(nameof(UpdateNicknamePanel), 0f, 5f); // 5 saniyede bir paneli g�ncelle
    }

    public void UpdateNicknamePanel()
    {
        if (PlayersNickname_Controller.Instance == null)
        {
            Debug.Log("PlayerManager mevcut de�il, panel g�ncellenemedi!");
            return; 
        }

        var nicknames = PlayersNickname_Controller.Instance.GetAllNicknames();

        Debug.Log("Panel g�ncelleniyor. Nickname'ler:");
        foreach (var nickname in nicknames)
        {
            Debug.Log(nickname); // Panelde g�sterilecek t�m nickname'leri kontrol et
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
            GameObject newNickname = Instantiate(nicknamePrefab, nicknamePanel.transform);

            // E�er bu nickname oyuncunun kendisine aitse "(You)" ekle
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

        Debug.Log($"Panelde toplam {nicknameObjects.Count} oyuncu g�steriliyor."); // Paneldeki toplam nickname say�s�n� yazd�r
    }

    public void ToggleNicknamePanel()
    {
        // Panelin aktiflik durumunu de�i�tir
        nicknamePanel.SetActive(!nicknamePanel.activeSelf);
    }
}
