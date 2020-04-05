using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using UnityEngine.UI;


// Todo: User profiles
//public class SeedUserProfile
//{
//    public string Name;

//}

public class SeedSteamManager : SteamManager
{

    public static SeedSteamManager SeedInstance { get { return Instance as SeedSteamManager; } }

    [Header("Essentials")]
    [SerializeField]
    public CSteamID UserSteamID;
    public string UserSteamName;

    [Header("Cosmetics")]
    public Sprite UserAvatar;

    int UserAvatarHandle = 0;
    bool bSelfUserDataLoaded = false;

    //[Header("Other Users Cache")]
    //Dictionary<CSteamID, SeedUserProfile> AllLoadedSteamProfiles;

    // Events
    public System.Action<Sprite> UserAvatarLoaded;
    public System.Action SteamSelfUserInfoLoaded;

    // Callbacks
    private Callback<AvatarImageLoaded_t> avatarCallback;
    private Callback<PersonaStateChange_t> selfPersonaStateChangeCallback;
    private Callback<PersonaStateChange_t> personaStateChangeCallback;

    protected override void Awake()
    {
        base.Awake();
        avatarCallback = new Callback<AvatarImageLoaded_t>(OnAvatarImageLoaded);
        selfPersonaStateChangeCallback = new Callback<PersonaStateChange_t>(OnSelfPersonaStateLoaded);
        personaStateChangeCallback = new Callback<PersonaStateChange_t>(OnNonSelfPersonaStateLoaded);
    }

    // Start is called before the first frame update
    protected void Start()
    {
        Invoke("FetchSteamUserInfo", 1.0f);
    }

    private void FetchSteamUserInfo()
    {
        if (Initialized)
        {
            UserSteamID = SteamUser.GetSteamID();

            if (bSelfUserDataLoaded)
            {
                CancelInvoke("FetchSteamUserInfo");
                return;
            }

            SteamFriends.RequestUserInformation(UserSteamID, false); // Goes to a PersonaStateChange_t callback
        }
    }

    private void OnAvatarImageLoaded(AvatarImageLoaded_t avatar)
    {
        OnAvatarImageLoaded(avatar.m_iImage);
    }

    private void OnAvatarImageLoaded(int avatarHandle)
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
            UserAvatar = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, -tex.height), new Vector2());
            UserAvatarLoaded?.Invoke(UserAvatar);
        }
    }

    private void OnSelfPersonaStateLoaded(PersonaStateChange_t persona)
    {
        if (UserAvatarHandle == 0)
        {
            UserAvatarHandle = SteamFriends.GetLargeFriendAvatar(UserSteamID);
            if (UserAvatarHandle > 0)
            {
                OnAvatarImageLoaded(UserAvatarHandle);
            }
        }
        UserSteamName = SteamFriends.GetPersonaName();
        bSelfUserDataLoaded = true;
        SteamSelfUserInfoLoaded?.Invoke();
    }

    private void OnNonSelfPersonaStateLoaded(PersonaStateChange_t persona)
    {
        // Add to AllLoadedSteamProfiles
    }

}
