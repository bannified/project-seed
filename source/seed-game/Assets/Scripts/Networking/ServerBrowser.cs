using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Tember;

[RequireComponent(typeof(Tember.SeedListServer))]
public class ServerBrowser : MonoBehaviour
{
    [SerializeField]
    private Tember.SeedListServer ListServer;

    [SerializeField]
    private Transform ServerListContentBox;

    private List<ServerListEntry> Servers;

    [SerializeField]
    private List<ServerListEntryCell> ServerCellList;

    [SerializeField]
    private GameObject ServerCellPrefab;

    private void Awake()
    {
        Servers = new List<ServerListEntry>();
        ServerCellList = new List<ServerListEntryCell>();
        SetupListServerListeners();

    }

    void SetupListServerListeners()
    {
        ListServer.ServerFound += OnServerRetrieved;
        ListServer.AllServersLoaded += ForceRefreshBrowser;
    }

    public void RefreshServerList()
    {
        Servers.Clear();
        ListServer.RefreshLobbyList();
    }

    public void ForceRefreshBrowser()
    {
        Servers = new List<ServerListEntry>(ListServer.ServerEntries);
        Servers.Sort(ServerListEntry.GetServerPingComparer());

        ClearAllServerCells();
        foreach (ServerListEntry server in Servers)
        {
            AddServerCell(server);
        }
    }

    void OnServerRetrieved(Tember.ServerListEntry server)
    {
        Servers.Add(server);
    }

    void AddServerCell(Tember.ServerListEntry server)
    {
        GameObject go = Instantiate(ServerCellPrefab, ServerListContentBox);
        ServerListEntryCell cell = go.GetComponent<ServerListEntryCell>();
        cell.Setup(server);
        ServerCellList.Add(cell);
    }

    public void ClearAllServerCells()
    {
        for (int i = 0; i < ServerCellList.Count; i++)
        {
            Destroy(ServerCellList[i].gameObject);
        }

        ServerCellList.Clear();
    }
}
