using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class SingleLobbyTemplateUI : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI maxPlayersText;
    [SerializeField] private TextMeshProUGUI gameModeText;

    private Lobby lobby;
    private Button lobbyButton;

    private void Awake() {
        lobbyButton = GetComponent<Button>();
        lobbyButton.onClick.AddListener(OnLobbyButtonClicked);
    }

    private void OnLobbyButtonClicked() {
        LobbyManager.Instance.JoinLobby(lobby);
        LobbyManager.Instance.ShowPanel(GamePanels.JoinedLobbyUI);
    }

    public void UpdateLobbyTemplate(Lobby lobby) {

        this.lobby = lobby;

        lobbyNameText.text = lobby.Name;
        maxPlayersText.text = ($"{lobby.Players.Count}/{lobby.MaxPlayers}");
        gameModeText.text = lobby.Data[LobbyManager.GAME_MODE].Value;
    }
}
