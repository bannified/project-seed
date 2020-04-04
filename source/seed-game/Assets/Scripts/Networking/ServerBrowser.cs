using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

[RequireComponent(typeof(Tember.SeedListServer))]
public class ServerBrowser : MonoBehaviour
{
    [SerializeField]
    private Tember.SeedListServer ListServer;

    public void RefreshServerList()
    {
        ListServer.RefreshLobbyList();
    }

}
