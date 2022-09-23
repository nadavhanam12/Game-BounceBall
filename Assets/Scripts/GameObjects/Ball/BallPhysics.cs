using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPhysics : MonoBehaviour
{
    private Rigidbody2D m_rigidBody;
    private CircleCollider2D m_collider;
    private Vector2 waitingForce = Vector2.zero;
    BallArgs m_args;
    BallScript m_ballScript;
    bool isGamePaused;
    public void Init(BallScript ballScript, BallArgs args)
    {
        m_args = args;
        m_ballScript = ballScript;
        m_rigidBody = GetComponent<Rigidbody2D>();
        m_collider = GetComponent<CircleCollider2D>();

        m_rigidBody.gravityScale = m_args.m_gravity;
    }

    public Vector2 GetVelocity()
    {
        return m_rigidBody.velocity;
    }

    public void ToggleSimulate(bool simulate)
    {
        m_rigidBody.simulated = simulate;
    }
    public void ApplyPhysics(Vector2 force)
    {
        ToggleSimulate(true);
        if (isGamePaused)
        {
            waitingForce = force;
        }
        else
        {
            m_rigidBody.AddForce(force);
            m_rigidBody.AddTorque(force.x * m_args.BallTorqueMultiplier * -1);
        }
    }
    public void AddForce(float ballReflectPower)
    {
        m_rigidBody.AddForce(new Vector2(0, ballReflectPower));

    }
    public void SetGamePause(bool isInitialized, bool pause)
    {
        isGamePaused = pause;
        if (isInitialized)
        {
            m_rigidBody.simulated = !isGamePaused;
            if (!isGamePaused && waitingForce != Vector2.zero)
            {
                ResetVelocity();
                ApplyPhysics(waitingForce);
                waitingForce = Vector2.zero;
            }
        }
    }
    public void ResetVelocity()
    {
        m_rigidBody.velocity = Vector2.zero;
        m_rigidBody.angularVelocity = 0;
    }
    public void InitGravity()
    {
        m_rigidBody.gravityScale = m_args.m_gravity;
    }
    public void AddToGravity(float gravityAdded)
    {
        if (m_rigidBody.gravityScale <= m_args.BallMaxGravity)
            m_rigidBody.gravityScale += gravityAdded;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        //print("Ball OnCollusion: " + col.collider.name);
        if (col.gameObject.tag == "GameLowerBound")
            m_ballScript.BallFallen();
        else
            Physics2D.IgnoreCollision(m_collider, col.collider);
    }

    public void CheckBounds()
    {
        Vector3 ballPosition = this.gameObject.transform.position;
        if ((ballPosition.x - m_args.BallBoundDistanceTrigger < m_args.Bounds.GameLeftBound) && (m_rigidBody.velocity.x < 0))
        {
            //print("Reached left bound");
            Vector2 tempVelocity = m_rigidBody.velocity;
            if (Math.Abs(tempVelocity.x) > 0.5)
                tempVelocity.x *= -1;
            else
                tempVelocity.x = 0.5f;
            m_rigidBody.velocity = tempVelocity;
        }
        else if ((ballPosition.x + m_args.BallBoundDistanceTrigger > m_args.Bounds.GameRightBound) && (m_rigidBody.velocity.x > 0))
        {
            //print("Reached right bound");
            Vector2 tempVelocity = m_rigidBody.velocity;
            if (Math.Abs(tempVelocity.x) > 0.5)
                tempVelocity.x *= -1;
            else
                tempVelocity.x = 0.5f;

            m_rigidBody.velocity = tempVelocity;

        }
        else
            m_ballScript.EmitBallTrail(true);
    }
}
