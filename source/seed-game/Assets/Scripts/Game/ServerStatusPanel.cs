using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ServerStatusPanel : MonoBehaviour
{
    [SerializeField]
    private TMP_Text PlayerCountText;

    private void Start()
    {
        UpdatePlayerCountText(SeedGameStateBase.instance.PlayerCount, SeedGameStateBase.instance.StartingNumPlayers);
        SeedGameStateBase.instance.EventPlayerCountChanged += UpdatePlayerCountText;
    }

    private void UpdatePlayerCountText(int current, int max)
    {
        PlayerCountText.text = string.Format("{0}/{1}", current, max);
    }

    /// <summary>
    /// Should only be called by server.
    /// </summary>
    public void StartGame()
    {
        SeedGameModeBase.Instance.StartGame();
    }

}
