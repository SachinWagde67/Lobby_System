using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateLobbyUI : MonoBehaviour {

    [SerializeField] private TMP_InputField lobbyNameInputField;
    [SerializeField] private Button privatePublicButton;
    [SerializeField] private TMP_InputField maxPlayersInputField;
    [SerializeField] private Button gameModeButton;
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button backButton;

    string lobbyName = "My_Lobby";
    int maxPlayers = 2;
    bool isPrivate = true;
    GameType gameType;
    GameModes gameMode;
    TextMeshProUGUI privatePublicButtonText;
    TextMeshProUGUI gameModeText;
    bool hasLobbyNameText;
    bool hasMaxPlayersText;

    private void Awake() {

        createLobbyButton.interactable = false;
        privatePublicButtonText = privatePublicButton.GetComponentInChildren<TextMeshProUGUI>();
        gameModeText = gameModeButton.GetComponentInChildren<TextMeshProUGUI>();
        privatePublicButton.onClick.AddListener(PrivatePublicButton);
        gameModeButton.onClick.AddListener(ChangeGameMode);
        createLobbyButton.onClick.AddListener(CreateLobby);
        backButton.onClick.AddListener(OnBackButtonPressed);

        lobbyNameInputField.onValueChanged.AddListener(UpdateHasLobbyNameText);
        maxPlayersInputField.onValueChanged.AddListener(UpdateHasMaxPlayersText);
    }

    private void PrivatePublicButton() {
        isPrivate = !isPrivate;
        UpdateText();
    }

    private void ChangeGameMode() {

        switch(gameMode) {

            case GameModes.CaptureTheFlag:
                gameMode = GameModes.DeathMatch;
                break;
            case GameModes.DeathMatch:
                gameMode = GameModes.Ranked;
                break;
            case GameModes.Ranked:
                gameMode = GameModes.Unranked;
                break;
            case GameModes.Unranked:
                gameMode = GameModes.CaptureTheFlag;
                break;
        }

        UpdateText();
    }

    private void CreateLobby() {

        LobbyManager.Instance.CreateLobby(lobbyName, maxPlayers, gameType, gameMode);
        LobbyManager.Instance.ShowPanel(GamePanels.JoinedLobbyUI);
    }

    private void UpdateText() {

        privatePublicButtonText.text = isPrivate ? "Private" : "Public";
        gameType = isPrivate ? GameType.Private : GameType.Public;
        gameModeText.text = gameMode.ToString();
        lobbyName = lobbyNameInputField.text;
        maxPlayers = GetMaxPlayers();

        if(hasLobbyNameText && hasMaxPlayersText) {
            createLobbyButton.interactable = true;
        }
    }

    private int GetMaxPlayers() {

        int value = 2;

        if(int.TryParse(maxPlayersInputField.text, out value)) {
            return value;
        }

        return value;
    }

    private void UpdateHasLobbyNameText(string value) {
        hasLobbyNameText = !string.IsNullOrWhiteSpace(value);
        UpdateText();
    }

    private void UpdateHasMaxPlayersText(string value) {
        hasMaxPlayersText = !string.IsNullOrWhiteSpace(value);
        UpdateText();
    }

    private void OnBackButtonPressed() {
        LobbyManager.Instance.ShowPanel(GamePanels.MainMenuUI);
    }
}
