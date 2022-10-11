using Photon.Pun;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using Photon.Realtime;

public class MultiPlayerRoom : MonoBehaviourPunCallbacks
{
    [SerializeField] MainMenu m_mainMenu;
    [SerializeField] MultiplayerLobby m_multiplayerLobby;
    [SerializeField] TMP_Text m_roomNameText;

    [SerializeField] RoomPlayer roomPlayerPrefab;
    List<RoomPlayer> roomPlayerList = new List<RoomPlayer>();
    [SerializeField] Transform contentTransform;
    bool m_withLobby;
    bool loadingScene = false;
    bool m_assignAutoPlayer;
    public void Activate(string roomName, bool assignAutoPlayer, bool withLobby = true)
    {
        //print("MultiPlayerRoom Activating assignAutoPlayer: " + assignAutoPlayer);
        gameObject.SetActive(true);
        m_withLobby = withLobby;
        RoomPlayer[] roomPlayers = GetComponentsInChildren<RoomPlayer>();
        foreach (RoomPlayer child in roomPlayers)
            GameObject.Destroy(child.gameObject);
        //m_roomNameText.text = "Room Name: " + roomName;
        m_assignAutoPlayer = assignAutoPlayer;
        UpdatePlayerList();
        loadingScene = false;
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList();
    }

    public override void OnPlayerLeftRoom(Player newPlayer)
    {
        //UpdatePlayerList();
        OnClickedBack();
    }
    public void OnClickedBack()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount <= 1)
        {
            print("Remove Room: " + PhotonNetwork.CurrentRoom.Name);
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
        }
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        gameObject.SetActive(false);
        if (m_withLobby)
            m_multiplayerLobby.Activate();
        else
            m_mainMenu.OpenMenuGameOptions();
    }


    void UpdatePlayerList()
    {
        foreach (RoomPlayer roomPlayer in roomPlayerList)
            Destroy(roomPlayer.gameObject);
        roomPlayerList.Clear();

        if (PhotonNetwork.CurrentRoom == null)
            return;

        Player playerMaster = null, playerClient = null;

        foreach (KeyValuePair<int, Player> player in PhotonNetwork.CurrentRoom.Players)
            if (player.Value.IsMasterClient)
                playerMaster = player.Value;
            else
                playerClient = player.Value;

        if (playerMaster != null)
        {
            RoomPlayer roomPlayer = Instantiate(roomPlayerPrefab, contentTransform.transform.GetChild(0));
            roomPlayer.SetRoomPlayer(this, playerMaster);
            roomPlayerList.Add(roomPlayer);
        }

        if (playerClient != null)
        {
            RoomPlayer roomPlayer = Instantiate(roomPlayerPrefab, contentTransform.transform.GetChild(1));
            roomPlayer.SetRoomPlayer(this, playerClient);
            roomPlayerList.Add(roomPlayer);
        }
        else if (m_assignAutoPlayer)
        {
            RoomPlayer roomPlayer = Instantiate(roomPlayerPrefab, contentTransform.transform.GetChild(1));
            roomPlayer.SetRoomPlayer(this, null);
            roomPlayerList.Add(roomPlayer);
        }


    }
    public override void OnPlayerPropertiesUpdate(Player player, ExitGames.Client.Photon.Hashtable hashTable)
    {
        if (loadingScene)
            return;
        if (PhotonNetwork.IsMasterClient)
        {
            if (m_assignAutoPlayer)
            {
                Invoke("LoadScene", 2f);
                return;
            }
            foreach (RoomPlayer GetPlayer in roomPlayerList)
            {
                if (GetPlayer?.GetPlayer()?.CustomProperties["IsReady"] != null && !(bool)GetPlayer?.GetPlayer()?.CustomProperties["IsReady"])
                    return;
            }
            if (PhotonNetwork.CurrentRoom.PlayerCount != 2)
                return;
            Invoke("LoadScene", 2f);
        }
    }

    void LoadScene()
    {
        if (loadingScene)
            return;

        loadingScene = true;
        if (m_assignAutoPlayer)
            m_mainMenu.StartPvE();
        else
            m_mainMenu.StartPvP();
    }
}
