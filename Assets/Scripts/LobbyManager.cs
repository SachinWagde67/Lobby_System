using System;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour {

    public static LobbyManager Instance { get; private set; }

    public const string PLAYER_NAME = "PlayerName";
    public const string GAME_MODE = "GameMode";

    //public event Action<Lobby> OnLobbyCreated;
    public event Action<Lobby> OnLobbyJoined;
    public event Action<Lobby> OnLobbyJoinedUpdate;
    public event Action OnLobbyLeft;
    public event Action<Lobby> OnKickedFromLobby;
    public event Action<List<Lobby>> OnLobbiesListUpdated;

    [SerializeField] private Authenticate authenticatePanel;
    [SerializeField] private MainMenuUI mainMenuUIPanel;
    [SerializeField] private CreateLobbyUI createLobbyUIPanel;
    [SerializeField] private JoinUI joinUIPanel;
    [SerializeField] private JoinedLobbyUI joinedLobbyUIPanel;

    private Dictionary<GamePanels, GameObject> gamePanelsMap = new Dictionary<GamePanels, GameObject>();

    private string playerName;
    private Lobby joinedLobby;
    private float heartbeatTimer;
    private float lobbyUpdateTimer;

    public Lobby JoinedLobby => joinedLobby;

    private void Awake() {

        if(Instance != this && Instance != null) {
            Destroy(Instance.gameObject);
            return;
        }

        Instance = this;
        Application.runInBackground = true;
        CacheGamePanels();
    }

    private void CacheGamePanels() {

        gamePanelsMap.Add(GamePanels.PlayerNameAuthenticateUI, authenticatePanel.gameObject);
        gamePanelsMap.Add(GamePanels.MainMenuUI, mainMenuUIPanel.gameObject);
        gamePanelsMap.Add(GamePanels.CreateLobbyUI, createLobbyUIPanel.gameObject);
        gamePanelsMap.Add(GamePanels.JoinUI, joinUIPanel.gameObject);
        gamePanelsMap.Add(GamePanels.JoinedLobbyUI, joinedLobbyUIPanel.gameObject);
    }

    public void ShowPanel(GamePanels panelToShow) {

        if(gamePanelsMap != null && gamePanelsMap.Count > 0) {

            foreach(GamePanels panel in gamePanelsMap.Keys) {

                gamePanelsMap[panel].SetActive(false);

                if(panel == panelToShow) {
                    gamePanelsMap[panel].SetActive(true);
                }
            }
        }
    }

    private void Update() {

        LobbyHeartBeat();
        HandleLobbyUpdate();
    }

    private async void LobbyHeartBeat() {

        if(IsHost() && joinedLobby != null) {

            heartbeatTimer -= Time.deltaTime;

            if(heartbeatTimer < 0f) {

                heartbeatTimer = 15f;
                Debug.Log($"Heart beat pingged");
                await LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }
    }

    private async void HandleLobbyUpdate() {

        if(joinedLobby != null) {

            lobbyUpdateTimer -= Time.deltaTime;

            if(lobbyUpdateTimer < 0f) {

                lobbyUpdateTimer = 1.1f;

                joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);

                OnLobbyJoinedUpdate?.Invoke(joinedLobby);

                if(!IsPlayerInLobby()) {

                    Debug.Log($"Kicked from the lobby");

                    OnKickedFromLobby?.Invoke(joinedLobby);

                    joinedLobby = null;
                }
            }
        }
    }

    private bool IsPlayerInLobby() {

        if(joinedLobby != null && joinedLobby.Players != null) {

            foreach(Player player in joinedLobby.Players) {
                if(player.Id == AuthenticationService.Instance.PlayerId) {
                    return true;
                }
            }
        }

        return false;
    }

    public async void AuthenticatePlayer(string playerName) {

        this.playerName = playerName;
        InitializationOptions initializationOptions = new InitializationOptions();
        initializationOptions.SetProfile(playerName);

        await UnityServices.InitializeAsync(initializationOptions);

        AuthenticationService.Instance.SignedIn += () => {

            Debug.Log($"Player with name - {AuthenticationService.Instance.PlayerName} signed in!");

            RefreshLobby();
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private Player GetPlayer() {

        PlayerDataObject playerNameObject = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName);

        Dictionary<string, PlayerDataObject> keyValuePairs = new Dictionary<string, PlayerDataObject>();
        keyValuePairs.Add(PLAYER_NAME, playerNameObject);

        Player player = new Player(AuthenticationService.Instance.PlayerId, null, keyValuePairs);

        Debug.Log($"GetPlayer called - {player}");
        return player;
    }

    public async void CreateLobby(string lobbyName, int maxPlayers, GameType isPrivate, GameModes gameMode) {

        Player player = GetPlayer();

        CreateLobbyOptions lobbyOptions = new CreateLobbyOptions {
            Player = player,
            IsPrivate = isPrivate == GameType.Private ? true : false,
            Data = new Dictionary<string, DataObject> {
                { GAME_MODE, new DataObject(DataObject.VisibilityOptions.Public, gameMode.ToString()) }
            }
        };

        Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, lobbyOptions);

        joinedLobby = lobby;

        OnLobbyJoined?.Invoke(lobby);

        Debug.Log($"Lobby created - {lobby.Name}");
    }

    public async void JoinLobby(Lobby lobby) {

        Player player = GetPlayer();

        JoinLobbyByIdOptions joinLobbyByIdOptions = new JoinLobbyByIdOptions { Player = player };

        joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id, joinLobbyByIdOptions);

        OnLobbyJoined?.Invoke(lobby);

        Debug.Log($"Lobby joined - {joinedLobby.Name}, player - {player.Id}, name - {player.Data[LobbyManager.PLAYER_NAME].Value}");
    }

    public async void RefreshLobby() {

        try {

            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions();
            queryLobbiesOptions.Count = 10;

            queryLobbiesOptions.Filters = new List<QueryFilter> {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    value: "10",
                    op: QueryFilter.OpOptions.GT)
            };

            queryLobbiesOptions.Order = new List<QueryOrder> {
                new QueryOrder(
                    asc: false,
                    field: QueryOrder.FieldOptions.Created)
            };

            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();

            OnLobbiesListUpdated?.Invoke(queryResponse.Results);

        } catch(LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    public bool IsHost() {

        bool val = joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
        return val;
    }

    public async void KickPlayer(string PlayerID) {

        if(IsHost()) {

            try {

                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, PlayerID);

            } catch(LobbyServiceException e) {
                Debug.Log(e);
            }
        }
    }

    public async void LeaveLobby(bool shouldInvokeEvent = true) {

        if(joinedLobby != null) {

            try {

                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
                joinedLobby = null;

                if(shouldInvokeEvent) {
                    OnLobbyLeft?.Invoke();
                }

                Debug.Log($"Leave Lobby called");

            } catch(LobbyServiceException e) {
                Debug.Log(e);
            }
        }
    }
}
