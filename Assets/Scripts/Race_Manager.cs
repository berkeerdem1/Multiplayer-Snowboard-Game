using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Race_Manager : NetworkBehaviour
{
    public static Race_Manager Instance { get; private set; }

    public List<ulong> activePlayers = new List<ulong>(); // Yar��a kat�lan oyuncular
    private HashSet<ulong> disqualifiedPlayers = new HashSet<ulong>(); // Diskalifiye olan oyuncular

    public Transform raceStartPosition; // Ba�lang�� �izgisi pozisyonu
    public float countdownTime = 10f; // Geri say�m s�resi
    public bool isRaceActive = false; // Yar�� ba�lad� m�?
    public bool isRaceEnd = false; // taris bitti mi?
    private NetworkList<ulong> playersReady = new NetworkList<ulong>(); // Yar��a kat�lan oyuncular�n ID'leri


    public GameObject tpsCameraPrefab; // TPS kamera prefab'�
    private GameObject tpsCameraInstance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (IsServer)
        {
            playersReady = new NetworkList<ulong>();
            Debug.Log("playersReady NetworkList ba�ar�yla sunucuda olu�turuldu.");
        }

        playersReady.OnListChanged += OnPlayersReadyChanged;
    }

    private void OnDestroy()
    {
        // Dinleyiciyi temizleyin
        if (IsServer)
        {
            playersReady.OnListChanged -= OnPlayersReadyChanged;
        }
    }

    public void AddPlayerToRace(ulong playerId)
    {
        if (disqualifiedPlayers.Contains(playerId))
        {
            Debug.Log($"Oyuncu {playerId} diskalifiye oldu�u i�in yar��a tekrar kat�lamaz.");
            return;
        }

        if (!activePlayers.Contains(playerId))
        {
            activePlayers.Add(playerId);
            Debug.Log($"Oyuncu {playerId} yar��a kat�ld�.");
        }
    }

    public void RemovePlayerFromRace(ulong playerId)
    {
        if (activePlayers.Contains(playerId))
        {
            activePlayers.Remove(playerId);
            disqualifiedPlayers.Add(playerId); // Diskalifiye edildi�i i�in listeye ekle
            Debug.Log($"Oyuncu {playerId} diskalifiye edildi ve yar��tan ��kar�ld�.");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerReadyServerRpc(ulong playerId)
    {
        Debug.Log($"PlayerReadyServerRpc �a�r�ld�! Oyuncu ID: {playerId}");

        if (!playersReady.Contains(playerId))
        {
            playersReady.Add(playerId);
            Debug.Log($"Oyuncu {playerId} playersReady listesine eklendi. Mevcut liste: {string.Join(", ", playersReady)}");

            // Oyuncuyu ba�lang�� pozisyonuna ���nla
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(playerId, out var client))
            {
                var playerObject = client.PlayerObject;
                if (playerObject != null)
                {
                    playerObject.GetComponent<SnowboardController>().DisableControls();
                    var transform = playerObject.transform;
                    transform.position = raceStartPosition.position; // Ba�lang�� pozisyonu
                    transform.rotation = Quaternion.identity; // Varsay�lan rotasyon
                    Debug.Log($"Oyuncu {playerId} ba�lang�� pozisyonuna ���nland�.");
                }
                else
                {
                    Debug.LogWarning($"PlayerObject bulunamad�! Oyuncu ID: {playerId}");
                }
            }
            else
            {
                Debug.LogWarning($"ConnectedClients'ta oyuncu bulunamad�! Oyuncu ID: {playerId}");
            }
        }
    }




    private void OnPlayersReadyChanged(NetworkListEvent<ulong> changeEvent)
    {
        Debug.Log("OnPlayersReadyChanged tetiklendi.");
        Debug.Log($"Haz�r oyuncular: {string.Join(", ", playersReady)}");
        Debug.Log($"Haz�r oyuncu say�s�: {playersReady.Count}");

        if (!isRaceActive && playersReady.Count > 0 && playersReady.Count == NetworkManager.Singleton.ConnectedClients.Count)
        {
            Debug.Log("T�m oyuncular haz�r. Yar�� ba�l�yor!");
            StartCountdown();
        }
        else if (isRaceActive)
        {
            Debug.Log("Yar�� zaten aktif. Yeni oyuncular eklenmeyecek.");
        }
        else
        {
            Debug.LogWarning("T�m oyuncular hen�z haz�r de�il.");
        }
    }



    private void StartCountdown()
    {
        isRaceActive = true;
        Debug.Log("StartCountdown �a�r�ld�. Geri say�m ba�lat�l�yor.");
        StartCoroutine(StartRaceCountdown());
    }


    private IEnumerator StartRaceCountdown()
    {
        float remainingTime = countdownTime;
        Debug.Log($"StartRaceCountdown ba�lad�. Geri say�m s�resi: {countdownTime} saniye.");

        while (remainingTime > 0)
        {
            Debug.Log($"Yar�� ba�l�yor: {remainingTime} saniye kald�!");
            yield return new WaitForSeconds(1f);
            remainingTime--;
        }

        Debug.Log("Geri say�m bitti. Yar�� ba�lat�l�yor!");
        StartRaceServerRpc();
    }


    [ServerRpc(RequireOwnership = false)]
    public void StartRaceServerRpc()
    {
        isRaceEnd = false;
        Debug.Log("StartRaceServerRpc �a�r�ld�. Yar�� ba�lat�ld�!");

        foreach (ulong playerId in playersReady)
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(playerId, out var client))
            {
                var playerObject = client.PlayerObject;
                playerObject.GetComponent<Player_Disqualify>().isInRace = true;
                var controller = playerObject.GetComponent<SnowboardController>();

                if (controller != null)
                {
                    controller.EnableControls();
                    Debug.Log($"Oyuncu {playerId} yar��a kat�ld� ve kontrolleri a��ld�.");
                }
                else
                {
                    Debug.LogError($"Oyuncu {playerId} i�in SnowboardController bulunamad�!");
                }
            }
            else
            {
                Debug.LogError($"PlayerID {playerId} i�in NetworkManager'da bir client bulunamad�.");
            }
        }

        Debug.Log("Yar��a dahil olan t�m oyuncular�n kontrolleri a��ld�.");
        playersReady.Clear();
    }

    public void EndRace(ulong winnerId)
    {
        Debug.Log($"Yar�� sona erdi! Birinci oyuncu: {winnerId}");

        // TPS kameray� aktif et
        var winner = NetworkManager.Singleton.ConnectedClients[winnerId].PlayerObject.transform;
        ActivateTpsCamera(winner);

        StartCoroutine(EndRaceCoroutine());
    }

    private void ActivateTpsCamera(Transform winner)
    {
        if (tpsCameraPrefab != null)
        {
            // E�er kamera zaten varsa, �nceki kameray� yok et
            if (tpsCameraInstance != null)
            {
                Destroy(tpsCameraInstance);
            }

            tpsCameraInstance = Instantiate(tpsCameraPrefab);
            tpsCameraInstance.transform.position = winner.position + new Vector3(0, 5, -10); // Kameray� oyuncunun arkas�na konumland�r
            tpsCameraInstance.transform.LookAt(winner); // Kameray� oyuncuya bakt�r

            // Kamera her frame'de oyuncuyu takip etsin
            StartCoroutine(FollowPlayer(winner));
        }
    }

    private IEnumerator FollowPlayer(Transform winner)
    {
        while (tpsCameraInstance != null)
        {
            // Kameray� her frame'de oyuncunun konumuna g�re ayarl�yoruz
            tpsCameraInstance.transform.position = Vector3.Lerp(tpsCameraInstance.transform.position, winner.position + new Vector3(0, 5, -10), Time.deltaTime * 5f); // Kamera oyuncunun arkas�nda
            tpsCameraInstance.transform.LookAt(winner); // Kamera her zaman oyuncuya bakacak

            yield return null; // Bir sonraki frame'e ge�i� yap
        }
    }

    private IEnumerator EndRaceCoroutine()
    {
        yield return new WaitForSeconds(5f); // 5 saniye TPS kamera g�ster
        if (tpsCameraInstance != null)
        {
            Destroy(tpsCameraInstance);
        }

        ResetRace();
    }

    public void ResetRace()
    {
        isRaceEnd = true;
        activePlayers.Clear();
        disqualifiedPlayers.Clear();
        Debug.Log("Yar�� s�f�rland�.");
        isRaceActive = true;
    }

}
