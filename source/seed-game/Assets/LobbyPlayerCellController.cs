using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerCellController : MonoBehaviour
{
    [SerializeField]
    private TMPro.TMP_Text PlayerNameText;

    [SerializeField]
    private SeedUserProfile profile;

    public void SetPlayerName(string name)
    {
        PlayerNameText.text = name;
    }

    private void UpdateWithUserProfile(SeedUserProfile profile)
    {
        this.profile = profile;
        SetPlayerName(profile.Name);
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
