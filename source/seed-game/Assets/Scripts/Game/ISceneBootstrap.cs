using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerBootstrap
{
    void SetupWithPlayer(SeedPlayer player);
}

public interface IGameStateBootstrap
{
    void SetupWithGameState();
}
