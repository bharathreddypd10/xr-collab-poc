using UnityEngine;
using TMPro;

public class JoinRoomUIManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject roomPanel;

    public TMP_InputField roomCodeInput;

    public TMP_Text statusText;

    public GameObject createRoomButton;

    public GameObject joinRoomButton;

    private string currentRoomCode;

    private async void OnEnable()
    {
        // Connect to lobby only when the room panel becomes active
        await FusionManager.Instance.ConnectToLobby();
    }

    public async void CreateRoom()
    {
        string roomCode = GenerateRoomCode();

        currentRoomCode = roomCode;

        roomCodeInput.text = "";

        statusText.text =
            $"Creating Room : {roomCode}";

        bool success =
            await FusionManager.Instance.StartSession(roomCode);

        if (success)
{
    GUIUtility.systemCopyBuffer =
        roomCode;

    statusText.text =
        $"Room Created\n\n{roomCode}\n\nCode Copied";

    Invoke(nameof(HideUI), 1f);
}
        else
        {
            statusText.text =
                "Failed To Create Room";
        }
    }

private void HideUI()
{
    roomPanel.SetActive(false);
}
    public async void JoinRoom()
    {
        string roomCode =
            roomCodeInput.text.Trim().ToUpper();

        if (string.IsNullOrEmpty(roomCode))
        {
            statusText.text =
                "Enter Room Code";

            return;
        }

        bool roomExists =
            FusionManager.Instance.RoomExists(roomCode);

        if (!roomExists)
        {
            statusText.text =
                "No Rooms Available To Join";

            return;
        }

        statusText.text =
            $"Joining : {roomCode}";

        bool success =
            await FusionManager.Instance.StartSession(roomCode);

        if (success)
        {
            statusText.text =
                $"Joined\n\n{roomCode}";

            Invoke(nameof(HideUI), 1f);
        }
        else
        {
            statusText.text =
                "Failed To Join";
        }
    }

    private string GenerateRoomCode()
    {
        return "XR" + Random.Range(1000, 9999);
    }
}