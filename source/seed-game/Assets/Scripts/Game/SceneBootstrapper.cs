using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Sirenix.OdinInspector;

public class SceneBootstrapper : MonoBehaviour
{
    [SceneObjectsOnly]
    public List<GameObject> ToBootstrapWithPlayer;

    [SceneObjectsOnly]
    public List<GameObject> ToBootstrapWithGameState;

    public void TryBootstrapAll()
    {
        if (SeedGameStateBase.instance == null)
        {
            return;
        }

        foreach (GameObject obj in ToBootstrapWithGameState)
        {
            IGameStateBootstrap[] casted = obj.GetComponents<IGameStateBootstrap>();
            foreach (IGameStateBootstrap toBootstrap in casted)
            {
                toBootstrap.SetupWithGameState(SeedGameStateBase.instance);
            }
        }

        if (ClientScene.localPlayer == null)
        {
            return;
        }

        SeedPlayer player = ClientScene.localPlayer.GetComponent<SeedPlayer>();

        if (player == null)
        {
            return;
        }

        foreach (GameObject obj in ToBootstrapWithPlayer)
        {
            ILocalPlayerBootstrap[] casted = obj.GetComponents<ILocalPlayerBootstrap>();
            foreach (ILocalPlayerBootstrap toBootstrap in casted)
            {
                toBootstrap.SetupWithPlayer(player);
            }
        }
    }
}
