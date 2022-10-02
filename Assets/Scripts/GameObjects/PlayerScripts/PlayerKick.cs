using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerScript;

public class PlayerKick : MonoBehaviour
{
    private CircleCollider2D m_hitZone;
    public bool InKickCooldown { get; private set; } = false;
    PlayerScript m_player;
    PlayerArgs m_args;
    public bool IsNextKickIsSpecial { get; private set; } = false;

    public void Init(PlayerScript player, PlayerArgs args)
    {
        m_player = player;
        m_args = args;

        m_hitZone = gameObject.GetComponent<CircleCollider2D>();
        m_hitZone.radius = m_args.playerStats.m_hitZoneRadius;
        SetNextKickIsSpecial(false);
    }

    public void SetNextKickIsSpecial(bool isSpecial)
    {
        //print("SetNextKickIsSpecial " + isSpecial);
        IsNextKickIsSpecial = isSpecial;
    }

    public void StartKickCoolDown()
    {
        StartCoroutine(KickCooldown());
    }
    protected IEnumerator KickCooldown()
    {
        InKickCooldown = true;
        yield return new WaitForSeconds(m_args.playerStats.KickCooldown);
        InKickCooldown = false;
    }

    public void ReachHitPosition(KickType kickType, List<BallScript> ballsHit)
    {
        if (m_player.IsCurrentlyInTurn() && !InKickCooldown)
        {
            //print("ReachHitPosition " + m_player.GetIndex() + " " + kickType);
            if (m_args.BallsManager.ContainsCorrectBall(ballsHit))
            {
                StartKickCoolDown();
                if (kickType == KickType.Special)
                {
                    SetNextKickIsSpecial(false);
                    m_player.SendKickEventData(kickType);
                    m_player.ActivateAura();
                }
            }
            m_args.BallsManager.ApplyKick(m_args.PlayerIndex, ballsHit, kickType);
        }
    }


    public List<BallScript> CheckBallInHitZone()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(m_hitZone.bounds.center, m_hitZone.radius);
        List<BallScript> ballsHit = new List<BallScript>();
        BallScript curBallScript;
        foreach (Collider2D hitCollider in hitColliders)
        {
            //print("hitCollider: " + hitCollider.name);
            curBallScript = hitCollider.GetComponent<BallScript>();
            if (curBallScript != null && !curBallScript.BallHasFallen)
                ballsHit.Add(curBallScript);

        }
        return ballsHit;
    }





}
