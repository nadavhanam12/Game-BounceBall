using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallHitVisual : MonoBehaviour
{
    private bool isActive = false;
    private SpriteRenderer m_spriteRenderer;
    private Vector3 m_initialScale;

    //private Animator m_anim;
    [SerializeField]
    [Range(0, 1)] private float ScaleDownFromOriginalScale = 0.5f;
    [SerializeField] private float timeToPlay = 1f;
    [SerializeField] private float ScaleFactor = 1f;
    [SerializeField] private float IdleTime = 1f;


    public void Init(Vector3 scale)
    {
        gameObject.SetActive(false);
        m_initialScale = scale * ScaleDownFromOriginalScale;
        gameObject.transform.localScale = m_initialScale;
        m_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        //m_anim = GetComponent<Animator>();
        //m_anim.speed = 1 / timeToPlay;

    }

    public void Activate(Color color, Vector3 position)
    {
        if (isActive)
        {
            Deactivate();
        }
        RandomAngle();
        m_spriteRenderer.color = color;
        transform.position = position;
        isActive = true;
        gameObject.SetActive(true);
        fadeIn();
        LeanTween.rotateZ(gameObject, 2 * 360 + transform.rotation.z, timeToPlay + IdleTime / 2)
        .setEase(LeanTweenType.easeOutCirc);
        LeanTween.scale(gameObject, m_initialScale * ScaleFactor, timeToPlay)
        .setOnComplete(fadeout);

    }

    void RandomAngle()
    {
        int rnd = Random.Range(0, 360);
        transform.rotation = Quaternion.Euler(0, 0, rnd);
    }
    public void fadeIn()
    {
        LeanTween.alpha(gameObject, 1, timeToPlay)
        .setEase(LeanTweenType.easeInCirc);
    }
    public void fadeout()
    {
        LeanTween.alpha(gameObject, 0.5f, IdleTime)
        .setEase(LeanTweenType.easeInCirc)
         .setOnComplete(Deactivate); ;
    }

    public void Deactivate()
    {
        //m_anim.enabled = false;
        isActive = false;
        gameObject.SetActive(false);
        gameObject.transform.localScale = m_initialScale;
    }

    public bool IsActive()
    {
        return isActive;
    }



}
