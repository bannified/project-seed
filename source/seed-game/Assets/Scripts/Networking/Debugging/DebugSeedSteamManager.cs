using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugSeedSteamManager : SeedSteamManager
{

    // Just a stub to prevent Steam from initializing.
    protected override void Awake()
    {
        if (s_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        s_instance = this;
    }
}
