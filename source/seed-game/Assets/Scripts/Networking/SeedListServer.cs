// add this component to the NetworkManager
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Mirror;
using Steamworks;
using System;

namespace Tember
{
    [System.Serializable]
    public class ServerListEntry
    {
        public CSteamID steamId;
        public string ip; // IP of the game server. This should be the host's IP.
        public ushort port; // Port of the game server. 
        public string name;
        public ushort currentPlayers;
        public ushort maxPlayers;
        public CSteamID gameServerId;

        public int lastLatency = -1;

#if !UNITY_WEBGL // Ping isn't known in WebGL builds
        public Ping ping;
#endif

        // Default Constructor.
        public ServerListEntry(CSteamID steamId)
        {
            this.steamId = steamId;
            this.currentPlayers = Convert.ToUInt16(SteamMatchmaking.GetNumLobbyMembers(steamId));
            this.maxPlayers = Convert.ToUInt16(SteamMatchmaking.GetLobbyMemberLimit(steamId));
            uint ip;
            bool bIsGameServerPresent = SteamMatchmaking.GetLobbyGameServer(steamId, out ip, out this.port, out this.gameServerId);
            this.ip = Networking.UIntToIP(ip);
            this.name = SteamMatchmaking.GetLobbyData(steamId, "name");

            ping = new Ping(this.ip);
        }

        public ServerListEntry(string ip, ushort port, string name, ushort currentPlayers, ushort maxPlayers)
        {
            this.ip = ip;
            this.port = port;
            this.name = name;
            this.currentPlayers = currentPlayers;
            this.maxPlayers = maxPlayers;
#if !UNITY_WEBGL // Ping isn't known in WebGL builds
            ping = new Ping(ip);
#endif
        }

        public override string ToString()
        {
            return string.Format(
@"Steam ID: {0}
Name: {1}
IP/port: {2}/{3}
Vacancy: {4}/{5}", 
steamId, name, ip, port, currentPlayers, maxPlayers);
        }

        public void PrintAllLobbyData()
        {
            int lobbyDataCount = SteamMatchmaking.GetLobbyDataCount(steamId);

            string lobbyDataString = string.Format("Lobby {0}'s data: \n", steamId);
            for (int lobbyDataIndex = 0; lobbyDataIndex < lobbyDataCount; lobbyDataIndex++)
            {
                string key, value;
                bool bIsDataFound = SteamMatchmaking.GetLobbyDataByIndex(steamId, lobbyDataIndex,
                    out key, Steamworks.Constants.k_nMaxLobbyKeyLength,
                    out value, Steamworks.Constants.k_cubChatMetadataMax);
                if (bIsDataFound)
                {
                    lobbyDataString += string.Format("Lobby Data {0}: [{1} : {2}]\n", lobbyDataIndex + 1, key, value);
                }
            }

            Debug.Log(lobbyDataString);
        }

        /* Comparers for sorting Servers */
        public static IComparer<ServerListEntry> GetServerPingComparer()
        {
            return Util.FunctionalComparer<ServerListEntry>.Create((s1, s2) => { return s2.ping.time - s1.ping.time; });
        }
    }

    public class SeedListServer : MonoBehaviour
    {
        CallResult<LobbyMatchList_t> m_CallResultLobbyMatchList;
        Callback<LobbyDataUpdate_t> m_CallbackLobbyInfo;

        public System.Action<ServerListEntry> ServerFound;
        public System.Action AllServersLoaded;

        public List<ServerListEntry> ServerEntries;

        private uint loadedCount;

        private uint numberServersToLoad;

        public void Start()
        {
            Setup();
            RefreshLobbyList();
        }

        void Setup()
        {
            m_CallResultLobbyMatchList = new CallResult<LobbyMatchList_t>();

            m_CallbackLobbyInfo = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataReceived);
        }

        public void RefreshLobbyList()
        {
            ServerEntries = new List<ServerListEntry>();
            SteamAPICall_t lobbyRequest = SteamMatchmaking.RequestLobbyList();
            m_CallResultLobbyMatchList.Set(lobbyRequest, OnLobbyMatchListReceived);
        }

        private void Update()
        {
            SteamAPI.RunCallbacks();
        }

        public void OnLobbyDataReceived(LobbyDataUpdate_t data)
        {
            CSteamID lobbyId = new CSteamID(data.m_ulSteamIDLobby);
            ServerListEntry entry = new ServerListEntry(lobbyId);
            ServerEntries.Add(entry);
            ServerFound?.Invoke(entry);
            ++loadedCount;
            if (loadedCount == numberServersToLoad)
            {
                AllServersLoaded?.Invoke();
            }
        }

        public void OnLobbyMatchListReceived(LobbyMatchList_t lobbyMatchList, bool bIsFailure)
        {
            if (bIsFailure)
            {
                Debug.Log("FAILED: Lobby List request from Steam.");
                return;
            }

            Debug.Log("SUCCESS: Lobby List request from Steam.");

            ServerEntries.Capacity = Convert.ToInt32(lobbyMatchList.m_nLobbiesMatching);

            loadedCount = 0;
            numberServersToLoad = lobbyMatchList.m_nLobbiesMatching;

            for (int lobbyIndex = 0; lobbyIndex < lobbyMatchList.m_nLobbiesMatching; lobbyIndex++)
            {
                CSteamID lobbyId = SteamMatchmaking.GetLobbyByIndex(lobbyIndex);
                bool bLobbyRequestDataSuccess = SteamMatchmaking.RequestLobbyData(lobbyId);
            }
        }

    }
}
