using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// SeedGameStateBase is a server-client entity that dictates the gameplay flow.
/// Game state is synchronized across server and clients, and is meant to house data
/// that can be read by clients (but not changed!).
/// </summary>
public class SeedGameStateBase : NetworkBehaviour
{
    public static SeedGameStateBase instance { get { return _instance; } }
    private static SeedGameStateBase _instance;

    public delegate void OnPlayerCountDelegate(int current, int max);

    public SyncListInt PlayerList;

    [SyncEvent]
    public event OnPlayerCountDelegate EventPlayerCountChanged;

    [SyncVar(hook = nameof(OnPlayerCountChanged))]
    public int PlayerCount;

    [SyncVar(hook = nameof(OnNumPlayersChanged))]
    public int StartingNumPlayers;

    [ClientRpc]
    public virtual void RpcStartGame()
    {

    }

    public virtual void Setup()
    {
        if (_instance != null)
        {
            Debug.LogErrorFormat("There's already an instance of SeedGameStateBase on {0}!", _instance.name);
            Destroy(this.gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(this);
    }

    private void OnPlayerCountChanged(int oldCount, int newCount)
    {
        PlayerCount = newCount;
        if (isServer)
        {
            EventPlayerCountChanged?.Invoke(PlayerCount, StartingNumPlayers);
        }
    }

    private void OnNumPlayersChanged(int oldNum, int newNum)
    {
        StartingNumPlayers = newNum;
        if (isServer)
        {
            EventPlayerCountChanged?.Invoke(PlayerCount, StartingNumPlayers);
        }
    }

    public void RegisterPlayer()
    {
        ++PlayerCount;
    }

    public void UnregisterPlayer()
    {
        --PlayerCount;
    }

    public override void OnStartServer()
    {
        Setup();
    }

    public override void OnStartClient()
    {
    }
}
