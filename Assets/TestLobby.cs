using QFSW.QC;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class TestLobby : MonoBehaviour
{
    private Lobby _hostLobby;
    private float _heartbeatTimer;


    private void Update()
    {
        HandleHeartbeat();
    }

    private async void HandleHeartbeat()
    {
        if (_hostLobby == null) return;

        _heartbeatTimer -= Time.deltaTime;
        
        if (_heartbeatTimer <= 0f)
        {
            const float heartbeatTimerMax = 15;
            _heartbeatTimer = heartbeatTimerMax;

            await LobbyService.Instance.SendHeartbeatPingAsync(_hostLobby.Id);
        }
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log($"Signed in {AuthenticationService.Instance.PlayerId}");
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }


    [Command]
    private async void CreateLobby()
    {
        try
        {
            string lobbyName = "Lobby";
            int maxPlayers = 2;
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers);

            _hostLobby = lobby;
            Debug.Log($"Created lobby {lobby.Name}");
        }
        catch (LobbyServiceException error)
        {
            Debug.Log(error);
        }
    }

    [Command]
    private async void JoinLobby()
    {
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            if (queryResponse.Results.Count >= 0)
                await Lobbies.Instance.JoinLobbyByIdAsync(queryResponse.Results[0].Id);
        }
        catch (LobbyServiceException error)
        {
            Debug.Log(error);
        }
    }

    [Command]
    private async void ListLobby()
    {
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            Debug.Log($"Lobbies found: {queryResponse.Results.Count}");
            foreach (Lobby queryResponseResult in queryResponse.Results)
            {
                Debug.Log($"{queryResponseResult.Name} {queryResponseResult.MaxPlayers} ");
            }
        }
        catch (LobbyServiceException error)
        {
            Debug.Log(error);
        }
    }
}