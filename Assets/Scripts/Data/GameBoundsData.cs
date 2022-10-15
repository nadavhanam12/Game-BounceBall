using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoundsData : MonoBehaviour
{

    public GameObject m_gameUpperBound;
    public GameObject m_gameLowerBound;
    public GameObject m_gameLeftBound;
    public GameObject m_gameRightBound;
    public GameObject m_gamePlayGroundLowerBound;

    public GameBounds GenerateBounds(Camera cam)
    {
        GameBounds bounds = new GameBounds();
        bounds.GameUpperBound = cam.ScreenToWorldPoint(this.m_gameUpperBound.transform.position).y;
        bounds.GameLowerBound = cam.ScreenToWorldPoint(this.m_gameLowerBound.transform.position).y;
        bounds.GamePlayGroundLowerBound = cam.ScreenToWorldPoint(this.m_gamePlayGroundLowerBound.transform.position).y;
        bounds.GameLeftBound = cam.ScreenToWorldPoint(this.m_gameLeftBound.transform.position).x;
        bounds.GameRightBound = cam.ScreenToWorldPoint(this.m_gameRightBound.transform.position).x;

        return bounds;
    }
    public void ChangeBoundsPvP(GameBounds m_gameBounds)
    {
        Vector3 pos;
        pos = m_gameUpperBound.transform.position;
        pos.y = m_gameBounds.GameUpperBound;
        m_gameUpperBound.transform.position = pos;

        pos = m_gameLowerBound.transform.position;
        pos.y = m_gameBounds.GameLowerBound;
        m_gameLowerBound.transform.position = pos;

        pos = m_gameLeftBound.transform.position;
        pos.x = m_gameBounds.GameLeftBound;
        m_gameLeftBound.transform.position = pos;

        pos = m_gameRightBound.transform.position;
        pos.x = m_gameBounds.GameRightBound;
        m_gameRightBound.transform.position = pos;

        pos = m_gamePlayGroundLowerBound.transform.position;
        pos.y = m_gameBounds.GamePlayGroundLowerBound;
        m_gamePlayGroundLowerBound.transform.position = pos;
    }

}
