using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour {

    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button joinLobbyButton;

    private void Awake() {

        createLobbyButton.onClick.AddListener(CreateLobby);
        joinLobbyButton.onClick.AddListener(JoinLobby);
    }

    private void CreateLobby() {

        LobbyManager.Instance.ShowPanel(GamePanels.CreateLobbyUI);
    }

    private void JoinLobby() {

        LobbyManager.Instance.ShowPanel(GamePanels.JoinUI);
    }
}
