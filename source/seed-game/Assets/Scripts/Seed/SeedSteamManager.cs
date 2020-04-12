﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using UnityEngine.UI;
using System;

[System.Serializable]
public class SeedUserProfile
{
    public string Name;
    public int UserAvatarHandle;
    public Sprite UserAvatarSprite;
    public CSteamID SteamID;

    public SeedUserProfile(ulong ulSteamId)
    {
        SteamID = new CSteamID(ulSteamId);
        Name = SteamFriends.GetFriendPersonaName(SteamID);
        UserAvatarHandle = SteamFriends.GetLargeFriendAvatar(SteamID);
    }

    public static bool operator==(SeedUserProfile p1, SeedUserProfile p2)
    {
        return p1.SteamID.m_SteamID == p2.SteamID.m_SteamID;
    }
    public static bool operator !=(SeedUserProfile p1, SeedUserProfile p2)
    {
        return p1.SteamID.m_SteamID != p2.SteamID.m_SteamID;
    }

    public override bool Equals(object obj) 
    {
        SeedUserProfile casted = obj as SeedUserProfile;
        if (obj == null || casted == null) return false;
        return casted == this;
    }

    public override int GetHashCode()
    {
        return SteamID.m_SteamID.GetHashCode();
    }

    public override string ToString()
    {
        return string.Format("SeedUserProfile: {0}, {1}, {2}, {3}", Name, SteamID.m_SteamID.ToString(), UserAvatarHandle, UserAvatarSprite);
    }

}

public class SeedSteamManager : SteamManager
{

    public static SeedSteamManager SeedInstance { get { return Instance as SeedSteamManager; } }

    [Header("Essentials")]
    [SerializeField]
    public CSteamID UserSteamID;
    public SeedUserProfile LocalUserProfile;
    public string UserSteamName;

    [Header("Cosmetics")]
    public Sprite UserAvatar;

    int UserAvatarHandle = 0;
    bool bSelfUserDataLoaded = false;

    [Header("Other Users Cache")]
    private Dictionary<CSteamID, SeedUserProfile> _AllLoadedUserProfiles = new Dictionary<CSteamID, SeedUserProfile>();
    public IReadOnlyDictionary<CSteamID, SeedUserProfile> AllLoadedUserProfiles { get { return _AllLoadedUserProfiles; } }

    // Events
    public System.Action<CSteamID> SteamUserInfoLoaded;
    public System.Action<SeedUserProfile> UserAvatarLoaded;
    public System.Action SelfUserAvatarLoaded;
    public System.Action SelfUserInfoLoaded;

    // Callbacks
    private Callback<AvatarImageLoaded_t> avatarCallback;
    private Callback<PersonaStateChange_t> personaStateChangeCallback;

    protected override void Awake()
    {
        base.Awake();
        avatarCallback = new Callback<AvatarImageLoaded_t>(OnAvatarImageLoaded);
        personaStateChangeCallback = new Callback<PersonaStateChange_t>(OnPersonaStateLoaded);
    }

    // Start is called before the first frame update
    protected void Start()
    {
        FetchSelfSteamInfo();
    }

    private void FetchSelfSteamInfo()
    {
        if (Initialized)
        {
            UserSteamID = SteamUser.GetSteamID();

            LocalUserProfile = TryAddProfile(UserSteamID.m_SteamID);

            if (bSelfUserDataLoaded)
            {
                CancelInvoke("FetchSelfSteamInfo");
                return;
            }

            FetchSteamUserInfo(UserSteamID);
        }
    }

    private void FetchSteamUserInfo(CSteamID steamID)
    {
        if (!SteamFriends.RequestUserInformation(steamID, false)) // Goes to a PersonaStateChange_t callback if not loaded
        {
            // loaded already.
            HandleSteamUserLoaded(steamID.m_SteamID);
        }
    }

    private void OnAvatarImageLoaded(AvatarImageLoaded_t avatar)
    {
        Sprite result = TryConvertHandleToSprite(avatar.m_iImage);

        if (result != null)
        {
            SeedUserProfile profile;

            if (!_AllLoadedUserProfiles.TryGetValue(avatar.m_steamID, out profile))
            {
                profile = new SeedUserProfile(avatar.m_steamID.m_SteamID);
                _AllLoadedUserProfiles.Add(profile.SteamID, profile);
            }

            profile.UserAvatarSprite = result;

            if (profile.SteamID.m_SteamID == UserSteamID.m_SteamID)
            {
                LocalUserProfile.UserAvatarSprite = result;
                SelfUserAvatarLoaded?.Invoke();
            }

            UserAvatarLoaded?.Invoke(profile);

        }
    }

    private Sprite TryConvertHandleToSprite(int avatarHandle)
    {
        uint imageWidth, imageHeight;
        SteamUtils.GetImageSize(avatarHandle, out imageWidth, out imageHeight);

        int uImageSizeInBytes = System.Convert.ToInt32(imageWidth * imageHeight * 4);

        byte[] imageBuf = new byte[uImageSizeInBytes];
        bool success = SteamUtils.GetImageRGBA(avatarHandle, imageBuf, uImageSizeInBytes);

        if (success)
        {
            Texture2D tex = new Texture2D(System.Convert.ToInt32(imageWidth), System.Convert.ToInt32(imageHeight), TextureFormat.RGBA32, false);
            tex.LoadRawTextureData(imageBuf);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, -tex.height), new Vector2());
        }

        return null;
    }

    private void OnPersonaStateLoaded(PersonaStateChange_t persona)
    {
        SteamUserInfoLoaded?.Invoke(new CSteamID(persona.m_ulSteamID));
        HandleSteamUserLoaded(persona.m_ulSteamID);
    }

    public SeedUserProfile TryGetProfile(CSteamID steamID)
    {
        SeedUserProfile profile;

        if (_AllLoadedUserProfiles.TryGetValue(steamID, out profile))
        {
            return profile;
        }

        return null;
    }

    public SeedUserProfile TryGetProfile(ulong steamID)
    {
        SeedUserProfile profile;

        if (_AllLoadedUserProfiles.TryGetValue(new CSteamID(steamID), out profile))
        {
            return profile;
        }

        return null;
    }

    private SeedUserProfile TryAddProfile(ulong steamID)
    {
        SeedUserProfile profile;

        if (!_AllLoadedUserProfiles.TryGetValue(new CSteamID(steamID), out profile))
        {
            profile = new SeedUserProfile(steamID);
            _AllLoadedUserProfiles.Add(profile.SteamID, profile);
        }

        return profile;
    }

    private void HandleSteamUserLoaded(ulong steamID)
    {
        SeedUserProfile profile = TryAddProfile(steamID);
        profile.UserAvatarSprite = TryConvertHandleToSprite(profile.UserAvatarHandle);
        if (profile == LocalUserProfile) // self
        {
            bSelfUserDataLoaded = true;
            SelfUserInfoLoaded?.Invoke();
        }

        SteamUserInfoLoaded?.Invoke(profile.SteamID);
    }

}
