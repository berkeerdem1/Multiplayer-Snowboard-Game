using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RacePath : MonoBehaviour
{
    public Transform[] pathPoints; // Yar�� yolundaki noktalar

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

        // �izgiyi olu�tur
        transform.DOPath(waypoints, 5f, PathType.CatmullRom)
            .SetOptions(false)
            .SetLookAt(0.01f); // Opsiyonel: Objelerin y�n� i�in

        // Oklar� yerle�tir ve s�radaki noktaya bakt�r
        for (int i = 0; i < pathPoints.Length; i++)
        {
            Vector3 currentPoint = pathPoints[i].position;
            Vector3 nextPoint;

            // Son ok i�in �zel durum
            if (i < pathPoints.Length - 1)
            {
                nextPoint = pathPoints[i + 1].position;
            }
            else
            {
                // D�ng�y� kapatmak istemiyorsan�z bu k�sm� kald�rabilirsiniz
                nextPoint = pathPoints[0].position; // �lk noktaya bakar
            }

            // Oku olu�tur
            GameObject arrow = Instantiate(arrowPrefab, currentPoint, Quaternion.identity);

            // Oku s�radaki waypoint'e �evir
            arrow.transform.LookAt(nextPoint);
        }
    }
}
