using Fusion;
using UnityEngine;

public class DungeonMatch : NetworkBehaviour
{
    public static DungeonMatch Instance { get; private set; }

    [Networked]
    public NetworkBool IsOver { get; set; }

    [Networked]
    public PlayerRef Loser { get; set; }

    [Networked]
    public PlayerRef BluePlayer { get; set; }

    public bool CanPlay
    {
        get
        {
            if (IsOver) return false;

            int connectedPlayer = 0;

            foreach (PlayerRef player in Runner.ActivePlayers)
            {
                connectedPlayer++;
            }

            return connectedPlayer == 2;
        }
    }

    public override void Spawned()
    {
        Instance = this;

        if (HasStateAuthority)
        {
            IsOver = false;
            BluePlayer = Runner.LocalPlayer;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ReportVictoriousPlayer(PlayerRef victorious, RpcInfo info = default)
    {
        if (!HasStateAuthority || IsOver) return;

        if (!CanPlay) return;

        if (info.Source != victorious) return;

        foreach (PlayerRef player in Runner.ActivePlayers)
        {
            if (player == victorious) continue;

            Loser = player;
            IsOver = true;

            Debug.Log("Partida encerrada!!!");
            break;
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (Instance != this) Instance = null;
    }
}
