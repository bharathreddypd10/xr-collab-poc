using UnityEngine;
using TMPro;

public class JoinRoomUIManager : MonoBehaviour
{
    public GameObject joinPanel;
    public GameObject networkRunnerManager;
    public TMP_InputField roomCodeInput;
    public TMP_Text statusText;

    public void JoinRoom()
    {
        string roomCode = roomCodeInput.text;

        if (string.IsNullOrWhiteSpace(roomCode))
        {
            statusText.text = "Enter room code";
            return;
        }

        PlayerPrefs.SetString("RoomCode", roomCode);

        joinPanel.SetActive(false);
        networkRunnerManager.SetActive(true);

        statusText.text = "Joining room...";
    }
}
