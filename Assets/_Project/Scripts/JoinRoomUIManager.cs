using UnityEngine;
using TMPro;

public class JoinRoomUIManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField roomCodeInput;
    public TMP_Text statusText;

    public async void CreateRoom()
    {
        string roomCode = GenerateRoomCode();

        roomCodeInput.text = roomCode;

        statusText.text = "Creating Room...";

        bool success =
            await FusionManager.Instance.StartSession(roomCode);

        if (success)
        {
            statusText.text =
                $"Room Created: {roomCode}";
        }
        else
        {
            statusText.text =
                "Failed to create room";
        }
    }

    public async void JoinRoom()
    {
        string roomCode = roomCodeInput.text.Trim();

        if (string.IsNullOrEmpty(roomCode))
        {
            statusText.text =
                "Enter Room Code";
            return;
        }

        statusText.text =
            "Joining Room...";

        bool success =
            await FusionManager.Instance.StartSession(roomCode);

        if (success)
        {
            statusText.text =
                $"Joined: {roomCode}";
        }
        else
        {
            statusText.text =
                "Failed to Join";
        }
    }

    private string GenerateRoomCode()
    {
        return "XR" + Random.Range(1000, 9999);
    }
}