using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HandGesturesUI : MonoBehaviour
{
    public enum State { None, RegularKickGesture, SlideKickGesture, JumpGesture }
    Animator m_anim;
    State m_curState;
    public GameObject m_regKickHighlightCircle;
    public GameObject m_slideHighlightCircle;

    public GameObject m_jumpHighlightCircle;

    public void Init()
    {
        m_anim = GetComponent<Animator>();
        m_curState = State.None;
    }

    public void PlayRegularKickGesture()
    {
        m_curState = State.RegularKickGesture;
        StartHighlightCircle(m_regKickHighlightCircle);
        m_anim.SetTrigger("RegKickGestureTrigger");
    }
    public void PlaySlideKickGesture()
    {
        m_curState = State.SlideKickGesture;
        StartHighlightCircle(m_slideHighlightCircle);
        m_anim.SetTrigger("SlideGestureTrigger");

    }
    public void PlayJumpGesture()
    {
        m_curState = State.JumpGesture;
        StartHighlightCircle(m_jumpHighlightCircle);
        m_anim.SetTrigger("JumpGestureTrigger");

    }

    public void StopGesture()
    {
        //should change the hand to good gesture and tween it up and down and disapear
        m_anim.SetTrigger("IdleTrigger");


        GameObject activeCircle = m_regKickHighlightCircle;
        switch (m_curState)
        {
            case State.RegularKickGesture:
                activeCircle = m_regKickHighlightCircle;
                break;
            case State.SlideKickGesture:
                activeCircle = m_slideHighlightCircle;
                break;
            case State.JumpGesture:
                activeCircle = m_jumpHighlightCircle;
                break;
        }
        StopHighlightCircle(activeCircle);
    }

    private void StartHighlightCircle(GameObject circle)
    {
        LeanTween.cancel(circle);
        circle.SetActive(true);
        LeanTween.scale(circle, Vector3.one * 1.5f, 0.5f).setLoopPingPong();
    }
    void StopHighlightCircle(GameObject circle)
    {
        LeanTween.cancel(m_regKickHighlightCircle);
        m_regKickHighlightCircle.SetActive(false);
    }

}
