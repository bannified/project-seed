using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerCellController : MonoBehaviour
{
    [SerializeField]
    private TMPro.TMP_Text PlayerNameText;

    public void SetPlayerName(string name)
    {
        PlayerNameText.text = name;
    }

}
