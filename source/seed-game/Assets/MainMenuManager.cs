﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField]
    GameObject MainMenuGroup;
    [SerializeField]
    LobbyGroupController LobbyGroup;

    public void OnCreateLobbyClicked()
    {
        MainMenuGroup.SetActive(false);
        LobbyGroup.gameObject.SetActive(true);
        NetworkManager.singleton.StartHost();
    }

    public void OnJoinLobbyClicked()
    {
        MainMenuGroup.SetActive(false);
        LobbyGroup.gameObject.SetActive(true);
        NetworkManager.singleton.StartClient();
    }

    public void RefreshPlayerList()
    {
        LobbyGroup.ClearAllPlayerLobbyCells();
        Debug.Log("Refresh Player List.");
        LobbyGroup.AddPlayers((NetworkManager.singleton as SeedNetworkRoomManager).roomPlayers);
    }

    public void LeaveLobby()
    {
        MainMenuGroup.SetActive(true);
        LobbyGroup.gameObject.SetActive(false);
        LobbyGroup.ClearAllPlayerLobbyCells();
        NetworkManager.singleton.StopClient();
        NetworkManager.singleton.StopHost();
    }
}
