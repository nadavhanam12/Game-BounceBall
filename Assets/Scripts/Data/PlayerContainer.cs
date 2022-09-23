using Photon.Pun;
using UnityEngine;
using static GameManagerAbstract;

public class PlayerContainer : MonoBehaviour
{
    [SerializeField] Transform LeftPlayerSpawnPoint;
    [SerializeField] Transform RightPlayerSpawnPoint;
    [SerializeField] PlayerScript RedHatPlayerPrefab;
    [SerializeField] PlayerScript PumpkinHeadPlayerPrefab;

    public PlayerScript SpawnRegularPlayer()
    {
        PlayerScript newPlayer = Instantiate(RedHatPlayerPrefab, LeftPlayerSpawnPoint);
        return newPlayer;
    }

    public PlayerScript SpawnAutoPlayer()
    {
        PlayerScript newPlayer = Instantiate(PumpkinHeadPlayerPrefab, RightPlayerSpawnPoint);
        Destroy(newPlayer.GetComponent<PlayerScript>());
        newPlayer = newPlayer.gameObject.AddComponent<PlayerAutoScript>();
        return newPlayer;
    }

    public PlayerScript SpawnPlayerPvP(PlayerIndex playerIndex)
    {
        string prefabName;
        if (playerIndex == PlayerIndex.First)
            prefabName = "Player/RedHatBoy";
        else
            prefabName = "Player/PumpkinHead";
        GameObject prefabGameObject = PhotonNetwork.Instantiate(prefabName, Vector3.zero, Quaternion.identity);
        PlayerScript playerScript = prefabGameObject.GetComponent<PlayerScript>();
        return playerScript;
    }

    public void SetPlayerParent(string playerIndex, PlayerScript playerScript)
    {
        Transform playerParent;
        if (playerIndex == PlayerIndex.First.ToString())
            playerParent = LeftPlayerSpawnPoint;
        else
            playerParent = RightPlayerSpawnPoint;
        playerScript.transform.parent = playerParent;
        playerScript.transform.localPosition = Vector3.zero;
    }
}
