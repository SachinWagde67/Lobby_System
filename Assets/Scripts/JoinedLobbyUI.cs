using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class JoinedLobbyUI : MonoBehaviour {

    [SerializeField] private Transform playerListSingleTemplate;
    [SerializeField] private Transform parent;
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI playersText;
    [SerializeField] private TextMeshProUGUI gameModeText;
    [SerializeField] private Button leaveLobbyButton;

    private Lobby lobby;

    private void Awake() {
        leaveLobbyButton.onClick.AddListener(LeaveLobby);
    }

    private void Start() {
        LobbyManager.Instance.OnLobbyJoined += UpdatePlayersInLobby;
        LobbyManager.Instance.OnLobbyJoinedUpdate += UpdatePlayersInLobby;
        LobbyManager.Instance.OnLobbyLeft += LobbyLeft;
        LobbyManager.Instance.OnKickedFromLobby += KickedFromLobby;
    }

    private void OnDestroy() {
        LobbyManager.Instance.OnLobbyJoined -= UpdatePlayersInLobby;
        LobbyManager.Instance.OnLobbyJoinedUpdate -= UpdatePlayersInLobby;
        LobbyManager.Instance.OnLobbyLeft -= LobbyLeft;
        LobbyManager.Instance.OnKickedFromLobby -= KickedFromLobby;
    }

    private void LobbyLeft() {
        LobbyManager.Instance.ShowPanel(GamePanels.JoinUI);
    }

    private void KickedFromLobby(Lobby lobby) {
        LobbyManager.Instance.ShowPanel(GamePanels.JoinUI);
    }

    private void LeaveLobby() {

        LobbyManager.Instance.LeaveLobby(shouldInvokeEvent: false);
        LobbyManager.Instance.ShowPanel(GamePanels.MainMenuUI);
    }

    private void UpdatePlayersInLobby(Lobby lobby) {


        this.lobby = lobby;
        Debug.Log($"Update Player list called");

        foreach(Transform child in parent) {
            Destroy(child?.gameObject);
        }

        Debug.Log($"Players count - {lobby.Players.Count}");

        foreach(Player player in lobby.Players) {

            Debug.Log($"player name - {player.Data[LobbyManager.PLAYER_NAME].Value}");

            Transform singlePlayerTemplate = Instantiate(playerListSingleTemplate, parent);
            singlePlayerTemplate.gameObject.SetActive(true);
            SinglePlayerTemplateUI singleLobbyTemplateUI = singlePlayerTemplate.GetComponent<SinglePlayerTemplateUI>();
            singleLobbyTemplateUI.UpdatePlayer(player);

            bool val = LobbyManager.Instance.IsHost() && player.Id != AuthenticationService.Instance.PlayerId;
            singleLobbyTemplateUI.EnableKickButton(val);
        }

        UpdateTexts(lobby);
    }

    private void UpdateTexts(Lobby lobby) {

        lobbyNameText.text = lobby.Name;
        playersText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
        gameModeText.text = lobby.Data[LobbyManager.GAME_MODE].Value;
    }
}
