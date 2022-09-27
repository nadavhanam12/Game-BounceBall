using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Threading.Tasks;

public class LoadingPage : MonoBehaviourPunCallbacks
{
    [SerializeField] MultiplayerLobby m_multiplayerLobby;
    [SerializeField] MainMenu m_mainMenu;


    public void Activate()
    {
        Debug.Log("Activate");
        gameObject.SetActive(true);
        if (PhotonNetwork.IsConnected)
        {
            SwitchToLobby();
            return;
        }
        PhotonNetwork.NickName = PlayerPrefs.GetString("PlayerName");
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("OnCustomAuthenticationFailed " + cause.ToString());
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connection made to " + PhotonNetwork.CloudRegion + " server.");
        if (PhotonNetwork.NetworkClientState == ClientState.Joining)
        {
            print("ClientState.Joining");
            SwitchToLobby();
        }
        else
            PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        print("OnJoinedLobby");
        SwitchToLobby();
    }

    void SwitchToLobby()
    {
        gameObject.SetActive(false);
        m_multiplayerLobby.Activate();
    }

    public override void OnCustomAuthenticationFailed(string debugMessage)
    {
        Debug.Log("OnCustomAuthenticationFailed " + debugMessage);
    }


    public void OnClickedBack()
    {
        PhotonNetwork.Disconnect();
        gameObject.SetActive(false);
        m_mainMenu.OpenMenuGameOptions();
    }

}
