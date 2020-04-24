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

    [EnumToggleButtons]
    [SerializeField]
    private ConnectionType connectionType;

    [Required][AssetsOnly]
    [SerializeField]
    private SeedGameNetworkManager NetworkManagerPrefab;

    [ReadOnly]
    [SerializeField]
    private SeedGameNetworkManager networkManager;

    [SerializeField]
    private string networkAddress = "localhost";

    private void Awake()
    {
         networkManager = Instantiate<SeedGameNetworkManager>(NetworkManagerPrefab);

        switch (connectionType)
        {
            case ConnectionType.Host:
                networkManager.StartHost();
                break;
            case ConnectionType.Client:
                networkManager.networkAddress = networkAddress;
                networkManager.StartClient();
                break;
            case ConnectionType.Server:
                networkManager.StartServer();
                break;
            default:
                break;
        }

    }
}
