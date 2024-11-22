using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System.Linq; // LINQ i�in gerekli


public class Race_Manager : NetworkBehaviour
{
    public static Race_Manager Instance { get; private set; }

    public NetworkList<ulong> activePlayers = new NetworkList<ulong>(); // Yar��a kat�lan oyuncular

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
        isRaceActive = false;

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

    private void OnClientConnected(ulong clientId)
    {
        if (IsServer)
        {
            // Yeni ba�lanan oyuncuyu listeye ekle
            if (!activePlayers.Contains(clientId))
                activePlayers.Add(clientId);
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (IsServer)
        {
            // Ayr�lan oyuncuyu listeden ��kar
            if (activePlayers.Contains(clientId))
                activePlayers.Remove(clientId);
        }
    }

    public bool IsPlayerReady(ulong playerId)
    {
        return playersReady.Contains(playerId);
    }

    [ServerRpc(RequireOwnership = false)] 
    public void AddPlayerToRaceServerRpc(ulong playerId)
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
        if (playersReady.Contains(playerId))
        {
            Debug.LogWarning($"Oyuncu {playerId} zaten haz�r.");
            return;
        }

        //playersReady.Add(playerId);
        playersReady.Add(NetworkManager.Singleton.LocalClientId);
        // Haz�r listesini di�er istemcilere g�nder
        SendPlayersReadyList();


        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(playerId, out var client))
        {
            var playerObject = client.PlayerObject;
            if (playerObject != null)
            {
                playerObject.GetComponent<SnowboardController>().DisableControls();
                //playerObject.transform.SetPositionAndRotation(raceStartPosition.position, Quaternion.identity);
                Debug.Log($"Oyuncu {playerId} ba�lang�� pozisyonuna ���nland�.");
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TeleportPlayerToStartServerRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        {
            // Oyuncunun GameObject'ini al
            var playerObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
            // Ba�lang�� pozisyonuna ���nla
            playerObject.transform.position = raceStartPosition.position;
            Debug.Log($"Oyuncu {clientId} ba�lang�� noktas�na ���nland�.");
        }
    }

    [ClientRpc]
    private void UpdatePlayerPositionClientRpc(ulong clientId, Vector3 newPosition)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            var playerObject = NetworkManager.Singleton.LocalClient.PlayerObject;
            playerObject.transform.position = newPosition;
        }
    }

    private void SendPlayersReadyList()
    {
        // NetworkList'in i�eriklerini manuel olarak bir listeye aktarma
        List<ulong> playersReadyList = new List<ulong>();

        foreach (var playerId in playersReady)
        {
            playersReadyList.Add(playerId);
        }

        // Listeyi diziye �evir ve RPC metodunu �a��r
        UpdatePlayersReadyClientRpc(playersReadyList.ToArray());
    }


    [ClientRpc]
    private void UpdatePlayersReadyClientRpc(ulong[] readyPlayers)
    {
        Debug.Log($"Haz�r oyuncular g�ncellendi: {string.Join(", ", readyPlayers)}");
    }


    private void OnPlayersReadyChanged(NetworkListEvent<ulong> changeEvent)
    {
        Debug.Log($"Haz�r oyuncu say�s�: {playersReady.Count}");
        if (!isRaceActive && playersReady.Count > 0)
        {
            StartCountdown();
        }
    }


    private void StartCountdown()
    {
        StartCoroutine(StartRaceCountdown());
    }


    private IEnumerator StartRaceCountdown()
    {
        float remainingTime = countdownTime;
        while (remainingTime > 0)
        {
            Debug.Log($"Yar�� ba�l�yor: {remainingTime} saniye kald�!");
            yield return new WaitForSeconds(1f);
            remainingTime--;
        }

        StartRaceServerRpc();
    }


    [ServerRpc(RequireOwnership = false)]
    public void StartRaceServerRpc()
    {
        isRaceEnd = false;
        isRaceActive = true;

        foreach (ulong playerId in playersReady)
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(playerId, out var client))
            {
                var controller = client.PlayerObject.GetComponent<SnowboardController>();
                controller?.EnableControls();
                Debug.Log($"Oyuncu {playerId} yar��a ba�lad�.");
            }
        }

        playersReady.Clear();
    }

    [ClientRpc]
    private void UpdatePlayerRaceStateClientRpc(ulong playerId, bool isInRace)
    {
        if (playerId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log($"Yar�� durumu g�ncellendi: Oyuncu {playerId} yar��ta m�? {isInRace}");
            Race_Manager.Instance.isRaceActive = isInRace;
        }
    }

    //public void EndRace(ulong winnerId)
    //{
    //    isRaceActive = false;

    //    Debug.Log($"Yar�� sona erdi! Birinci oyuncu: {winnerId}");

    //    // TPS kameray� aktif et
    //    var winner = NetworkManager.Singleton.ConnectedClients[winnerId].PlayerObject.transform;
    //    ActivateTpsCamera(winner);

    //    StartCoroutine(EndRaceCoroutine());
    //}

    [ServerRpc(RequireOwnership = false)]
    public void CheckWinnerServerRpc(ulong winnerId)
    {
        if (NetworkManager.Singleton.ConnectedClients.ContainsKey(winnerId))
        {
            var winner = NetworkManager.Singleton.ConnectedClients[winnerId].PlayerObject.transform;

            // Kazanan bilgisi istemcilere iletiliyor
            AnnounceWinnerClientRpc(winnerId);
        }
    }

    [ClientRpc]
    private void AnnounceWinnerClientRpc(ulong winnerId)
    {
        isRaceActive = false;
        Debug.Log($"Kazanan oyuncu ID: {winnerId}");
        // �lgili i�lemler (�rne�in, UI g�ncellemesi)
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetWinnerServerRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        {
            Debug.Log($"Kazanan oyuncu: {clientId}");
            var winner = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.transform;
            tpsCameraPrefab.transform.position = winner.position + new Vector3(0, 5, -10); // Kamera pozisyonunu ayarlayin

            ActivateTpsCamera(winner.transform);
        }
        else
        {
            Debug.LogWarning($"ClientId {clientId} bulunamad�.");
        }
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
            tpsCameraInstance.transform.position = winner.position + new Vector3(0, 5, -10); // Kameray� oyuncunun arkas�na yerle�tir
            tpsCameraInstance.transform.LookAt(winner); // Kameray� oyuncuya do�ru bakt�r

            // Kamera her frame'de oyuncuyu takip etsin
            StartCoroutine(FollowPlayer(winner));
        }
    }

    private IEnumerator FollowPlayer(Transform winner)
    {
        float followDuration = 5f; // Kameran�n ne kadar s�re boyunca takip edece�ini belirleyin
        float timeElapsed = 0f;

        while (timeElapsed < followDuration && tpsCameraInstance != null)
        {
            tpsCameraInstance.transform.position = Vector3.Lerp(tpsCameraInstance.transform.position, winner.position + new Vector3(0, 5, -10), Time.deltaTime * 5f);
            tpsCameraInstance.transform.LookAt(winner); // Kamera oyuncuyu takip etmeye devam edecek

            timeElapsed += Time.deltaTime; // Ge�en zaman� art�r
            yield return null; // Bir sonraki frame'e ge�
        }

        // S�re doldu�unda kamera takip etmeyi durdurabiliriz
        Destroy(tpsCameraInstance); // Kameray� yok et (veya ba�ka bir �ey yap)
    }

    public void ResetRace()
    {
        isRaceEnd = true;
        isRaceActive = false;
        activePlayers.Clear();
        disqualifiedPlayers.Clear();
        playersReady.Clear();
        Debug.Log("Yar�� s�f�rland�.");
        //isRaceActive = true;
    }

}
