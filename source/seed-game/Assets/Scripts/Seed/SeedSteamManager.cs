using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using UnityEngine.UI;

[System.Serializable]
public class SeedUserProfile
{
    public string Name;
    public int UserAvatarHandle;
    public Sprite UserAvatarSprite;
    public CSteamID SteamID;

    public delegate void ProfileUpdatedDelegate(SeedUserProfile profile);
    public event ProfileUpdatedDelegate ProfileUpdatedEvent;

    public SeedUserProfile(ulong ulSteamId)
    {
        SteamID = new CSteamID(ulSteamId);
    }

    public void LoadSteamData()
    {
        Name = SteamFriends.GetFriendPersonaName(SteamID);
        UserAvatarHandle = SteamFriends.GetLargeFriendAvatar(SteamID);
        ProfileUpdatedEvent?.Invoke(this);
    }

    public void SetAvatarSprite(Sprite sprite)
    {
        UserAvatarSprite = sprite;
        ProfileUpdatedEvent?.Invoke(this);
    }

    public static bool operator==(SeedUserProfile p1, SeedUserProfile p2)
    {
        if (p1 is null) return p2 is null;
        return p1.Equals(p2);
    }
    public static bool operator !=(SeedUserProfile p1, SeedUserProfile p2)
    {
        return !p1.Equals(p2);
    }

    public override bool Equals(object obj) 
    {
        SeedUserProfile casted = obj as SeedUserProfile;
        if (obj == null || casted == null) return false;
        return casted.SteamID.m_SteamID == this.SteamID.m_SteamID;
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
    public System.Action<SeedUserProfile> SelfUserInfoLoaded;

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

            LocalUserProfile = FetchSteamUserInfo(UserSteamID);
        }
    }

    public SeedUserProfile FetchSteamUserInfo(CSteamID steamID)
    {
        SeedUserProfile profile = TryAddProfile(steamID.m_SteamID);

        if (!SteamFriends.RequestUserInformation(steamID, false)) // Goes to a PersonaStateChange_t callback if not loaded
        {
            // loaded already.
            HandleSteamUserLoaded(steamID.m_SteamID);
        }

        return profile;
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
                LocalUserProfile.SetAvatarSprite(result);
            }

            //UserAvatarLoaded?.Invoke(profile);

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

    public SeedUserProfile TryAddProfile(ulong steamID)
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
        profile.LoadSteamData();
        profile.SetAvatarSprite(TryConvertHandleToSprite(profile.UserAvatarHandle));

        if (profile.SteamID == UserSteamID)
        {
            SelfUserInfoLoaded?.Invoke(profile);
        }

        SteamUserInfoLoaded?.Invoke(profile.SteamID);
    }

}
