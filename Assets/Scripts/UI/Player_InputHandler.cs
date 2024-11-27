using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Player_InputHandler : NetworkBehaviour
{
    private void Update()
    {
        if (!IsOwner) return;

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
