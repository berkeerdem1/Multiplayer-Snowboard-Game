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
        InvokeRepeating(nameof(CheckAndUpdateNicknames), 0f, 5f); 
    }

    // Eksik nickname'leri ekler
    private void CheckAndUpdateNicknames()
    {
        if (PlayerManager.Instance == null) return;

        var allNicknames = PlayerManager.Instance.GetAllNicknames();

        foreach (var nickname in allNicknames)
        {
            // E�er panelde bu nickname yoksa, ekle
            if (!IsNicknameInPanel(nickname))
            {
                AddPlayerToList(nickname);
            }
        }
    }

    private bool IsNicknameInPanel(string nickname)
    {
        foreach (var obj in nicknameObjects)
        {
            if (obj.GetComponent<Text>().text == nickname)
            {
                return true;
            }
        }
        return false;
    }


    // Oyuncu listesini UI'ya ekleme
    public void AddPlayerToList(string nickname)
    {
        // Yeni bir nickname UI'si olu�tur
        GameObject newNickname = Instantiate(nicknamePrefab, nicknamePanel.transform);
        newNickname.GetComponent<Text>().text = nickname; // Nickname'i ayarla
        nicknameObjects.Add(newNickname);
    }

    public void UpdatePlayerListUI(NetworkList<FixedString32Bytes> nicknames)
    {
        // Eski nickname UI'lar�n� temizle
        foreach (var obj in nicknameObjects)
        {
            Destroy(obj);
        }

        nicknameObjects.Clear();

        // Yeni nickname'ler i�in UI olu�tur
        foreach (var nickname in nicknames)
        {
            GameObject newNickname = Instantiate(nicknamePrefab, nicknamePanel.transform);
            newNickname.GetComponent<Text>().text = nickname.ToString();
            nicknameObjects.Add(newNickname);
        }
    }
    public void ToggleNicknamePanel()
    {
        // Panelin aktiflik durumunu de�i�tir
        nicknamePanel.SetActive(!nicknamePanel.activeSelf);
    }
}
