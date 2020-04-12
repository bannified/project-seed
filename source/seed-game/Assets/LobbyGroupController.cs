using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using Steamworks;
using TMPro;

public class LobbyGroupController : MonoBehaviour
{
    [SerializeField]
    private GameObject LobbyPlayerCellPrefab;

    [SerializeField]
    private GameObject PlayersPanel;

    [SerializeField]
    private Dictionary<string, LobbyPlayerCellController> playerNameToLobbyCellMap;

    [SerializeField]
    private ScrollRect ChatScrollRect;

    [SerializeField]
    private TMP_InputField ChatMessageInputField;

    [SerializeField]
    private SeedSteamLobby CurrentLobby;

    [SerializeField]
    private TMP_Text ChatHistoryText;

    private void Awake()
    {
        playerNameToLobbyCellMap = new Dictionary<string, LobbyPlayerCellController>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            // Send message
            SendLobbyChatMessage();
        }
    }

    public void SetupWithLobby(SeedSteamLobby lobby)
    {
        this.CurrentLobby = lobby;
        CurrentLobby.ChatMessageReceivedEvent += OnReceiveLobbyChatMessage;
    }

    private void SendLobbyChatMessage()
    {
        SeedUserProfile profile = SeedSteamManager.SeedInstance.LocalUserProfile;
        if (profile.SteamID.m_SteamID == 0)
        {
            return;
        }

        CurrentLobby.SendChatMessage(ChatMessageInputField.text, profile);
        ChatMessageInputField.text = "";
        ChatMessageInputField.ActivateInputField();
    }

    private void OnReceiveLobbyChatMessage(string message, SeedUserProfile sender)
    {
        Debug.LogFormat("Chat message received from {0}: {1}", sender.Name, message);
        string msg = string.Format("{0}: {1}{2}", sender.Name, message, System.Environment.NewLine);
        ChatHistoryText.text += msg;
        ChatScrollRect.verticalScrollbar.value = 0;
    }

    public void AddPlayers(List<SeedNetworkRoomPlayer> players)
    {
        foreach (var player in players)
        {
            AddPlayer(player);
        }
    }

    public void AddPlayer(SeedUserProfile profile)
    {
        LobbyPlayerCellController cell = Instantiate(LobbyPlayerCellPrefab, PlayersPanel.transform).GetComponent<LobbyPlayerCellController>();
        cell.SetupWithUserProfile(profile);

        playerNameToLobbyCellMap.Add(profile.SteamID.m_SteamID.ToString(), cell);
    }

    public void AddPlayer(SeedNetworkRoomPlayer player)
    {
        LobbyPlayerCellController cell = Instantiate(LobbyPlayerCellPrefab, PlayersPanel.transform).GetComponent<LobbyPlayerCellController>();
        cell.SetPlayerName(player.PlayerName);

        playerNameToLobbyCellMap.Add(player.PlayerName, cell);
    }

    public void RemovePlayer(string playerName)
    {
        LobbyPlayerCellController cell;
        if (playerNameToLobbyCellMap.TryGetValue(playerName, out cell))
        {
            Destroy(cell.gameObject);
            playerNameToLobbyCellMap.Remove(playerName);
        }
    }

    public void ClearAll()
    {
        ClearAllPlayerLobbyCells();
    }

    public void ClearAllPlayerLobbyCells()
    {
        foreach (var cell in playerNameToLobbyCellMap)
        {
            Destroy(cell.Value.gameObject);
        }

        playerNameToLobbyCellMap.Clear();
    }

    public void UpdateChatLobby(IReadOnlyCollection<CSteamID> steamLobbyPlayerList)
    {
        if (SeedSteamManager.SeedInstance == null)
        {
            return;
        }

        HashSet<string> playersInRoom = new HashSet<string>(playerNameToLobbyCellMap.Keys);
        if (steamLobbyPlayerList != null)
        {
            foreach (var player in steamLobbyPlayerList)
            {
                SeedUserProfile profile = SeedSteamManager.SeedInstance.TryGetProfile(player);
                if (profile == null) continue;

                if (!playerNameToLobbyCellMap.ContainsKey(player.m_SteamID.ToString()))
                {
                    AddPlayer(profile);
                }
                playersInRoom.Remove(profile.SteamID.m_SteamID.ToString());
            }

            foreach (var oldPlayer in playersInRoom)
            {
                RemovePlayer(oldPlayer);
            }
        }
    }

    public void UpdateLobby()
    {
        HashSet<string> playersInRoom = new HashSet<string>(playerNameToLobbyCellMap.Keys);
        if (NetworkManager.singleton is SeedNetworkRoomManager room)
        {
            foreach (var player in room.roomPlayers)
            {
                if (!playerNameToLobbyCellMap.ContainsKey(player.PlayerName))
                {
                    AddPlayer(player);
                }
                playersInRoom.Remove(player.PlayerName);
            }

            foreach (var oldPlayer in playersInRoom)
            {
                RemovePlayer(oldPlayer);
            }
        }
    }
}
