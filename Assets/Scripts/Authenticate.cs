using TMPro;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

public class Authenticate : MonoBehaviour {

    [SerializeField] private Button authenticateButton;
    [SerializeField] private TMP_InputField playerNameInputField;

    string playerName;

    private void Awake() {

        authenticateButton.enabled = false;
        authenticateButton.onClick.AddListener(AuthenticatePlayer);
        playerNameInputField.onValueChanged.AddListener(UpdateButton);
    }

    private void AuthenticatePlayer() {

        playerName = playerNameInputField.text;

        LobbyManager.Instance.AuthenticatePlayer(playerName);
        LobbyManager.Instance.ShowPanel(GamePanels.MainMenuUI);
    }

    private void UpdateButton(string val) {

        bool hasText = !string.IsNullOrEmpty(val);
        authenticateButton.enabled = hasText;
    }

}
