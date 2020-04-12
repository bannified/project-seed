using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class SeedSteamLobby : MonoBehaviour
{
    public System.Action<CSteamID> LobbyCreatedEvent;
    public System.Action<CSteamID> LobbyEnterEvent;
    public System.Action<IReadOnlyCollection<CSteamID>> LobbyDataUpdated;

    private Callback<LobbyCreated_t> m_lobbyCreatedCallback;
    private Callback<LobbyEnter_t> m_lobbyEnterCallback;
    private Callback<LobbyDataUpdate_t> m_lobbyDataUpdateCallback;

    public CSteamID LobbySteamID { get; }
    [SerializeField]
    private CSteamID _LobbySteamID;

    public CSteamID CreatedSteamID { get; }
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
        _LobbyMembersSteamIDs.Clear();

        int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers(_LobbySteamID);
        _LobbyMembersSteamIDs.Capacity = numLobbyMembers;
        for (int lobbyMemberIndex = 0; lobbyMemberIndex < numLobbyMembers; lobbyMemberIndex++)
        {
            CSteamID member = SteamMatchmaking.GetLobbyMemberByIndex(_LobbySteamID, lobbyMemberIndex);
            _LobbyMembersSteamIDs.Add(member);
        }

        LobbyDataUpdated?.Invoke(LobbyMembersSteamIDs);
    }

    public void Join(string lobbyId)
    {
        SteamMatchmaking.JoinLobby(new CSteamID(System.Convert.ToUInt64(lobbyId)));
    }
}
