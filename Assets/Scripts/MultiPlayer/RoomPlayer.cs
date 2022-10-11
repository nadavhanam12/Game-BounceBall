using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using System;

public class RoomPlayer : MonoBehaviourPunCallbacks
{

    [SerializeField] TMP_Text m_nameText;
    [SerializeField] Image m_playerImage;
    [SerializeField] List<Sprite> m_charactersImages;
    [SerializeField] List<string> m_charactersIdles;
    Player m_player;
    MultiPlayerRoom m_multiPlayerRoom;
    ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable();

    int m_selectedCharacterIndex;
    string[] m_randomNamesArray =
    {"Nadav66","Tal12","Maria99","Omer","Yafit33","Alon44","Itay15","Yali","Guy31","Naor","Ariel87","Yuval","Dor55","Tomer09","Kimchi88"
    ,"Taichi77","Eran","Amit","Ohad","Omri","Larisa33","Andrew12","Rotem32" };

    public void SetRoomPlayer(MultiPlayerRoom multiPlayerRoom, Player player)
    {
        m_multiPlayerRoom = multiPlayerRoom;
        m_player = player;

        if (m_player != null)
            m_nameText.text = m_player.NickName;
        else
        {
            m_nameText.text = GenerateRandomName();
        }

        if (m_player != null && m_player.IsMasterClient)
            m_selectedCharacterIndex = 0;
        else
            m_selectedCharacterIndex = 1;

        //m_playerImage.sprite = m_charactersImages[m_selectedCharacterIndex];
        if (m_selectedCharacterIndex == 1)
        {
            m_playerImage.rectTransform.localScale = new Vector3(-0.8f, 1.1f, 1);
            m_playerImage.rectTransform.anchoredPosition = new Vector2(0, -300);
        }

        GetComponent<Animator>().Play(m_charactersIdles[m_selectedCharacterIndex]);
        playerProperties["IsReady"] = false;
        PhotonNetwork.SetPlayerCustomProperties(playerProperties);
        Invoke("OnClickedReady", 1.5f);
    }

    private string GenerateRandomName()
    {
        int rnd = UnityEngine.Random.Range(0, m_randomNamesArray.Length);
        return m_randomNamesArray[rnd];
    }

    public void OnClickedReady()
    {
        if (PhotonNetwork.LocalPlayer == m_player)
        {
            playerProperties["IsReady"] = true;
            PhotonNetwork.SetPlayerCustomProperties(playerProperties);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player player, ExitGames.Client.Photon.Hashtable hashTable)
    {
        if (player == m_player)
        {
            if ((bool)player.CustomProperties["IsReady"])
                //apply ready VFX
                Debug.Log("Is Ready");
        }
    }


    public Player GetPlayer() { return m_player; }
}
