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

    [SerializeField]
    SeedUserProfile profile;

    public void Start()
    {
        if (SeedSteamManager.SeedInstance.LocalUserProfile.SteamID.m_SteamID == 0)
        {
            SeedSteamManager.SeedInstance.SelfUserInfoLoaded += SetupWithUserProfile;
        } else
        {
            SetupWithUserProfile(SeedSteamManager.SeedInstance.LocalUserProfile);
        }
    }

    public void SetupWithUserProfile(SeedUserProfile profile)
    {
        this.profile = profile;
        SetSteamUserInfo(profile);
        profile.ProfileUpdatedEvent += SetSteamUserInfo;
    }

    private void SetSteamUserInfo(SeedUserProfile profile)
    {
        PlayerSteamIdText.text = profile.SteamID.ToString();
        PlayerNameText.text = profile.Name;
        PlayerAvatarImage.sprite = profile.UserAvatarSprite;
    }
}
