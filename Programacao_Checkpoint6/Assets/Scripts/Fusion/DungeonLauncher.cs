using Fusion;
using TMPro;
using UnityEngine;

public class DungeonLauncher : MonoBehaviour
{
    [SerializeField] private NetworkRunner runner;
    [SerializeField] private TMP_InputField roomNameInput;
    [SerializeField] private TMP_Text textConnectionStatus;
    [SerializeField] private GameObject menuPanel;

    private bool connecting;

    public async void ConnectToRoom()
    {
        if (connecting || runner.IsRunning) return;

        string roomName = roomNameInput.text;

        if (string.IsNullOrEmpty(roomName))
        {
            textConnectionStatus.text = "Digite o nome da sala";
            return;
        }
        
        connecting = true;
        textConnectionStatus.text = "Conectando...";

        runner.ProvideInput = false;

        var sceneManager = GetComponent<NetworkSceneManagerDefault>();
        
        var result = await runner.StartGame(
            new StartGameArgs
            {
                GameMode = GameMode.Shared,
                SessionName = roomName,
                PlayerCount = 2,
                SceneManager = sceneManager
            }
        );
        
        if (result.Ok)
        {
            menuPanel.SetActive(false);
        }
        else
        {
            textConnectionStatus.text = "Erro: " + result.ShutdownReason;
            Debug.LogError(result.ShutdownReason);
        }
        
        connecting = false;
    }

}
