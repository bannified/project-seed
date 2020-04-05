using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField]
    GameObject MainMenuGroup;
    [SerializeField]
    LobbyGroupController LobbyGroup;
    [SerializeField]
    ServerBrowser ListServerGroup;

    public void OnCreateLobbyClicked()
    {
        MainMenuGroup.SetActive(false);
        LobbyGroup.gameObject.SetActive(true);
        if (NetworkManager.singleton is SeedNetworkRoomManager room)
        {
            room.SwitchToDirectIPTransport();
        }
        NetworkManager.singleton.StartHost();
    }

    public void OnJoinLobbyClicked()
    {
        MainMenuGroup.SetActive(false);
        LobbyGroup.gameObject.SetActive(true);
        if (NetworkManager.singleton is SeedNetworkRoomManager room)
        {
            room.SwitchToDirectIPTransport();
        }

        NetworkManager.singleton.StartClient();
    }

    public void OnSteamCreateLobbyClicked()
    {
        MainMenuGroup.SetActive(false);
        LobbyGroup.gameObject.SetActive(true);
        if (NetworkManager.singleton is SeedNetworkRoomManager room)
        {
            room.playerName = SeedSteamManager.SeedInstance.UserSteamName;
            room.SwitchToSteamTransport();
        }
        NetworkManager.singleton.StartHost();
    }

    public void OnSteamJoinLobbyClicked()
    {
        MainMenuGroup.SetActive(false);
        LobbyGroup.gameObject.SetActive(true);
        if (NetworkManager.singleton is SeedNetworkRoomManager room)
        {
            room.playerName = SeedSteamManager.SeedInstance.UserSteamName;
            room.SwitchToSteamTransport();
        }

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

    public void OnListServerButtonClicked()
    {
        MainMenuGroup.SetActive(false);
        ListServerGroup.gameObject.SetActive(true);
    }
}
