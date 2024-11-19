using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Player_InputHandler : NetworkBehaviour
{
    private void Update()
    {
        // Yaln�zca yerel oyuncu giri�e izin verilir
        if (!IsOwner) return;

        // Tab tu�una bas�ld���nda paneli a�/kapat
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            UI_Manager.Instance.ToggleNicknamePanel();
        }
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            UI_Manager.Instance.ToggleNicknamePanel();
        }
    }
}
