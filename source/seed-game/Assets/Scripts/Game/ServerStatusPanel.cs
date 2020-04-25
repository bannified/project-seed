using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Mirror;

public class ServerStatusPanel : MonoBehaviour, IGameStateBootstrap
{
    [SerializeField]
    private TMP_Text PlayerCountText;

    [SerializeField]
    private Button StartGameButton;

    private void UpdatePlayerCountText(int current, int max)
    {
        PlayerCountText.text = string.Format("{0}/{1}", current, max);

        if (NetworkManager.singleton.mode == NetworkManagerMode.Host)
        {
            StartGameButton.gameObject.SetActive(current >= max);
        }
    }

    /// <summary>
    /// Should only be called by server.
    /// </summary>
    public void StartGame()
    {
        SeedGameModeBase.Instance.StartGame();
    }

    public void SetupWithGameState(SeedGameStateBase gameState)
    {
        UpdatePlayerCountText(SeedGameStateBase.instance.PlayerCount, SeedGameStateBase.instance.StartingNumPlayers);
        SeedGameStateBase.instance.EventPlayerCountChanged += UpdatePlayerCountText;
    }
}
