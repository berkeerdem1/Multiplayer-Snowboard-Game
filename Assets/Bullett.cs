using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Bullett : NetworkBehaviour
{

    public int damage = 20; // Merminin verdi�i hasar

    private void OnTriggerEnter(Collider collision)
    {
        if (IsServer) // �arp��may� yaln�zca sunucu kontrol eder
        {
            // �arpt��� objenin PlayerManager scripti var m� kontrol et
            var player = collision.gameObject.GetComponent<SnowboardController>();
            if (player != null)
            {
                print("Mermi:  Degdim");
                Vector3 dir = new Vector3(collision.transform.position.x, collision.transform.position.y, collision.transform.position.z);
                player.BulletDamageServerRpc(dir); // Oyuncuya hasar ver
            }

            // Mermiyi yok et
            Destroy(gameObject);
        }

        if (collision.gameObject.CompareTag("Shield"))
        {
            Destroy(gameObject);
        }
    }


}
