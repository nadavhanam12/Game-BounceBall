using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class LoadingPage : MonoBehaviourPunCallbacks
{
    [SerializeField] MultiplayerLobby m_multiplayerLobby;
    [SerializeField] MainMenu m_mainMenu;


    public void Activate()
    {
        gameObject.SetActive(true);

        PhotonNetwork.NickName = PlayerPrefs.GetString("PlayerName");
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        //Debug.Log("Connection made to " + PhotonNetwork.CloudRegion + " server.");
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        //print("OnJoinedLobby");
        gameObject.SetActive(false);
        m_multiplayerLobby.Activate();
    }

    public override void OnCustomAuthenticationFailed(string debugMessage)
    {
        Debug.Log("OnCustomAuthenticationFailed " + debugMessage);
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("OnCustomAuthenticationFailed " + cause.ToString());
    }

    public void OnClickedBack()
    {
        PhotonNetwork.Disconnect();
        gameObject.SetActive(false);
        m_mainMenu.OpenMenuGameOptions();
    }

}
