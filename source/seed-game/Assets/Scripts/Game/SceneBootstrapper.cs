using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SceneBootstrapper : MonoBehaviour
{
    public List<GameObject> ToBootstrapWithPlayer;
    public List<GameObject> ToBootstrapWithGameState;

    private void Start()
    {
        foreach (GameObject obj in ToBootstrapWithGameState)
        {
            IGameStateBootstrap[] casted = obj.GetComponents<IGameStateBootstrap>();
            foreach (IGameStateBootstrap toBootstrap in casted)
            {
                toBootstrap.SetupWithGameState();
            }
        }

        SeedPlayer player = ClientScene.localPlayer.GetComponent<SeedPlayer>();

        if (player == null)
        {
            return;
        }

        foreach (GameObject obj in ToBootstrapWithPlayer)
        {
            IPlayerBootstrap[] casted = obj.GetComponents<IPlayerBootstrap>();
            foreach (IPlayerBootstrap toBootstrap in casted)
            {
                toBootstrap.SetupWithPlayer(player);
            }
        }
    }

}
