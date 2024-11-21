using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RacePath : MonoBehaviour
{
    public Transform[] pathPoints; // Yarýþ yolundaki noktalar

    public LineRenderer lineRenderer;

    public GameObject arrowPrefab;

    //
    private void Start()
    {
        lineRenderer.positionCount = pathPoints.Length;
        for (int i = 0; i < pathPoints.Length; i++)
        {
            lineRenderer.SetPosition(i, pathPoints[i].position);
        }

        Vector3[] waypoints = new Vector3[pathPoints.Length];
        for (int i = 0; i < pathPoints.Length; i++)
        {
            waypoints[i] = pathPoints[i].position;
        }

        // Çizgiyi oluþtur
        transform.DOPath(waypoints, 5f, PathType.CatmullRom)
            .SetOptions(false)
            .SetLookAt(0.01f); // Opsiyonel: Objelerin yönü için

        // Oklarý yerleþtir ve sýradaki noktaya baktýr
        for (int i = 0; i < pathPoints.Length; i++)
        {
            Vector3 currentPoint = pathPoints[i].position;
            Vector3 nextPoint;

            // Son ok için özel durum
            if (i < pathPoints.Length - 1)
            {
                nextPoint = pathPoints[i + 1].position;
            }
            else
            {
                // Döngüyü kapatmak istemiyorsanýz bu kýsmý kaldýrabilirsiniz
                nextPoint = pathPoints[0].position; // Ýlk noktaya bakar
            }

            // Oku oluþtur
            GameObject arrow = Instantiate(arrowPrefab, currentPoint, Quaternion.identity);

            // Oku sýradaki waypoint'e çevir
            arrow.transform.LookAt(nextPoint);
        }
    }
}
