using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerLobby : MonoBehaviourPunCallbacks
{
    [SerializeField] MultiPlayerRoom m_multiPlayerRoom;
    [SerializeField] MainMenu m_mainMenu;
    [SerializeField] TMP_InputField m_newRoomName;

    [SerializeField] AvailableMatch availableMatchPrefab;
    List<AvailableMatch> availableMatchList = new List<AvailableMatch>();
    [SerializeField] Transform contentTransform;
    float m_timeBeforeUpdates = 1.5f;
    float m_nextUpdateTime;

    public void Activate()
    {
        gameObject.SetActive(true);
        foreach (AvailableMatch availableMatch in availableMatchList)
            Destroy(availableMatch.gameObject);
        availableMatchList.Clear();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public void OnClickCreate()
    {
        if (m_newRoomName.text.Length > 0)
        {
            if (!PhotonNetwork.IsConnectedAndReady)
            {
                print("OnClickCreate- Not IsConnectedAndReady");
                return;
            }

            PhotonNetwork.CreateRoom(m_newRoomName.text, new RoomOptions()
            { MaxPlayers = 2, PlayerTtl = 0, EmptyRoomTtl = 0, BroadcastPropsChangeToAll = true });
        }
    }


    public void OnRoomChoosen(string roomName)
    {
        if (PhotonNetwork.IsConnectedAndReady)
            PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinedRoom()
    {
        gameObject.SetActive(false);
        m_multiPlayerRoom.Activate(PhotonNetwork.CurrentRoom.Name);
    }


    public override void OnCreateRoomFailed(short returnCode, string message)
    {//A game with the specified id already exist. is the message if already exist
        Debug.Log(message);
    }


    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (Time.time >= m_nextUpdateTime)
        {
            UpdateRoomList(roomList);
            m_nextUpdateTime = Time.time + m_timeBeforeUpdates;
        }
    }

    void UpdateRoomList(List<RoomInfo> roomList)
    {
        foreach (AvailableMatch availableMatch in availableMatchList)
            Destroy(availableMatch.gameObject);
        availableMatchList.Clear();

        foreach (RoomInfo roomInfo in roomList)
        {
            if (roomInfo.PlayerCount == 0 || !roomInfo.IsOpen || !roomInfo.IsVisible || roomInfo.RemovedFromList)
            {
                Debug.Log(string.Format("Blocked Room: Name: {0}, PlayersCount: {1}, IsOpen: {2}, IsVisible: {3}",
                                                        roomInfo.Name, roomInfo.PlayerCount, roomInfo.IsOpen, roomInfo.IsVisible));
                return;
            }

            AvailableMatch availableMatch = Instantiate(availableMatchPrefab, contentTransform);
            availableMatch.SetAvailableMatch(this, roomInfo.Name);
            availableMatchList.Add(availableMatch);
        }

    }

    public void OnClickedBack()
    {
        PhotonNetwork.Disconnect();
        gameObject.SetActive(false);
        m_mainMenu.OpenMenuGameOptions();
    }
}
