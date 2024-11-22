using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Player_Disqualify : NetworkBehaviour
{
    public Transform[] pathPoints; // Path'in noktalarý
    public float maxDistance = 15f; // Path'ten maksimum uzaklýk
    public float returnTime = 10f; // Path'e dönmek için süre
    private Vector3 startPosition; // Baþlangýç noktasý

    private bool isOutOfBounds = false; // Path dýþýna çýktý mý?
    private float outOfBoundsTimer = 0f; // Path'e dönmek için geri sayým
    private RacePath racePath;
    public bool isInRace = false; // Oyuncu yarýþta mý?

    public Text returnTimeText;

    private void Awake()
    {
        racePath = FindFirstObjectByType<RacePath>();
    }

    private void Start()
    {
        pathPoints = racePath.pathPoints;
        startPosition = transform.position;
        returnTimeText.enabled = false;
    }

    private void FixedUpdate()
    {
        if (!isInRace || !Race_Manager.Instance.isRaceActive || !IsClient)
            return; // Yarýþta deðilse hiçbir þey yapma

        if (Race_Manager.Instance.isRaceEnd)
        {
            isInRace = false;
            returnTimeText.enabled = false;
        }
           

        // Oyuncunun path'e olan mesafesini kontrol et
        Vector3 closestPoint = GetClosestPathPoint(transform.position);
        float distanceToPath = Vector3.Distance(transform.position, closestPoint);

        if (distanceToPath > maxDistance)
        {
            if (!isOutOfBounds)
            {
                // Path dýþýna çýktý, geri sayýmý baþlat
                isOutOfBounds = true;
                outOfBoundsTimer = returnTime;
                Debug.Log("Path dýþýna çýktýnýz! Geri dönmek için 10 saniyeniz var.");
                
            }
        }
        else
        {
            if (isOutOfBounds)
            {
                // Path'e geri döndü, geri sayýmý sýfýrla
                isOutOfBounds = false;
                Debug.Log("Path'e geri döndünüz!");
            }
        }

        // Geri sayým iþlemi
        if (isOutOfBounds)
        {
            outOfBoundsTimer -= Time.deltaTime;
            returnTimeText.enabled = true;
            returnTimeText.text = outOfBoundsTimer.ToString();

            if (outOfBoundsTimer <= 0)
            {
                // Path'e geri dönmedi, diskalifiye
                DisqualifyPlayer();
            }
        }
    }

    // Path'teki en yakýn noktayý bul
    private Vector3 GetClosestPathPoint(Vector3 position)
    {
        Vector3 closestPoint = pathPoints[0].position;
        float minDistance = Vector3.Distance(position, closestPoint);

        foreach (Transform point in pathPoints)
        {
            float distance = Vector3.Distance(position, point.position);
            if (distance < minDistance)
            {
                closestPoint = point.position;
                minDistance = distance;
            }
        }

        return closestPoint;
    }

    // Oyuncuyu diskalifiye et ve baþlangýç noktasýna ýþýnla
    private void DisqualifyPlayer()
    {
        returnTimeText.text = "Disqualify";
        Debug.Log("Diskalifiye oldunuz! Baþlangýç noktasýna ýþýnlanýyorsunuz.");

        transform.position = startPosition;

        // Yarýþtan çýkar ve diskalifiye et
        Race_Manager.Instance.RemovePlayerFromRace(NetworkManager.Singleton.LocalClientId);

        isOutOfBounds = false;
        isInRace = false; // Yarýþtan çýkar
    }
}
