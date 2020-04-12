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
        SeedSteamManager.SeedInstance.SelfUserInfoLoaded += SetSteamUserInfo;
        SeedSteamManager.SeedInstance.SelfUserAvatarLoaded += SetAvatarImage;
    }

    private void SetSteamUserInfo()
    {
        SeedSteamManager steam = SeedSteamManager.SeedInstance;
        PlayerSteamIdText.text = steam.LocalUserProfile.SteamID.ToString();
        PlayerNameText.text = steam.LocalUserProfile.Name;
        PlayerAvatarImage.sprite = steam.LocalUserProfile.UserAvatarSprite;
    }

    private void SetAvatarImage()
    {
        SeedSteamManager steam = SeedSteamManager.SeedInstance;
        PlayerAvatarImage.sprite = steam.LocalUserProfile.UserAvatarSprite;
    }
}
