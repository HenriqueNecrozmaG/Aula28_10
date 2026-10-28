using Fusion;
using System.Collections;
using TMPro;
using UnityEngine;

public class DungeonHud : MonoBehaviour
{
    [SerializeField] private NetworkRunner runner;

    [Header("HUD")]
    [SerializeField] private TMP_Text textTime;
    [SerializeField] private TMP_Text matchStatusText;

    [Header("Resultado")]
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private TMP_Text resultsText;

    private int time;
    private bool hasMatchBegun;

    void Start()
    {
        resultsPanel.SetActive(false);
        matchStatusText.text = "Aguardando estado da partida...";
        hasMatchBegun = false;
    }

    void Update()
    {
        if (runner == null || !runner.IsRunning) return;

        DungeonMatch match = DungeonMatch.Instance;

        if (match == null)
        {
            matchStatusText.text = "Aguardando estado da partida...";
            return;
        }

        if (match.IsOver)
        {
            ShowResults(match);
        }
        else
        {
            resultsPanel.SetActive(false);
            matchStatusText.text = match.CanPlay ? "Jogo em andamento" : "Aguardando segundo jogador...";
        }

        if (match.CanPlay && !hasMatchBegun)
        {
            StartCoroutine(Time());
            hasMatchBegun = true;
        }
    }

    private void ShowResults(DungeonMatch match)
    {
        resultsPanel.SetActive(true);

        matchStatusText.text = "Partida encerrada";

        bool localPlayerLost = match.Loser == runner.LocalPlayer;

        string winnerName = match.Loser == match.BluePlayer ? "Vermelho" : "Azul";

        string message = localPlayerLost ? "Você perdeu" : "Você venceu";

        resultsText.text = message + "\n Vencedor: Jogador " + winnerName + "\n Tempo: " + time;
    }

    IEnumerator Time()
    {
        yield return new WaitForSeconds(1);
        time++;
        textTime.text = "Tempo: " + time;
        StartCoroutine(Time());
    }
}
