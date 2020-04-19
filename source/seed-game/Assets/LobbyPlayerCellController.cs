using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerCellController : MonoBehaviour
{
    [SerializeField]
    private Image BackgroundPanel;

    [SerializeField]
    private TMPro.TMP_Text PlayerNameText;

    [SerializeField]
    private Image avatarImage;

    [SerializeField]
    private SeedUserProfile profile;

    [SerializeField]
    private Color ReadyColor = Color.green;

    [SerializeField]
    private Color NotReadyColor = Color.grey;

    public void SetPlayerName(string name)
    {
        PlayerNameText.text = name;
    }

    public void SetReadyStatus(bool isReady)
    {
        BackgroundPanel.color = isReady ? ReadyColor : NotReadyColor;
    }

    private void UpdateWithUserProfile(SeedUserProfile profile)
    {
        this.profile = profile;
        SetPlayerName(profile.Name);
        avatarImage.sprite = profile.UserAvatarSprite;
    }

    public void SetupWithUserProfile(SeedUserProfile profile)
    {
        UpdateWithUserProfile(profile);
        profile.ProfileUpdatedEvent += UpdateWithUserProfile;
    }

    private void OnDestroy()
    {
        profile.ProfileUpdatedEvent -= UpdateWithUserProfile;
    }

}
