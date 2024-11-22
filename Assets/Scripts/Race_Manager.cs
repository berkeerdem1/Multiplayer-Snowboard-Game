using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System.Linq; // LINQ için gerekli


public class Race_Manager : NetworkBehaviour
{
    public static Race_Manager Instance { get; private set; }

    public NetworkList<ulong> activePlayers = new NetworkList<ulong>(); // Yarýþa katýlan oyuncular

    private HashSet<ulong> disqualifiedPlayers = new HashSet<ulong>(); // Diskalifiye olan oyuncular

    public Transform raceStartPosition; // Baþlangýç çizgisi pozisyonu
    public float countdownTime = 10f; // Geri sayým süresi
    public bool isRaceActive = false; // Yarýþ baþladý mý?
    public bool isRaceEnd = false; // taris bitti mi?
    private NetworkList<ulong> playersReady = new NetworkList<ulong>(); // Yarýþa katýlan oyuncularýn ID'leri


    public GameObject tpsCameraPrefab; // TPS kamera prefab'ý
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
            Debug.Log("playersReady NetworkList baþarýyla sunucuda oluþturuldu.");
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
            // Yeni baðlanan oyuncuyu listeye ekle
            if (!activePlayers.Contains(clientId))
                activePlayers.Add(clientId);
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (IsServer)
        {
            // Ayrýlan oyuncuyu listeden çýkar
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
            Debug.Log($"Oyuncu {playerId} diskalifiye olduðu için yarýþa tekrar katýlamaz.");
            return;
        }

        if (!activePlayers.Contains(playerId))
        {
            activePlayers.Add(playerId);
            Debug.Log($"Oyuncu {playerId} yarýþa katýldý.");
        }
    }

    public void RemovePlayerFromRace(ulong playerId)
    {
        if (activePlayers.Contains(playerId))
        {
            activePlayers.Remove(playerId);
            disqualifiedPlayers.Add(playerId); // Diskalifiye edildiði için listeye ekle
            Debug.Log($"Oyuncu {playerId} diskalifiye edildi ve yarýþtan çýkarýldý.");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerReadyServerRpc(ulong playerId)
    {
        if (playersReady.Contains(playerId))
        {
            Debug.LogWarning($"Oyuncu {playerId} zaten hazýr.");
            return;
        }

        //playersReady.Add(playerId);
        playersReady.Add(NetworkManager.Singleton.LocalClientId);
        // Hazýr listesini diðer istemcilere gönder
        SendPlayersReadyList();


        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(playerId, out var client))
        {
            var playerObject = client.PlayerObject;
            if (playerObject != null)
            {
                playerObject.GetComponent<SnowboardController>().DisableControls();
                //playerObject.transform.SetPositionAndRotation(raceStartPosition.position, Quaternion.identity);
                Debug.Log($"Oyuncu {playerId} baþlangýç pozisyonuna ýþýnlandý.");
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
            // Baþlangýç pozisyonuna ýþýnla
            playerObject.transform.position = raceStartPosition.position;
            Debug.Log($"Oyuncu {clientId} baþlangýç noktasýna ýþýnlandý.");
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
        // NetworkList'in içeriklerini manuel olarak bir listeye aktarma
        List<ulong> playersReadyList = new List<ulong>();

        foreach (var playerId in playersReady)
        {
            playersReadyList.Add(playerId);
        }

        // Listeyi diziye çevir ve RPC metodunu çaðýr
        UpdatePlayersReadyClientRpc(playersReadyList.ToArray());
    }


    [ClientRpc]
    private void UpdatePlayersReadyClientRpc(ulong[] readyPlayers)
    {
        Debug.Log($"Hazýr oyuncular güncellendi: {string.Join(", ", readyPlayers)}");
    }


    private void OnPlayersReadyChanged(NetworkListEvent<ulong> changeEvent)
    {
        Debug.Log($"Hazýr oyuncu sayýsý: {playersReady.Count}");
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
            Debug.Log($"Yarýþ baþlýyor: {remainingTime} saniye kaldý!");
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
                Debug.Log($"Oyuncu {playerId} yarýþa baþladý.");
            }
        }

        playersReady.Clear();
    }

    [ClientRpc]
    private void UpdatePlayerRaceStateClientRpc(ulong playerId, bool isInRace)
    {
        if (playerId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log($"Yarýþ durumu güncellendi: Oyuncu {playerId} yarýþta mý? {isInRace}");
            Race_Manager.Instance.isRaceActive = isInRace;
        }
    }

    //public void EndRace(ulong winnerId)
    //{
    //    isRaceActive = false;

    //    Debug.Log($"Yarýþ sona erdi! Birinci oyuncu: {winnerId}");

    //    // TPS kamerayý aktif et
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
        // Ýlgili iþlemler (örneðin, UI güncellemesi)
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
            Debug.LogWarning($"ClientId {clientId} bulunamadý.");
        }
    }
    private void ActivateTpsCamera(Transform winner)
    {
        if (tpsCameraPrefab != null)
        {
            // Eðer kamera zaten varsa, önceki kamerayý yok et
            if (tpsCameraInstance != null)
            {
                Destroy(tpsCameraInstance);
            }

            tpsCameraInstance = Instantiate(tpsCameraPrefab);
            tpsCameraInstance.transform.position = winner.position + new Vector3(0, 5, -10); // Kamerayý oyuncunun arkasýna yerleþtir
            tpsCameraInstance.transform.LookAt(winner); // Kamerayý oyuncuya doðru baktýr

            // Kamera her frame'de oyuncuyu takip etsin
            StartCoroutine(FollowPlayer(winner));
        }
    }

    private IEnumerator FollowPlayer(Transform winner)
    {
        float followDuration = 5f; // Kameranýn ne kadar süre boyunca takip edeceðini belirleyin
        float timeElapsed = 0f;

        while (timeElapsed < followDuration && tpsCameraInstance != null)
        {
            tpsCameraInstance.transform.position = Vector3.Lerp(tpsCameraInstance.transform.position, winner.position + new Vector3(0, 5, -10), Time.deltaTime * 5f);
            tpsCameraInstance.transform.LookAt(winner); // Kamera oyuncuyu takip etmeye devam edecek

            timeElapsed += Time.deltaTime; // Geçen zamaný artýr
            yield return null; // Bir sonraki frame'e geç
        }

        // Süre dolduðunda kamera takip etmeyi durdurabiliriz
        Destroy(tpsCameraInstance); // Kamerayý yok et (veya baþka bir þey yap)
    }

    public void ResetRace()
    {
        isRaceEnd = true;
        isRaceActive = false;
        activePlayers.Clear();
        disqualifiedPlayers.Clear();
        playersReady.Clear();
        Debug.Log("Yarýþ sýfýrlandý.");
        //isRaceActive = true;
    }

}
