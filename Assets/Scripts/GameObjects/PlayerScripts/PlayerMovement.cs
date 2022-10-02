using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    bool isGamePaused;
    public bool InParalyze { get; private set; }
    PlayerScript m_playerScript;
    PlayerArgs m_args;
    public bool IsJumping { get; private set; } = false;
    public bool IsJumpingUp { get; private set; } = false;
    public bool IsJumpingDown { get; private set; } = false;
    private Quaternion m_initialRotation;
    private Vector3 m_initialPosition;
    private Vector3 m_initialScale;

    float m_minimumDistanceToMove = 1f;

    public void Init(PlayerScript playerScript, PlayerArgs args)
    {
        m_playerScript = playerScript;
        InParalyze = false;
        m_args = args;

        m_initialRotation = gameObject.transform.rotation;
        m_initialPosition = gameObject.transform.position;
        m_initialScale = gameObject.transform.localScale;
    }

    public void InitPlayer()
    {
        if (!this) return;
        gameObject.transform.rotation = m_initialRotation;
        Vector3 positionUpper = m_initialPosition;
        gameObject.transform.position = positionUpper;
        gameObject.transform.localScale = m_initialScale;

        SetInParalyze(false);
        SetIsJumping(false);
    }
    public void SetInParalyze(bool isParalyze)
    {
        InParalyze = isParalyze;
    }

    public void OnMoveX(Vector2 direction, bool withAnimation = true)
    {
        if (isGamePaused)
            return;
        if (IsJumping)
            return;
        if (CheckPlayerInBounds(direction))
        {
            if (withAnimation)
                m_playerScript.PlayRunAnim();

            SpinPlayerToDirection(direction);
            transform.Translate(direction * m_args.playerStats.m_movingSpeed, Space.World);
        }

    }

    void SpinPlayerToDirection(Vector3 direction)
    {
        if ((direction == Vector3.left) && (transform.localScale.x > 0))
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;

        }
        else if ((direction == Vector3.right) && (transform.localScale.x < 0))
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }

    public void SetGamePause(bool isPause)
    {
        isGamePaused = isPause;
    }

    //for touch control
    public void MovePlayerToPosition(Vector2 position)
    {
        if (InParalyze)
            return;
        Vector2 playerPosition = transform.position;
        //print(playerPosition + "   " + position);
        if ((Math.Abs(playerPosition.x - position.x) > m_minimumDistanceToMove))
            if (playerPosition.x < position.x)
                OnMoveX(Vector2.right);
            else
                OnMoveX(Vector2.left);
        else
            m_playerScript.OnPlayIdle();
    }

    //for keyboard control
    public void MoveX(Vector2 dir)
    {
        if (InParalyze)
            return;
        OnMoveX(dir);
    }

    private bool CheckPlayerInBounds(Vector3 direction)
    {
        Vector3 playerPos = transform.position;
        if (direction.x <= 0)
        {//he is moving left
            if (playerPos.x - m_args.playerStats.PlayerBoundDistanceTrigger < m_args.Bounds.GameLeftBound)
                return false;
        }
        else //he is moving right
            if (playerPos.x + m_args.playerStats.PlayerBoundDistanceTrigger > m_args.Bounds.GameRightBound)
            return false;
        return true;
    }

    ///Jumping
    public void GetJump()
    {

        if (IsJumping)
        {
            if (IsJumpingUp)
            {
                if (transform.position.y < m_args.playerStats.m_maxHeight)
                    transform.Translate(Vector3.up * m_args.playerStats.m_jumpSpeed, Space.World);
                else
                {
                    IsJumpingUp = false;
                    IsJumpingDown = true;
                }
            }
            else if (IsJumpingDown)
            {
                if (transform.position.y > m_initialPosition.y)
                {
                    transform.Translate(Vector3.down * m_args.playerStats.m_jumpSpeed, Space.World);
                    if (transform.position.y < m_initialPosition.y)
                    {
                        Vector3 curPos = transform.position;
                        curPos.y = m_initialPosition.y;
                        transform.position = curPos;
                    }
                }
                else
                    IsJumpingDown = false;
            }
            else
                IsJumping = false;
        }
    }

    internal void SetIsJumping(bool v)
    {
        IsJumping = v;
    }

    internal void SetIsJumpingUp(bool v)
    {
        IsJumpingUp = v;
    }
}
