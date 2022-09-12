using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Bomb;

public class PlayerBomb : MonoBehaviour
{
    private Bomb m_curBomb;

    PlayerArgs m_args;

    public void Init(PlayerArgs args)
    {
        m_args = args;


    }


    public void OnTriggerEnter2D(Collider2D collider)
    {
        //print("OnTriggerEnter2D");
        /*BallScript ball = collider.GetComponent<BallScript>();
        if (ball)
            print("OnTriggerEnter2D " + ball.GetIndex());*/
        Bomb curCollider = collider.GetComponent<Bomb>();
        if (curCollider != null)
        {
            Status bombStatus = curCollider.GetStatus();
            if ((bombStatus == Status.FreeInScene) || (bombStatus == Status.ReachedLowerBound))
            {
                if (m_curBomb == null)
                {
                    m_curBomb = curCollider;
                    //m_curBomb.gameObject.transform.parent = m_pickableSpot.transform;
                    m_curBomb.PickUp();
                    if (m_args.ToggleBombUI != null)
                        m_args.ToggleBombUI(true);
                }

            }
            /* else if (bombStatus == Status.Activated)
             {
                 curCollider.Explode();
                 PlayerHitByBomb();
             }*/


        }
    }

    public void ActivateBomb()
    {
        if (m_curBomb != null)
        {
            AnalyticsManager.Instance().CommitData(
            AnalyticsManager.AnalyticsEvents.Event_Bomb_Throw,
            new Dictionary<string, object> {
                            { "PlayerIndex", m_args.PlayerIndex }});

            bool throwRight = transform.localScale.x > 0;
            m_curBomb.SetActivateDirections(throwRight);
            m_curBomb.Activate();
            m_curBomb = null;
            if (m_args.ToggleBombUI != null)
                m_args.ToggleBombUI(false);
        }
    }

    internal bool HasBomb()
    {
        return m_curBomb != null;
    }
}
