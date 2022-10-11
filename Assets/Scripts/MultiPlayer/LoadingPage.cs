using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Threading.Tasks;
using TMPro;

public class LoadingPage : MonoBehaviourPunCallbacks
{
    [SerializeField] MultiplayerLobby m_multiplayerLobby;
    [SerializeField] MultiPlayerRoom m_multiPlayerRoom;
    [SerializeField] MainMenu m_mainMenu;
    [SerializeField] bool withLobby;
    [SerializeField] bool m_assignPvEAfterWaitTime;
    [SerializeField] int m_waitTime;
    [SerializeField] TMP_Text m_text;
    bool foundOpponent;


    public void Activate()
    {
        Debug.Log("Activate");
        gameObject.SetActive(true);
        if (PhotonNetwork.IsConnected)
        {
            OnConnectedToMaster();
            return;
        }
        PhotonNetwork.NickName = PlayerPrefs.GetString("PlayerName");
        //PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
        m_text.text = "Connecting...";
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("OnCustomAuthenticationFailed " + cause.ToString());
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connection made to " + PhotonNetwork.CloudRegion + " server.");
        if (withLobby)
        {
            if (PhotonNetwork.NetworkClientState == ClientState.Joining)
            {
                print("ClientState.Joining");
                SwitchToLobby();
            }
            else
                PhotonNetwork.JoinLobby();
        }
        else
        {
            m_text.text = "Searching For Players...";
            foundOpponent = false;
            bool succeed = PhotonNetwork.JoinRandomOrCreateRoom();
            if (!succeed)
                Debug.Log("not succeed JoinRandomOrCreateRoom");
            if (m_assignPvEAfterWaitTime)
                Invoke("AssignAutoPlayer", m_waitTime);
        }
    }

    void AssignAutoPlayer()
    {
        if (foundOpponent) return;
        Debug.Log("AssignAutoPlayer");
        foundOpponent = true;
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        SwitchToRoomScreen(true);
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("OnPlayerEnteredRoom");
        foundOpponent = true;
        SwitchToRoomScreen();
    }
    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom");
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            foundOpponent = true;
        if (PhotonNetwork.IsMasterClient) return;
        SwitchToRoomScreen();
    }

    async void SwitchToRoomScreen(bool assignAutoPlayer = false)
    {
        m_text.text = "Joining Room...";
        await Task.Delay(2000);
        gameObject.SetActive(false);
        m_multiPlayerRoom.Activate(PhotonNetwork.CurrentRoom.Name, assignAutoPlayer, withLobby);
    }

    public override void OnJoinedLobby()
    {
        print("OnJoinedLobby");
        if (withLobby)
            SwitchToLobby();
    }

    async void SwitchToLobby()
    {
        //print("SwitchToLobby 1");
        await Task.Delay(2000);
        //print("SwitchToLobby 2");
        gameObject.SetActive(false);
        m_multiplayerLobby.Activate();
    }

    public override void OnCustomAuthenticationFailed(string debugMessage)
    {
        Debug.Log("OnCustomAuthenticationFailed " + debugMessage);
        OnClickedBack();
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {//A game with the specified id already exist. is the message if already exist
        Debug.Log(message);
        OnClickedBack();
    }


    public void OnClickedBack()
    {
        PhotonNetwork.Disconnect();
        gameObject.SetActive(false);
        m_mainMenu.OpenMenuGameOptions();
    }

}
