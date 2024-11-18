using Unity.Netcode;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using Unity.Services.Core; // Unity Services'ý baþlatmak için
using Unity.Services.Authentication;
using System; // Oyun oturumlarýný kimlik doðrulamasý ile baþlatmak için

public class RelayManager : MonoBehaviour
{
    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in:" + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        //CreateRelay();
    }

    public async void CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log("joinCode:" + joinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            NetworkManager.Singleton.StartHost();
        }
        catch(RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void JoinRelay(string joinCode)
    {
        try
        {
            Debug.Log("Joining Relay With" + joinCode);

            JoinAllocation joinAllLocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            //RelayServerData relayServerData = new RelayServerData(joinAllLocation, "dtls");
            //NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllLocation.RelayServer.IpV4,
                (ushort)joinAllLocation.RelayServer.Port,
                joinAllLocation.AllocationIdBytes,
                joinAllLocation.Key,
                joinAllLocation.ConnectionData,
                joinAllLocation.HostConnectionData
            );

            NetworkManager.Singleton.StartClient();

            Debug.Log("Successfully joined the game!");
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Failed to join the game. Reason: " + e.Message);
        }
    }
}

