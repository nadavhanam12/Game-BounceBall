using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}
