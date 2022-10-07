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
    public void Activate(string roomName, bool withLobby = true)
    {
        gameObject.SetActive(true);
        m_withLobby = withLobby;
        RoomPlayer[] roomPlayers = GetComponentsInChildren<RoomPlayer>();
        foreach (RoomPlayer child in roomPlayers)
            GameObject.Destroy(child.gameObject);

        //m_roomNameText.text = "Room Name: " + roomName;
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


    }
    public override void OnPlayerPropertiesUpdate(Player player, ExitGames.Client.Photon.Hashtable hashTable)
    {
        if (loadingScene)
            return;
        if (PhotonNetwork.IsMasterClient)
        {
            foreach (RoomPlayer GetPlayer in roomPlayerList)
                if (!(bool)GetPlayer?.GetPlayer()?.CustomProperties["IsReady"])
                    return;
            if (PhotonNetwork.CurrentRoom.PlayerCount != 2)
                return;

            Invoke("LoadScene", 3f);
        }

    }

    void LoadScene()
    {
        if (loadingScene)
            return;
        loadingScene = true;
        m_mainMenu.StartPvP();
    }
}
