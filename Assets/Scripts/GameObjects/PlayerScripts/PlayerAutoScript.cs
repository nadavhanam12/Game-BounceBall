using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAutoScript : PlayerScript
{

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
            //shit not on turn
        }
    }

    void AutoPlayMovement()
    {
        Vector3 ballTransform = m_args.BallsManager.GetCorrectBallPosition();
        Vector3 playerTransform = gameObject.transform.position;
        float deltaX = ballTransform.x - playerTransform.x;
        if (Mathf.Abs(deltaX) > m_args.playerStats.AutoPlayBallDistance)
        {
            if (deltaX > 0)
                m_playerMovement.OnMoveX(Vector3.right);
            else
                m_playerMovement.OnMoveX(Vector3.left);
        }
    }

    void AutoPlayKick()
    {
        if (!m_playerKicksManager.InKickCooldown)
        {
            int rnd = UnityEngine.Random.Range(0, 100);
            if (rnd <= m_args.playerStats.m_autoPlayDifficult)
            {
                List<BallScript> ballsHit = m_playerKicksManager.CheckBallInHitZone();
                if (ballsHit.Count > 0)
                {
                    OnKickPlay(KickType.Regular);
                }
            }
        }

    }

    public override void StartTurn(bool throwNewBall = true)
    {
        base.StartTurn(throwNewBall);
        m_playerKicksManager.StartKickCoolDown();
    }

    protected override void SendKickEventData(KickType kickType)
    {
        //no need to send data on Auto Play player
    }

}
