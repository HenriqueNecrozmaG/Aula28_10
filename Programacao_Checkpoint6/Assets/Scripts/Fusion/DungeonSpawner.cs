using Fusion;
using UnityEngine;

public class DungeonSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] private NetworkObject playerPrefab;
    [SerializeField] private NetworkObject matchPrefab;

    public void PlayerJoined(PlayerRef player)
    {
        if (player != Runner.LocalPlayer) return;

        bool isMaster = Runner.IsSharedModeMasterClient;

        if (isMaster)
        {
            Runner.Spawn(
                matchPrefab,
                Vector3.zero,
                Quaternion.identity
            );
        }

        Vector3 spawnPostion = isMaster ? new Vector3(-27f, 9.5f, 0f) : new Vector3(-29f, 9.5f, 0f);

        Quaternion spawnRotation = isMaster ? Quaternion.Euler(0, 90, 0) : Quaternion.Euler(0, 90, 0);

        NetworkObject playerObject = Runner.Spawn(
            playerPrefab,
            spawnPostion,
            spawnRotation
        );

        Runner.SetPlayerObject(player, playerObject);
    }
}
