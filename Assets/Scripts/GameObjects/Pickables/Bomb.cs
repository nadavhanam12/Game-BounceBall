using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour, IPickable
{

    [SerializeField] private SpriteRenderer m_bombSprite;
    private Sprite m_initSprite;

    private Rigidbody2D m_rigidBody;
    private Animator m_anim;
    private ParticleSystem m_particleSystem;
    private CircleCollider2D m_hitZone;
    private int m_index;

    public enum Status
    {
        Disabled,
        FreeInScene,
        Picked,
        Activated,
        ReachedLowerBound,
        Explode,
    }

    private Status m_status;

    private IPickableArgs m_args;

    private bool m_activateRight = true;

    public void Init(IPickableArgs args)
    {
        m_args = args;
        m_index = m_args.Index;
        m_rigidBody = GetComponent<Rigidbody2D>();
        m_anim = GetComponent<Animator>();
        m_particleSystem = GetComponentInChildren<ParticleSystem>();
        m_hitZone = gameObject.GetComponent<CircleCollider2D>();
        m_hitZone.radius = m_args.HitZoneRadius;
        m_initSprite = m_bombSprite.sprite;
        SetStatus(Status.Disabled);
    }
    public Status GetStatus() { return m_status; }

    public void GenerateInScene(Vector3 position)
    {
        transform.position = position;
        SetStatus(Status.FreeInScene);
    }
    public void CheckBounds()
    {
        Vector3 position = this.gameObject.transform.position;
        if (position.y <= m_args.Bounds.GamePlayGroundLowerBound)
        {
            if (m_status != Status.Activated)
            {
                SetStatus(Status.ReachedLowerBound);
                return;
            }
            SetStatus(Status.Explode);

        }
        else if (position.x - 2 < m_args.Bounds.GameLeftBound)
        {
            //m_curVelocityX *= -1;
            position.x = m_args.Bounds.GameRightBound - 3;
            this.gameObject.transform.position = position;
        }
        else if (position.x + 2 > m_args.Bounds.GameRightBound)
        {
            //m_curVelocityX *= -1;
            position.x = m_args.Bounds.GameLeftBound + 3;
            this.gameObject.transform.position = position;
        }
    }



    void FixedUpdate()
    {
        if ((m_status == Status.FreeInScene) || (m_status == Status.Activated))
        {
            CheckBounds();
        }
    }
    void SetStatus(Status newStatus)
    {

        switch (newStatus)
        {
            case Status.Disabled:
                //print("Disabled");
                gameObject.SetActive(false);

                m_bombSprite.gameObject.SetActive(true);//reset from explode
                m_bombSprite.transform.localScale = Vector3.one;//reset from explode
                m_particleSystem.gameObject.SetActive(true);//reset from explode
                m_bombSprite.sprite = m_initSprite;//reset from explode

                if (m_status == Status.Explode)
                {
                    m_args.PickablesManager.PickableDisabled(this);
                }

                break;
            case Status.FreeInScene:
                //print("FreeInScene");
                if (m_anim.isActiveAndEnabled)
                    m_anim.Play("IdleAnim");
                m_rigidBody.gravityScale = m_args.Gravity;
                gameObject.SetActive(true);
                break;

            case Status.ReachedLowerBound:
                //print("Reached lower bound");
                m_rigidBody.gravityScale = 0;
                m_rigidBody.velocity = Vector2.zero;
                break;

            case Status.Picked:
                //print("Picked");
                if (m_status != Status.ReachedLowerBound)
                {
                    m_rigidBody.gravityScale = 0;
                    m_rigidBody.velocity = Vector2.zero;
                }
                transform.localPosition = Vector3.zero;
                break;

            case Status.Activated:
                //print("Activated");
                m_args.PickablesManager.ReparentPickable(this);
                m_rigidBody.gravityScale = 5;
                int throwDirection = m_activateRight ? 1 : -1;
                m_rigidBody.AddForce(new Vector2(m_args.VelocityX * throwDirection, m_args.VelocityY));
                break;

            case Status.Explode:
                if (m_status != Status.Explode) //could arrive from ground impact and player impact at same frame
                {
                    m_rigidBody.gravityScale = 0;
                    m_rigidBody.velocity = Vector2.zero;
                    m_particleSystem.gameObject.SetActive(false);
                    if (m_anim.isActiveAndEnabled)
                        m_anim.Play("Explode");
                    CheckPlayersInHitZone();
                }

                break;

        }
        m_status = newStatus;

    }
    private void CheckPlayersInHitZone()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(m_hitZone.bounds.center, m_hitZone.radius);
        foreach (Collider2D hitCollider in hitColliders)
        {
            PlayerScript curPlayerScript = hitCollider.GetComponent<PlayerScript>();
            if (curPlayerScript != null)
            {
                print("PlayerHitByBomb");
                curPlayerScript.PlayerHitByBomb();
            }
        }
    }

    public void PickUp()
    {
        SetStatus(Status.Picked);
    }

    public void SetActivateDirections(bool throwRight)
    {
        m_activateRight = throwRight;
    }
    public void Activate()
    {
        SetStatus(Status.Activated);
    }

    public void Explode()
    {
        SetStatus(Status.Explode);
    }

    public void Destroy()
    {
        //m_args.PickablesManager.ReparentPickable(this);
        SetStatus(Status.Disabled);
    }

}
