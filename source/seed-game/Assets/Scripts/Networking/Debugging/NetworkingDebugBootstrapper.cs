using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Mirror;

public class NetworkingDebugBootstrapper : MonoBehaviour
{
    enum ConnectionType
    {
        Host,
        Client,
        Server
    }

    enum TransportType
    {
        Steam, // Requires Steam Client
        Telepathy, // Direct IP/LAN
    }

    [EnumToggleButtons]
    [SerializeField]
    private TransportType transportType = TransportType.Telepathy;

    [EnumToggleButtons]
    [SerializeField]
    private ConnectionType connectionType;

    [Required][AssetsOnly]
    [SerializeField]
    private SeedGameNetworkManager NetworkManagerPrefab;

    [Required]
    [AssetsOnly]
    [SerializeField]
    private DebugSeedSteamManager DebugSteamManagerPrefab;

    [SerializeField]
    private SeedUserProfile debugUserProfile;

    [ReadOnly]
    [SerializeField]
    private DebugSeedSteamManager steamManager;
    [ReadOnly]
    [SerializeField]
    private SeedGameNetworkManager networkManager;

    [SerializeField]
    private string networkAddress = "localhost";

    [SerializeField]
    private int numExpectedPlayers;

    private void Awake()
    {
        steamManager = Instantiate<DebugSeedSteamManager>(DebugSteamManagerPrefab);
        steamManager.LocalUserProfile = debugUserProfile;

        networkManager = Instantiate<SeedGameNetworkManager>(NetworkManagerPrefab);
        networkManager.numExpectedPlayers = numExpectedPlayers;

        switch (transportType)
        {
            case TransportType.Steam:
                networkManager.SwitchToSteamTransport();
                break;
            case TransportType.Telepathy:
                networkManager.SwitchToDirectIPTransport();
                break;
        }

        networkManager.networkAddress = networkAddress;

        switch (connectionType)
        {
            case ConnectionType.Host:
                networkManager.StartHost();
                break;
            case ConnectionType.Client:
                networkManager.StartClient();
                break;
            case ConnectionType.Server:
                networkManager.StartServer();
                break;
            default:
                break;
        }

        Destroy(this.gameObject);

    }
}
