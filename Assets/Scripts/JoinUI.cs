using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class JoinUI : MonoBehaviour {

    [SerializeField] private Transform lobbyListParent;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject lobbyListSingleTemplate;

    List<Lobby> lobbiesList = new List<Lobby>();

    private void Awake() {

        backButton.onClick.AddListener(OnBackButtonPressed);
        refreshButton.onClick.AddListener(RefreshLobbyList);
    }

    private void OnEnable() {
        UpdateLobbyList(lobbiesList);
    }

    private void Start() {
        LobbyManager.Instance.OnLobbiesListUpdated += UpdateLobbyList;
        LobbyManager.Instance.OnKickedFromLobby += OnKickFromLobby;
        LobbyManager.Instance.OnLobbyJoined += OnLobbyJoined;
        LobbyManager.Instance.OnLobbyLeft += OnLobbyLeft;
    }

    private void OnDestroy() {
        LobbyManager.Instance.OnLobbiesListUpdated -= UpdateLobbyList;
        LobbyManager.Instance.OnKickedFromLobby -= OnKickFromLobby;
        LobbyManager.Instance.OnLobbyJoined -= OnLobbyJoined;
        LobbyManager.Instance.OnLobbyLeft -= OnLobbyLeft;
    }

    private void OnKickFromLobby(Lobby lobby) {
        LobbyManager.Instance.ShowPanel(GamePanels.JoinUI);
    }

    private void OnLobbyLeft() {
        LobbyManager.Instance.ShowPanel(GamePanels.JoinUI);
    }

    private void OnLobbyJoined(Lobby lobby) {
        LobbyManager.Instance.ShowPanel(GamePanels.JoinedLobbyUI);
    }

    private void RefreshLobbyList() {

        LobbyManager.Instance.RefreshLobby();
    }

    private void OnBackButtonPressed() {
        LobbyManager.Instance.ShowPanel(GamePanels.MainMenuUI);
    }

    private void UpdateLobbyList(List<Lobby> lobbiesList) {

        Debug.Log("UpdateLobbyList called");

        this.lobbiesList = lobbiesList;

        foreach(Transform child in lobbyListParent) {
            Destroy(child?.gameObject);
        }

        Debug.Log($"lobbyLists count - {lobbiesList.Count}");

        foreach(Lobby lobby in lobbiesList) {

            Debug.Log($"lobby - {lobby.Name}");
            GameObject singleLobbyTemplate = Instantiate(lobbyListSingleTemplate, lobbyListParent);
            singleLobbyTemplate.SetActive(true);
            SingleLobbyTemplateUI singleLobbyTemplateUI = singleLobbyTemplate.GetComponent<SingleLobbyTemplateUI>();
            singleLobbyTemplateUI.UpdateLobbyTemplate(lobby);
        }
    }
}
