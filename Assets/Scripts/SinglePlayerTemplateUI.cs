using System.Runtime.CompilerServices;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class SinglePlayerTemplateUI : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Button kickButton;

    private Player player;

    private void Awake() {
        kickButton.onClick.AddListener(KickPlayer);
    }

    private void KickPlayer() {

        LobbyManager.Instance.KickPlayer(player.Id);
    }

    public void UpdatePlayer(Player player) {

        this.player = player;

        playerNameText.text = player.Data[LobbyManager.PLAYER_NAME].Value;
    }

    public void EnableKickButton(bool val) {

        kickButton.gameObject.SetActive(val);
    }
}
