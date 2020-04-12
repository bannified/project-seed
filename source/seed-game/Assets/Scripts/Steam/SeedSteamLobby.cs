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

    public System.Action<string, SeedUserProfile> ChatMessageReceivedEvent;

    private Callback<LobbyCreated_t> m_lobbyCreatedCallback;
    private Callback<LobbyEnter_t> m_lobbyEnterCallback;
    private Callback<LobbyDataUpdate_t> m_lobbyDataUpdateCallback;
    private Callback<LobbyChatUpdate_t> m_lobbyChatUpdateCallback;

    private Callback<LobbyChatMsg_t> m_lobbyChatMessageCallback;

    public CSteamID LobbySteamID { get { return _CreatedLobbySteamID; } }
    [SerializeField]
    private CSteamID _LobbySteamID;

    public CSteamID CreatedSteamID { get { return CreatedSteamID; } }
    [SerializeField]
    private CSteamID _CreatedLobbySteamID;

    [SerializeField]
    private List<CSteamID> _LobbyMembersSteamIDs = new List<CSteamID>();
    public IReadOnlyCollection<CSteamID> LobbyMembersSteamIDs { get { return _LobbyMembersSteamIDs.AsReadOnly(); } }

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

        UpdateLobbyMembersList();
        LobbyDataUpdated?.Invoke(LobbyMembersSteamIDs);
    }

    private void OnLobbyChatUpdate(LobbyChatUpdate_t chatUpdateMsg)
    {
        UpdateLobbyMembersList();
        LobbyDataUpdated?.Invoke(LobbyMembersSteamIDs);
    }

    private void UpdateLobbyMembersList()
    {
        _LobbyMembersSteamIDs.Clear();

        int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers(_LobbySteamID);
        for (int lobbyMemberIndex = 0; lobbyMemberIndex < numLobbyMembers; lobbyMemberIndex++)
        {
            CSteamID member = SteamMatchmaking.GetLobbyMemberByIndex(_LobbySteamID, lobbyMemberIndex);
            _LobbyMembersSteamIDs.Add(member);

            SeedSteamManager.SeedInstance.FetchSteamUserInfo(member);
        }
    }

    public void Join(string lobbyId)
    {
        SteamMatchmaking.JoinLobby(new CSteamID(System.Convert.ToUInt64(lobbyId)));
    }

    public void SendChatMessage(string msg, SeedUserProfile sender = null)
    {
        SteamMatchmaking.SendLobbyChatMsg(LobbySteamID, msg.ToEncodedByteArray(System.Text.Encoding.UTF32), msg.Length*4);
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
