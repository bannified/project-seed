using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILocalPlayerBootstrap
{
    void SetupWithPlayer(SeedPlayer player);
}

public interface IGameStateBootstrap
{
    void SetupWithGameState(SeedGameStateBase gameState);
}
