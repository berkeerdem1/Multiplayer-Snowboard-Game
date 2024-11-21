using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Race_Manager : NetworkBehaviour
{
    public static Race_Manager Instance { get; private set; }

    public List<ulong> activePlayers = new List<ulong>(); // Yarýþa katýlan oyuncular
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

    public void AddPlayerToRace(ulong playerId)
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
        Debug.Log($"PlayerReadyServerRpc çaðrýldý! Oyuncu ID: {playerId}");

        if (!playersReady.Contains(playerId))
        {
            playersReady.Add(playerId);
            Debug.Log($"Oyuncu {playerId} playersReady listesine eklendi. Mevcut liste: {string.Join(", ", playersReady)}");

            // Oyuncuyu baþlangýç pozisyonuna ýþýnla
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(playerId, out var client))
            {
                var playerObject = client.PlayerObject;
                if (playerObject != null)
                {
                    playerObject.GetComponent<SnowboardController>().DisableControls();
                    var transform = playerObject.transform;
                    transform.position = raceStartPosition.position; // Baþlangýç pozisyonu
                    transform.rotation = Quaternion.identity; // Varsayýlan rotasyon
                    Debug.Log($"Oyuncu {playerId} baþlangýç pozisyonuna ýþýnlandý.");
                }
                else
                {
                    Debug.LogWarning($"PlayerObject bulunamadý! Oyuncu ID: {playerId}");
                }
            }
            else
            {
                Debug.LogWarning($"ConnectedClients'ta oyuncu bulunamadý! Oyuncu ID: {playerId}");
            }
        }
    }




    private void OnPlayersReadyChanged(NetworkListEvent<ulong> changeEvent)
    {
        Debug.Log("OnPlayersReadyChanged tetiklendi.");
        Debug.Log($"Hazýr oyuncular: {string.Join(", ", playersReady)}");
        Debug.Log($"Hazýr oyuncu sayýsý: {playersReady.Count}");

        if (!isRaceActive && playersReady.Count > 0 && playersReady.Count == NetworkManager.Singleton.ConnectedClients.Count)
        {
            Debug.Log("Tüm oyuncular hazýr. Yarýþ baþlýyor!");
            StartCountdown();
        }
        else if (isRaceActive)
        {
            Debug.Log("Yarýþ zaten aktif. Yeni oyuncular eklenmeyecek.");
        }
        else
        {
            Debug.LogWarning("Tüm oyuncular henüz hazýr deðil.");
        }
    }



    private void StartCountdown()
    {
        isRaceActive = true;
        Debug.Log("StartCountdown çaðrýldý. Geri sayým baþlatýlýyor.");
        StartCoroutine(StartRaceCountdown());
    }


    private IEnumerator StartRaceCountdown()
    {
        float remainingTime = countdownTime;
        Debug.Log($"StartRaceCountdown baþladý. Geri sayým süresi: {countdownTime} saniye.");

        while (remainingTime > 0)
        {
            Debug.Log($"Yarýþ baþlýyor: {remainingTime} saniye kaldý!");
            yield return new WaitForSeconds(1f);
            remainingTime--;
        }

        Debug.Log("Geri sayým bitti. Yarýþ baþlatýlýyor!");
        StartRaceServerRpc();
    }


    [ServerRpc(RequireOwnership = false)]
    public void StartRaceServerRpc()
    {
        isRaceEnd = false;
        Debug.Log("StartRaceServerRpc çaðrýldý. Yarýþ baþlatýldý!");

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
                    Debug.Log($"Oyuncu {playerId} yarýþa katýldý ve kontrolleri açýldý.");
                }
                else
                {
                    Debug.LogError($"Oyuncu {playerId} için SnowboardController bulunamadý!");
                }
            }
            else
            {
                Debug.LogError($"PlayerID {playerId} için NetworkManager'da bir client bulunamadý.");
            }
        }

        Debug.Log("Yarýþa dahil olan tüm oyuncularýn kontrolleri açýldý.");
        playersReady.Clear();
    }

    public void EndRace(ulong winnerId)
    {
        Debug.Log($"Yarýþ sona erdi! Birinci oyuncu: {winnerId}");

        // TPS kamerayý aktif et
        var winner = NetworkManager.Singleton.ConnectedClients[winnerId].PlayerObject.transform;
        ActivateTpsCamera(winner);

        StartCoroutine(EndRaceCoroutine());
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
            tpsCameraInstance.transform.position = winner.position + new Vector3(0, 5, -10); // Kamerayý oyuncunun arkasýna konumlandýr
            tpsCameraInstance.transform.LookAt(winner); // Kamerayý oyuncuya baktýr

            // Kamera her frame'de oyuncuyu takip etsin
            StartCoroutine(FollowPlayer(winner));
        }
    }

    private IEnumerator FollowPlayer(Transform winner)
    {
        while (tpsCameraInstance != null)
        {
            // Kamerayý her frame'de oyuncunun konumuna göre ayarlýyoruz
            tpsCameraInstance.transform.position = Vector3.Lerp(tpsCameraInstance.transform.position, winner.position + new Vector3(0, 5, -10), Time.deltaTime * 5f); // Kamera oyuncunun arkasýnda
            tpsCameraInstance.transform.LookAt(winner); // Kamera her zaman oyuncuya bakacak

            yield return null; // Bir sonraki frame'e geçiþ yap
        }
    }

    private IEnumerator EndRaceCoroutine()
    {
        yield return new WaitForSeconds(5f); // 5 saniye TPS kamera göster
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
        Debug.Log("Yarýþ sýfýrlandý.");
        isRaceActive = true;
    }

}
