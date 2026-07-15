using Photon.Voice.Fusion;
using UnityEngine;
using UnityEngine.UI;

public class MicMuteButton : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Image icon;
    [SerializeField] private Sprite micOnSprite;
    [SerializeField] private Sprite micOffSprite;

    private void OnEnable()
    {
        NetworkAvatar.LocalAvatarSpawned += HandleLocalAvatarSpawned;
        NetworkAvatar.LocalAvatarDespawned += HandleLocalAvatarDespawned;

        // Covers the case where this button is enabled after the avatar already spawned.
        if (NetworkAvatar.Local != null)
            HandleLocalAvatarSpawned(NetworkAvatar.Local);
        else
            SetPanelVisible(false);
    }

    private void OnDisable()
    {
        NetworkAvatar.LocalAvatarSpawned -= HandleLocalAvatarSpawned;
        NetworkAvatar.LocalAvatarDespawned -= HandleLocalAvatarDespawned;
    }

    private void HandleLocalAvatarSpawned(NetworkAvatar avatar)
    {
        SetPanelVisible(true);
        RefreshIcon();
    }

    private void HandleLocalAvatarDespawned()
    {
        SetPanelVisible(false);
    }

    private void SetPanelVisible(bool visible)
    {
        if (panelRoot != null)
            panelRoot.SetActive(visible);
    }

    public void ToggleMic()
    {
        var recorder = GetLocalRecorder();
        if (recorder == null)
            return;

        recorder.TransmitEnabled = !recorder.TransmitEnabled;
        RefreshIcon();
    }

    private void RefreshIcon()
    {
        if (icon == null)
            return;

        var recorder = GetLocalRecorder();
        bool muted = recorder == null || !recorder.TransmitEnabled;
        icon.sprite = muted ? micOffSprite : micOnSprite;
    }

    private Photon.Voice.Unity.Recorder GetLocalRecorder()
    {
        NetworkAvatar local = NetworkAvatar.Local;
        if (local == null)
            return null;

        VoiceNetworkObject voiceObject = local.GetComponent<VoiceNetworkObject>();
        return voiceObject == null ? null : voiceObject.RecorderInUse;
    }
}
