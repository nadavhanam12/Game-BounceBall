using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AvailableMatch : MonoBehaviour
{
    [SerializeField] TMP_Text m_matchNameText;
    string m_matchName;
    MultiplayerLobby m_multiplayerLobby;

    public void SetAvailableMatch(MultiplayerLobby multiplayerLobby, string matchName)
    {
        m_multiplayerLobby = multiplayerLobby;
        m_matchName = matchName;
        m_matchNameText.text = m_matchName;
    }
    public void OnClicked()
    {
        m_multiplayerLobby.OnRoomChoosen(m_matchName);
    }
}
