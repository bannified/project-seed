using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ServerListEntryCell : MonoBehaviour
{
    [SerializeField]
    TMP_Text ServerNameText;

    [SerializeField]
    TMP_Text PingText;

    [SerializeField]
    TMP_Text VacancyText;

    [SerializeField]
    Button JoinButton;

    [SerializeField]
    Tember.ServerListEntry serverEntry;

    public void Setup(Tember.ServerListEntry entry)
    {
        ServerNameText.text = entry.name;
        PingText.text = entry.ping.time.ToString();
        VacancyText.text = string.Format("{0}/{1}", entry.currentPlayers, entry.maxPlayers);
    }

    public string GetServerIP()
    {
        if (serverEntry == null) {
            return "0.0.0.0";
        }

        if (serverEntry.port == 0)
        {
            return serverEntry.ip;
        }

        return string.Format("{0}:{1}", serverEntry.ip, serverEntry.port);
    }
}
