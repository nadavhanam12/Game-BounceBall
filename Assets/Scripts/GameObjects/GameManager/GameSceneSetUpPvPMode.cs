using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameSceneSetUpPvPMode : MonoBehaviour, IOnEventCallback
{
    List<Player> m_multiplayerReadyPlayers;
    GameManagerPvPMode m_gameManagerPvPMode;
    MainMenu m_mainMenu;

    public void Init(GameManagerPvPMode gameManagerPvPMode, MainMenu mainMenu)
    {
        m_gameManagerPvPMode = gameManagerPvPMode;
        m_mainMenu = mainMenu;
        m_multiplayerReadyPlayers = new List<Player>();
    }

    public bool IsConnected()
    {
        return PhotonNetwork.IsConnectedAndReady;
    }


    public async void ReadyToStartGameScene()
    {
        //Debug.Log("ReadyToStartGameScene");
        PhotonNetwork.IsMessageQueueRunning = true;
        await Task.Delay(1000);
        m_mainMenu?.UnloadMenu();
        PvPEventsService.Instance.SendEventToMaster
            (PvPEventsService.Event_PlayerReady, PhotonNetwork.LocalPlayer);
    }

    public void OnEvent(EventData photonEvent)
    {
        //print("OnEvent : " + photonEvent.Code);
        byte eventCode = photonEvent.Code;
        if (eventCode == PvPEventsService.Event_PlayerReady)
            HandlePlayerReady(photonEvent);
        if (eventCode == PvPEventsService.Event_AllPlayersReady)
            HandleAllPlayersReady();
    }

    void HandlePlayerReady(EventData photonEvent)
    {
        //print("Handle Player Ready");
        if (!PhotonNetwork.IsMasterClient)
            return;
        m_multiplayerReadyPlayers.Add((Player)photonEvent.CustomData);
        if (m_multiplayerReadyPlayers.Count == PhotonNetwork.PlayerList.Length)
        {
            //All players are ready
            PvPEventsService.Instance.SendEventToAll
            (PvPEventsService.Event_AllPlayersReady, PhotonNetwork.LocalPlayer);

        }
    }

    void HandleAllPlayersReady()
    {
        //print("Handle All Players Ready");
        m_gameManagerPvPMode.StartGameScene();
    }

    void OnEnable() { PhotonNetwork.AddCallbackTarget(this); }
    void OnDisable() { PhotonNetwork.RemoveCallbackTarget(this); }
}
