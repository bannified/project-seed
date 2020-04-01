using UnityEngine;
using Mirror;

/*
	Documentation: https://mirror-networking.com/docs/Components/NetworkRoomPlayer.html
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkRoomPlayer.html
*/

/// <summary>
/// This component works in conjunction with the NetworkRoomManager to make up the multiplayer room system.
/// The RoomPrefab object of the NetworkRoomManager must have this component on it.
/// This component holds basic room player data required for the room to function.
/// Game specific data for room players can be put in other components on the RoomPrefab or in scripts derived from NetworkRoomPlayer.
/// </summary>
public class SeedNetworkRoomPlayer : NetworkBehaviour
{
    [SyncVar]
    public string PlayerName;

    public MainMenuManager MainMenu;

    private void Awake()
    {
        if (NetworkManager.singleton is SeedNetworkRoomManager room)
        {
            // automatically replicated across clients.
            room.roomPlayers.Add(this);
        }
    }

    private void Start()
    {
        if (isLocalPlayer)
        {
            MainMenu = FindObjectOfType<MainMenuManager>();
            UpdateLobbyPlayerList();
        }
    }

    #region Room Client Callbacks

    ///// <summary>
    ///// This is a hook that is invoked on all player objects when entering the room.
    ///// <para>Note: isLocalPlayer is not guaranteed to be set until OnStartLocalPlayer is called.</para>
    ///// </summary>
    //public override void OnClientEnterRoom()
    //{
    //    Debug.Log(string.Format("Client {0} ({1}) has entered room.", index, PlayerName));

    //    if (isLocalPlayer)
    //    {
    //        MainMenuManager.instance.RefreshPlayerList();
    //    }

    //    ClientEnterRoom?.Invoke(this);
    //}

    ///// <summary>
    ///// This is a hook that is invoked on all player objects when exiting the room.
    ///// </summary>
    //public override void OnClientExitRoom()
    //{
    //    Debug.Log("Client has exited room.");
    //    ClientExitRoom?.Invoke(this);

    //}

    /// <summary>
    /// This is a hook that is invoked on clients when a RoomPlayer switches between ready or not ready.
    /// <para>This function is called when the a client player calls SendReadyToBeginMessage() or SendNotReadyToBeginMessage().</para>
    /// </summary>
    /// <param name="readyState">Whether the player is ready or not.</param>
    //public override void OnClientReady(bool readyState) {
    //    Debug.Log("Client is ready.");
    //}

    public override void OnStartLocalPlayer()
    {
        Debug.Log("OnStartLocalPlayer called. This should only be called per player.");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    public void UpdateLobbyPlayerList()
    {
        if (isLocalPlayer)
        {
            if (MainMenu != null)
                MainMenu.RefreshPlayerList();
        }
    }

    #endregion
}
