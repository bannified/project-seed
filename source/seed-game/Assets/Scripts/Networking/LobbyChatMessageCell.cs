using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LobbyChatMessageCell : MonoBehaviour
{
    [SerializeField]
    private TMP_Text messageText;

    public void Setup(string username, string message) {
        messageText.text = string.Format("{0}: {1}", username, message);
    }
}
