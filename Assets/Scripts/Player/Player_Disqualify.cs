using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Player_Disqualify : NetworkBehaviour
{
    public Transform[] pathPoints; // Path'in noktalar�
    public float maxDistance = 15f; // Path'ten maksimum uzakl�k
    public float returnTime = 10f; // Path'e d�nmek i�in s�re
    private Vector3 startPosition; // Ba�lang�� noktas�

    private bool isOutOfBounds = false; // Path d���na ��kt� m�?
    private float outOfBoundsTimer = 0f; // Path'e d�nmek i�in geri say�m
    private RacePath racePath;
    public bool isInRace = false; // Oyuncu yar��ta m�?

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
            return; // Yar��ta de�ilse hi�bir �ey yapma

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
                // Path d���na ��kt�, geri say�m� ba�lat
                isOutOfBounds = true;
                outOfBoundsTimer = returnTime;
                Debug.Log("Path d���na ��kt�n�z! Geri d�nmek i�in 10 saniyeniz var.");
                
            }
        }
        else
        {
            if (isOutOfBounds)
            {
                // Path'e geri d�nd�, geri say�m� s�f�rla
                isOutOfBounds = false;
                Debug.Log("Path'e geri d�nd�n�z!");
            }
        }

        // Geri say�m i�lemi
        if (isOutOfBounds)
        {
            outOfBoundsTimer -= Time.deltaTime;
            returnTimeText.enabled = true;
            returnTimeText.text = outOfBoundsTimer.ToString();

            if (outOfBoundsTimer <= 0)
            {
                // Path'e geri d�nmedi, diskalifiye
                DisqualifyPlayer();
            }
        }
    }

    // Path'teki en yak�n noktay� bul
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

    // Oyuncuyu diskalifiye et ve ba�lang�� noktas�na ���nla
    private void DisqualifyPlayer()
    {
        returnTimeText.text = "Disqualify";
        Debug.Log("Diskalifiye oldunuz! Ba�lang�� noktas�na ���nlan�yorsunuz.");

        transform.position = startPosition;

        // Yar��tan ��kar ve diskalifiye et
        Race_Manager.Instance.RemovePlayerFromRace(NetworkManager.Singleton.LocalClientId);

        isOutOfBounds = false;
        isInRace = false; // Yar��tan ��kar
    }
}
