using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class SeedSteamLobby : MonoBehaviour
{
    private static readonly int MAX_CHAT_MSG_LEN = 500;


    /* Lobby-specific Lobby Data Keys */
    public const string LOBBY_STATUS_KEY = "LobbyStatus";

    public const string LOBBY_STATUS_STARTED_VALUE = "Started";
    public const string LOBBY_STATUS_STARTING_VALUE = "Starting";
    public const string LOBBY_STATUS_WAITING_VALUE = "Waiting";

    /* Player-specific Lobby Data Keys */
    public static readonly string PLAYER_LOBBY_DATA_READY = "IsReady";

    public System.Action<CSteamID> LobbyCreatedEvent;
    public System.Action<CSteamID> LobbyEnterEvent;
    public System.Action<CSteamID> PlayerDataUpdated;

    public System.Action<CSteamID> PlayerLeaveEvent;
    public System.Action<CSteamID> PlayerEnterEvent;

    public System.Action GameStartInterruptedEvent;
    public System.Action GameStartInitiatedEvent;
    public System.Action GameStartedEvent;

    public System.Action<string, SeedUserProfile> ChatMessageReceivedEvent;

    private Callback<LobbyCreated_t> m_lobbyCreatedCallback;
    private Callback<LobbyEnter_t> m_lobbyEnterCallback;
    private Callback<LobbyDataUpdate_t> m_lobbyDataUpdateCallback;
    private Callback<LobbyChatUpdate_t> m_lobbyChatUpdateCallback;

    private Callback<LobbyChatMsg_t> m_lobbyChatMessageCallback;

    public CSteamID LobbySteamID { get { return _LobbySteamID; } }
    [SerializeField]
    private CSteamID _LobbySteamID;

    public CSteamID CreatedSteamID { get { return _CreatedLobbySteamID; } }
    [SerializeField]
    private CSteamID _CreatedLobbySteamID;

    public CSteamID LobbyOwnerID { get { return _LobbyOwnerID; } }
    [SerializeField]
    private CSteamID _LobbyOwnerID;

    /* Players in Lobby */
    [SerializeField]
    private List<CSteamID> _LobbyMembersSteamIDs = new List<CSteamID>();
    public IReadOnlyCollection<CSteamID> LobbyMembersSteamIDs { get { return _LobbyMembersSteamIDs.AsReadOnly(); } }

    private Dictionary<CSteamID, bool> _PlayerToReadyStateDict = new Dictionary<CSteamID, bool>();
    public IReadOnlyDictionary<CSteamID, bool> PlayerToReadyStateDict { get { return _PlayerToReadyStateDict; } }

    public void Awake()
    {
        m_lobbyCreatedCallback = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        m_lobbyEnterCallback = Callback<LobbyEnter_t>.Create(OnLobbyEnter);
        m_lobbyDataUpdateCallback = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
        m_lobbyChatUpdateCallback = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
        m_lobbyChatMessageCallback = Callback<LobbyChatMsg_t>.Create(OnReceiveChatMessage);
    }

    public void CreateLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 8);
    }

    public void LeaveLobby()
    {
        _LobbyMembersSteamIDs.Clear();
        SteamMatchmaking.LeaveLobby(LobbySteamID);
    }

    /* -------- Callbacks for SteamMatchmaking API ----------- */

    private void OnLobbyCreated(LobbyCreated_t createdMsg)
    {
        switch (createdMsg.m_eResult)
        {
            case EResult.k_EResultOK:
                break;
            case EResult.k_EResultInvalidPassword:
            case EResult.k_EResultFail:
            default:
                Debug.LogFormat("Error when creating Lobby: {0}", createdMsg.m_eResult);
                break;
        }
        _CreatedLobbySteamID = new CSteamID(createdMsg.m_ulSteamIDLobby);
        LobbyCreatedEvent?.Invoke(_CreatedLobbySteamID);
    }

    private void OnLobbyEnter(LobbyEnter_t enteredMsg)
    {
        _LobbySteamID = new CSteamID(enteredMsg.m_ulSteamIDLobby);
        _LobbyOwnerID = SteamMatchmaking.GetLobbyOwner(_LobbySteamID);
        InitPlayerList();
        LobbyEnterEvent?.Invoke(_LobbySteamID);
    }

    private void OnLobbyDataUpdate(LobbyDataUpdate_t dataUpdateMsg)
    {
        CSteamID steamId = new CSteamID(dataUpdateMsg.m_ulSteamIDMember);

        UpdateAllLobbyMemberData(steamId);

        CheckLobbyGameStatus();

        PlayerDataUpdated?.Invoke(steamId);
    }

    private void CheckLobbyGameStatus()
    {
        string value = SteamMatchmaking.GetLobbyData(LobbySteamID, LOBBY_STATUS_KEY);
        switch (value)
        {
            case LOBBY_STATUS_STARTED_VALUE:
                GameStartedEvent?.Invoke();
                break;
            case LOBBY_STATUS_STARTING_VALUE:
                GameStartInitiatedEvent?.Invoke();
                break;
            case LOBBY_STATUS_WAITING_VALUE:
                GameStartInterruptedEvent?.Invoke();
                break;
            default:
                break;
        }
    }

    private void OnLobbyChatUpdate(LobbyChatUpdate_t chatUpdateMsg)
    {
        SeedUserProfile userChanged = SeedSteamManager.SeedInstance.TryAddProfile(chatUpdateMsg.m_ulSteamIDUserChanged);
        EChatMemberStateChange stateChange = (EChatMemberStateChange)chatUpdateMsg.m_rgfChatMemberStateChange;
        switch (stateChange)
        {
            case EChatMemberStateChange.k_EChatMemberStateChangeEntered:
                // add to list
                AddPlayerToLobbyList(userChanged.SteamID);
                break;
            case EChatMemberStateChange.k_EChatMemberStateChangeLeft:
            case EChatMemberStateChange.k_EChatMemberStateChangeKicked:
            case EChatMemberStateChange.k_EChatMemberStateChangeDisconnected:
            case EChatMemberStateChange.k_EChatMemberStateChangeBanned:
                RemovePlayerFromLobbyList(userChanged.SteamID);
                // leave
                break;
            default:
                break;
        }
    }

    /* ------- END callback methods for SteamMatchmaking API --------- */

    private void UpdateAllLobbyMemberData(CSteamID playerId)
    {
        string value = SteamMatchmaking.GetLobbyMemberData(LobbySteamID, playerId, PLAYER_LOBBY_DATA_READY);
        bool isPlayerReady = value == "true";
        SetPlayerReadyState(playerId, isPlayerReady);
    }

    private void AddPlayerToLobbyList(CSteamID playerId)
    {
        _LobbyMembersSteamIDs.Add(playerId);
        SeedSteamManager.SeedInstance.FetchSteamUserInfo(playerId);
        PlayerEnterEvent?.Invoke(playerId);
    }

    private void RemovePlayerFromLobbyList(CSteamID playerId)
    {
        _LobbyMembersSteamIDs.Remove(playerId);
        PlayerLeaveEvent?.Invoke(playerId);
    }

    /// <summary>
    /// Reloads the lobby member list.
    /// </summary>
    /// <returns>True if the new list is different from the old</returns>
    private bool InitPlayerList()
    {
        List<CSteamID> before = new List<CSteamID>(_LobbyMembersSteamIDs);
        bool bIsListDiff = false;
        _LobbyMembersSteamIDs.Clear();

        int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers(_LobbySteamID);
        for (int lobbyMemberIndex = 0; lobbyMemberIndex < numLobbyMembers; lobbyMemberIndex++)
        {
            CSteamID member = SteamMatchmaking.GetLobbyMemberByIndex(_LobbySteamID, lobbyMemberIndex);
            AddPlayerToLobbyList(member);
            UpdateAllLobbyMemberData(member);

            if (before.Count > lobbyMemberIndex)
            {
                if (before[lobbyMemberIndex] != _LobbyMembersSteamIDs[lobbyMemberIndex])
                {
                    bIsListDiff = true;
                }
            } else
            {
                bIsListDiff = true;
            }
        }

        if (before.Count != _LobbyMembersSteamIDs.Count)
        {
            bIsListDiff = true;
        }

        return bIsListDiff;
    }

    public void Join(string lobbyId)
    {
        SteamMatchmaking.JoinLobby(new CSteamID(System.Convert.ToUInt64(lobbyId)));
    }

    public bool IsPlayerReady(CSteamID playerID)
    {
        bool result = false;
        PlayerToReadyStateDict.TryGetValue(playerID, out result);
        return result;
    }

    private void SetPlayerReadyState(CSteamID playerID, bool state)
    {
        _PlayerToReadyStateDict[playerID] = state;
    }

    public void SetSelfReady(bool state)
    {
        if (SeedSteamManager.SeedInstance == null)
        {
            return;
        }

        string value = state ? "true" : "false";
        SteamMatchmaking.SetLobbyMemberData(_LobbySteamID, PLAYER_LOBBY_DATA_READY, value);
    }

    public void InitiateGameStart()
    {
        if (SeedSteamManager.SeedInstance == null)
        {
            return;
        }

        SteamMatchmaking.SetLobbyData(_LobbySteamID, LOBBY_STATUS_KEY, LOBBY_STATUS_STARTING_VALUE);
    }

    public void GameStart()
    {
        if (SeedSteamManager.SeedInstance == null)
        {
            return;
        }

        SteamMatchmaking.SetLobbyData(_LobbySteamID, LOBBY_STATUS_KEY, LOBBY_STATUS_STARTED_VALUE);
        //SeedGameNetworkManager.SeedInstance.HostChangeToGameScene();
    }

    public bool IsLobbyReady()
    {
        foreach (var id in _LobbyMembersSteamIDs)
        {
            bool ready = false;
            _PlayerToReadyStateDict.TryGetValue(id, out ready);
            if (!ready)
            {
                return false;
            }
        }

        return true;
    }

    /* Lobby Chat */

    public void SendChatMessage(string msg, SeedUserProfile sender = null)
    {
        SteamMatchmaking.SendLobbyChatMsg(LobbySteamID, msg.ToEncodedByteArray(System.Text.Encoding.UTF32), msg.Length * 4);
    }

    public void OnReceiveChatMessage(LobbyChatMsg_t chatMessage)
    {
        CSteamID chatterId;
        EChatEntryType chatEntryType;
        byte[] arr = new byte[MAX_CHAT_MSG_LEN];
        int msgLen = SteamMatchmaking.GetLobbyChatEntry(new CSteamID(chatMessage.m_ulSteamIDLobby), System.Convert.ToInt32(chatMessage.m_iChatID),
            out chatterId, arr, MAX_CHAT_MSG_LEN, out chatEntryType);

        string msg = System.Text.Encoding.UTF32.GetString(arr);
        msg = msg.TrimEnd('\0');
        SeedUserProfile chatterProfile = SeedSteamManager.SeedInstance.TryGetProfile(chatterId);

        ChatMessageReceivedEvent?.Invoke(msg, chatterProfile);
    }
}
