using Mirror;
using Mirror.FizzySteam;
using Steamworks;
using Steamworks.Data;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SteamworksManager : MonoBehaviour
{
    public static SteamworksManager Instance { get; private set; }

    private FizzyFacepunch transport;

    private Lobby[] availableLobbies;

    public Lobby? currentLobby { get; private set; } = null;

    public ulong hostId;

    private const string BeaverKey = "BeaverGame";
    private const string BeaverValue = "3876";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        transport = GetComponent<FizzyFacepunch>();

        if(transport == null)
        {
            Debug.LogWarning("FizzyFacepunch not enabled! Replace transport to enable Steamworks!");
        }

        SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreatedCallback;
        SteamMatchmaking.OnLobbyCreated += OnLobbyCreatedCallback;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEnteredCallback;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoinedCallback;
        SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberDisconnectedCallback;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeaveCallback;
        SteamMatchmaking.OnLobbyInvite += OnLobbyInvite;
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
    }

    private void OnDestroy()
    {
        SteamMatchmaking.OnLobbyGameCreated -= OnLobbyGameCreatedCallback;
        SteamMatchmaking.OnLobbyCreated -= OnLobbyCreatedCallback;
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEnteredCallback;
        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoinedCallback;
        SteamMatchmaking.OnLobbyMemberDisconnected -= OnLobbyMemberDisconnectedCallback;
        SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeaveCallback;
        SteamMatchmaking.OnLobbyInvite -= OnLobbyInvite;
        SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }

    public void Disconnect()
    {
        currentLobby?.Leave();

        if (NetworkManager.singleton == null)
            return;

        NetworkManager.singleton.StopClient();
        NetworkManager.singleton.StopServer();
    }

    // friend sent invite
    private void OnLobbyInvite(Friend _friend, Lobby _lobby)
    {
        Debug.Log($"Invite from {_friend.Name}");
    }

    // When invite is accepted, or join a friend's game
    private async void OnGameLobbyJoinRequested(Lobby _lobby, SteamId _steamId)
    {
        RoomEnter joinedLobby = await _lobby.Join();
        if (joinedLobby != RoomEnter.Success)
            Debug.LogWarning("Failed to create lobby");
        else
        {
            currentLobby = _lobby;
            Debug.Log("Joined Lobby");
        }
    }

    private void OnLobbyMemberLeaveCallback(Lobby _lobby, Friend _friend)
    {
        Debug.Log($"{_friend.Name} left the lobby");
    }

    private void OnLobbyMemberDisconnectedCallback(Lobby _lobby, Friend _friend)
    {
        Debug.Log($"{_friend.Name} was disconnected from the lobby");
    }

    private void OnLobbyMemberJoinedCallback(Lobby _lobby, Friend _friend)
    {
        Debug.Log($"{_friend.Name} joined the lobby");
    }

    private void OnLobbyEnteredCallback(Lobby _lobby)
    {
        Debug.Log("Lobby Entered");

        Debug.Log("Attempting to start steam client...");
        StartClient(currentLobby.Value.Owner.Id);
    }

    private void OnLobbyCreatedCallback(Result _result, Lobby _lobby)
    {
        if (_result != Result.OK)
        {
            Debug.LogWarning("Lobby was not created");
            return;
        }

        _lobby.SetPublic();
        _lobby.SetJoinable(true);
        _lobby.SetData(BeaverKey, BeaverValue);
        _lobby.SetGameServer(_lobby.Owner.Id);
    }

    private void OnLobbyGameCreatedCallback(Lobby _lobby, uint _ip, ushort _port, SteamId _steamId)
    {
        Debug.Log($"Lobby was created -> IP: {_ip} | Port: {_port} | SteamID: {_steamId}");
    }

    public async void StartHost()
    {
        Debug.Log("Started Steam Server!");

        currentLobby = await SteamMatchmaking.CreateLobbyAsync(2);
    }

    public void StartClient(SteamId _steamId)
    {
        Debug.Log("Started Steam Client!");
        if ((NetworkManager.singleton as NetworkManagerBeaver).isServer)
        {
            Debug.Log("Host cannot create client!");
            return;
        }


        //transport.SteamUserID = _steamId;

        (NetworkManager.singleton as NetworkManagerBeaver).networkAddress = _steamId.ToString();

        (NetworkManager.singleton as NetworkManagerBeaver).StartClient();
    }

    public async void JoinAvailableLobby(Button joinButton)
    {
        joinButton.interactable = false;

        bool lobbyFound = await GenerateLobbyList();

        joinButton.interactable = !lobbyFound;
    }

    private async Task<bool> GenerateLobbyList()
    {
        LobbyQuery query = SteamMatchmaking.LobbyList.WithSlotsAvailable(1).WithKeyValue(BeaverKey, BeaverValue).FilterDistanceFar();
        availableLobbies = await query.RequestAsync();

        if (availableLobbies.Length == 0)
        {
            Debug.Log("No Available Lobbies... Expanding Search...");
            query = SteamMatchmaking.LobbyList.WithSlotsAvailable(1).WithKeyValue(BeaverKey, BeaverValue);
            availableLobbies = await query.RequestAsync();
        }

        if (availableLobbies.Length == 0)
        {
            Debug.Log("No Available Lobbies Worldwide!");
            return false;
        }

        Debug.Log("Available Lobbies: " + availableLobbies.Length);
        JoinLobby(availableLobbies[0]);
        return true;
    }

    private async void JoinLobby(Lobby _lobby)
    {
        RoomEnter joinedLobby = await _lobby.Join();
        if (joinedLobby != RoomEnter.Success)
            Debug.LogWarning("Failed to join lobby...");
        else
        {
            currentLobby = _lobby;
            Debug.Log("Joined Lobby");
        }
    }
}
