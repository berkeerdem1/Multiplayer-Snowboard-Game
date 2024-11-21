using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blinking_Arrow : MonoBehaviour
{
    public float blinkDuration = 1f; // Tam bir yan�p s�nme d�ng�s� i�in s�re
    private SpriteRenderer[] spriteRenderers;

    private void Start()
    {
        // T�m SpriteRenderer bile�enlerini al
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        if (spriteRenderers.Length > 0)
        {
            StartCoroutine(BlinkAlpha());
        }
        else
        {
            Debug.LogWarning("Bu obje i�inde SpriteRenderer bulunamad�!");
        }
    }

    private IEnumerator BlinkAlpha()
    {
        float elapsedTime = 0f;

        while (true)
        {
            // Alpha azaltma (fade out)
            while (elapsedTime < blinkDuration / 2)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / (blinkDuration / 2));

                // Her bir sprite i�in alpha de�eri ayarla
                foreach (var spriteRenderer in spriteRenderers)
                {
                    Color originalColor = spriteRenderer.color;
                    spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                }

                yield return null;
            }

            elapsedTime = 0f;

            // Alpha art�rma (fade in)
            while (elapsedTime < blinkDuration / 2)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsedTime / (blinkDuration / 2));

                // Her bir sprite i�in alpha de�eri ayarla
                foreach (var spriteRenderer in spriteRenderers)
                {
                    Color originalColor = spriteRenderer.color;
                    spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                }

                yield return null;
            }

            elapsedTime = 0f;
        }
    }
}
