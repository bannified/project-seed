using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SteamPlayerProfileGroup : MonoBehaviour
{
    [SerializeField]
    private TMP_Text PlayerNameText;

    [SerializeField]
    private TMP_Text PlayerSteamIdText;

    [SerializeField]
    private Image PlayerAvatarImage;

    private void Start()
    {
        SeedSteamManager.SeedInstance.SteamSelfUserInfoLoaded += SetSteamUserInfo;
        SeedSteamManager.SeedInstance.UserAvatarLoaded += SetAvatarImage;
    }

    private void SetSteamUserInfo()
    {
        SeedSteamManager steam = SeedSteamManager.SeedInstance;
        PlayerSteamIdText.text = steam.UserSteamID.m_SteamID.ToString();
        PlayerNameText.text = steam.UserSteamName;
    }

    private void SetAvatarImage(Sprite avatarSprite)
    {
        PlayerAvatarImage.sprite = avatarSprite;
    }
}
