using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class SeedSteamLobby : MonoBehaviour
{
    private static readonly int MAX_CHAT_MSG_LEN = 500;

    public System.Action<CSteamID> LobbyCreatedEvent;
    public System.Action<CSteamID> LobbyEnterEvent;
    public System.Action<IReadOnlyCollection<CSteamID>> LobbyDataUpdated;

    public System.Action<CSteamID> PlayerLeaveEvent;
    public System.Action<CSteamID> PlayerEnterEvent;

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
    }

    private void OnLobbyEnter(LobbyEnter_t enteredMsg)
    {
        _LobbySteamID = new CSteamID(enteredMsg.m_ulSteamIDLobby);
        LobbyEnterEvent?.Invoke(_LobbySteamID);
    }

    private void OnLobbyDataUpdate(LobbyDataUpdate_t dataUpdateMsg)
    {
        InitPlayerList();
        LobbyDataUpdated?.Invoke(LobbyMembersSteamIDs);
    }

    private void OnLobbyChatUpdate(LobbyChatUpdate_t chatUpdateMsg)
    {
        SeedUserProfile userChanged = SeedSteamManager.SeedInstance.TryGetProfile(chatUpdateMsg.m_ulSteamIDUserChanged);
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

    public void SetPlayerReadyState(CSteamID playerID, bool state)
    {
        _PlayerToReadyStateDict[playerID] = state;
    }

    public void SetSelfReady(bool state)
    {
        if (SeedSteamManager.SeedInstance == null)
        {
            return;
        }

        string key = string.Format("{0}_readystate", SeedSteamManager.SeedInstance.LocalUserProfile.SteamID.m_SteamID);
        string value = state ? "true" : "false";
        SteamMatchmaking.SetLobbyMemberData(_LobbySteamID, key, "true");
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
