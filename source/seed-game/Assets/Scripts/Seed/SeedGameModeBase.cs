using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

/// <summary>
/// SeedGameManager is a server-only entity that dictates the gameplay flow.
/// There can only ever be one instance of SeedGameMode in any game.
/// </summary>
public class SeedGameModeBase : NetworkBehaviour
{
    public static SeedGameModeBase Instance { get { return _instance; } }

    private static SeedGameModeBase _instance;

    static int RunningPlayerId = 1;

    [SerializeField]
    private SeedGameStateBase GameStatePrefab;

    public delegate void PlainDelegate();

    [SerializeField]
    public Dictionary<int, SeedPlayer> PlayerNumberToPlayer = new Dictionary<int, SeedPlayer>();

    public int StartingNumPlayers;

    [ReadOnly]
    [SerializeField]
    private SeedGameStateBase GameState;

    private void Awake()
    {
        if (_instance != null)
        {
            Debug.LogErrorFormat("There's already an instance of SeedGameModeBase on {0}!", _instance.name);
            Destroy(this.gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(this);

        GameState = Instantiate<SeedGameStateBase>(GameStatePrefab);
        GameState.Setup();
    }

    public void SetNumPlayers(int numPlayers)
    {
        GameState.StartingNumPlayers = numPlayers;
        RpcSetNumPlayers(numPlayers);
    }

    [ClientRpc]
    public void RpcSetNumPlayers(int numPlayers)
    {
        GameState.StartingNumPlayers = numPlayers;
    }

    public void RegisterPlayer(SeedPlayer player)
    {
        player.GameId = RunningPlayerId;
        ++RunningPlayerId;
        PlayerNumberToPlayer.Add(player.GameId, player);
        ++GameState.PlayerCount;
        RpcUpdatePlayerCount();
    }


    [ClientRpc]
    public void RpcUpdatePlayerCount()
    {
        ++GameState.PlayerCount;
    }

    public void UnregisterPlayer(SeedPlayer player)
    {
        if (PlayerNumberToPlayer.ContainsKey(player.GameId))
        {
            PlayerNumberToPlayer.Remove(player.GameId);
        }
        --GameState.PlayerCount;
    }

    public virtual void CheckServerStatus()
    {

    }

    public override void OnNetworkDestroy()
    {
        // Do cleanup
    }

    public virtual void StartGame()
    {
        Debug.Log("Game Started");
    }

    /// <summary>
    /// Called after server becomes active. 
    /// </summary>
    public override void OnStartServer()
    {
        NetworkServer.Spawn(GameState.gameObject);
    }

    public override void OnStartClient()
    {
        // this should only be called on the server
        if (!isServer)
        {
            throw new Exception("SeedGameModeBase is on not on the server");
        }
    }

    public override void OnStartLocalPlayer()
    {
        throw new NotImplementedException();
    }

    public override void OnStartAuthority()
    {

    }

    public override void OnStopAuthority()
    {

    }
}
