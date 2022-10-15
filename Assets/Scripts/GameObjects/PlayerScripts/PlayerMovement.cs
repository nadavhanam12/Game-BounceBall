using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerMovement : MonoBehaviourPun
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
        gameObject.transform.position = m_initialPosition;
        gameObject.transform.localScale = m_initialScale;

        SetInParalyze(false);
        SetIsJumping(false);
    }
    public void SetInParalyze(bool isParalyze)
    {
        InParalyze = isParalyze;
    }

    public void OnMoveX(Vector2Int direction)
    {
        if (isGamePaused)
            return;
        if (IsJumping)
            return;
        if (CheckPlayerInBounds(direction.x))
            MoveToDirection(direction);
    }
    protected virtual void MoveToDirection(Vector2Int direction)
    {
        if (CheckPlayerInBounds(direction.x))
        {
            m_playerScript.PlayRunAnim();
            SpinPlayerToDirection(direction.x);
            transform.Translate(direction.x * m_args.playerStats.m_movingSpeed, 0, 0, Space.World);
        }
    }

    void SpinPlayerToDirection(int direction)
    {
        if ((direction == -1) && (transform.localScale.x > 0))
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;

        }
        else if ((direction == 1) && (transform.localScale.x < 0))
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
                OnMoveX(Vector2Int.right);
            else
                OnMoveX(Vector2Int.left);
        else
            m_playerScript.OnPlayIdle();
    }

    //for keyboard control
    public void MoveX(Vector2Int dir)
    {
        if (InParalyze)
            return;
        OnMoveX(dir);
    }

    private bool CheckPlayerInBounds(int direction)
    {
        Vector3 playerPos = transform.position;
        if (direction <= 0)
        {//he is moving left
            if (playerPos.x - m_args.playerStats.PlayerBoundDistanceTrigger < m_args.Bounds.GameLeftBound)
                return false;
        }
        else //he is moving right
            if (playerPos.x + m_args.playerStats.PlayerBoundDistanceTrigger > m_args.Bounds.GameRightBound)
            return false;
        return true;
    }
    public void StartJump()
    {
        SetIsJumping(true);
        SetIsJumpingUp(true);
        m_playerScript.PlayJumpAnim();
    }
    ///Jumping
    public void GetJump()
    {
        if (IsJumping)
        {
            if (IsJumpingUp)
            {
                if (transform.position.y < m_args.playerStats.m_maxHeight)
                    ApplyJump(1);
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
                    ApplyJump(-1);
                }
                else
                    InitialAfterJump();
            }
            else
                InitialAfterJump();
        }
        else if (gameObject.transform.position.y != m_initialPosition.y)
            InitialAfterJump();

    }

    protected virtual void InitialAfterJump()
    {
        InitPosY();
    }

    protected virtual void ApplyJump(int movY)
    {
        transform.Translate(0, movY * m_args.playerStats.m_jumpSpeed, 0, Space.World);
    }

    internal void InitPosY()
    {
        IsJumpingDown = false;
        IsJumpingUp = false;
        IsJumping = false;
        Vector3 pos = gameObject.transform.position;
        pos.y = m_initialPosition.y;
        gameObject.transform.position = pos;
        m_playerScript.OnPlayIdle();
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
