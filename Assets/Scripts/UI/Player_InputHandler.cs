using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Player_InputHandler : NetworkBehaviour
{
    private void Update()
    {
        // Yalnýzca yerel oyuncu giriþe izin verilir
        if (!IsOwner) return;

        // Tab tuþuna basýldýðýnda paneli aç/kapat
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
