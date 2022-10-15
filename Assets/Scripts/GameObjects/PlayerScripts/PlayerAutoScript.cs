using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAutoScript : PlayerScript
{
    bool m_enableMovement = true;
    protected override void Update()
    {
        if (!isGamePaused)
        {
            if (!m_playerMovement.InParalyze)
            {
                m_playerMovement.GetJump();
                AutoPlayGeneral();
            }
        }

    }

    void AutoPlayGeneral()
    {
        if (m_currentlyInTurn)
        {
            AutoPlayKick();
            AutoPlayMovement();
        }
        else
        {
            AutoBehaviorNotOnTurn();
        }
    }

    void AutoPlayMovement()
    {
        if (!m_enableMovement) return;
        if (m_playerMovement.IsJumping) return;
        int rnd = UnityEngine.Random.Range(0, 100);
        Vector3 ballTransform = m_args.BallsManager.GetCorrectBallPosition();
        Vector3 playerTransform = gameObject.transform.position;
        float deltaX = ballTransform.x - playerTransform.x;
        float deltaY = ballTransform.y - playerTransform.y;

        if (rnd <= m_args.playerStats.m_autoPlayDifficult)
        {
            if (Mathf.Abs(deltaX) > m_args.playerStats.AutoPlayBallDistance)
            {
                if (deltaX > 0)
                    m_playerMovement.OnMoveX(Vector2Int.right);
                else
                    m_playerMovement.OnMoveX(Vector2Int.left);
            }
        }
        else
        {
            if (Random.Range(0, 1f) <= 0.25)
            {
                /*if (deltaX > 0)
                    m_playerMovement.OnMoveX(Vector3.left);
                else
                    m_playerMovement.OnMoveX(Vector3.right);*/
                base.OnJump();
            }
            else
            {
                StartCoroutine("MovementCorutine");
            }

        }
    }
    void AutoBehaviorNotOnTurn()
    {
        if (!m_enableMovement) return;
        if (m_playerMovement.IsJumping) return;
        float rnd = Random.Range(0, 1f);
        if (rnd <= 0.01)
        {
            base.OnJump();
        }
        else if (rnd <= 0.5)
        {
            StartCoroutine("MovementCorutine");
        }
        else if (rnd <= 0.75)
        {
            m_playerMovement.OnMoveX(Vector2Int.left);
        }
        else
        {
            m_playerMovement.OnMoveX(Vector2Int.right);
        }
    }

    void AutoPlayKick()
    {
        if (!m_playerKicksManager.InKickCooldown)
        {
            /*int rnd = UnityEngine.Random.Range(0, 100);
            if (rnd <= m_args.playerStats.m_autoPlayDifficult)
            {*/
            List<BallScript> ballsHit = m_playerKicksManager.CheckBallInHitZone();
            if (ballsHit.Count > 0)
                OnKickPlay(ballsHit);
            //}
        }

    }

    public override void StartTurn(bool throwNewBall = true)
    {
        StartCoroutine("MovementCorutine");
        base.StartTurn(throwNewBall);
        m_playerKicksManager.StartKickCoolDown();
    }

    IEnumerator MovementCorutine()
    {
        //print("MovementCorutine");
        base.OnPlayIdle();
        m_enableMovement = false;
        yield return new WaitForSeconds(m_args.playerStats.AutoPlayMovementDelay);
        m_enableMovement = true;
    }

    public override void SendKickEventData(KickType kickType)
    {
        //no need to send data on Auto Play player
    }

}
