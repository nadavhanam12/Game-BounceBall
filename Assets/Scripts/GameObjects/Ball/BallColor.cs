using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallColor : MonoBehaviour
{
    private SpriteRenderer m_spriteRenderer;
    private Color m_curColor = Color.white;
    public int CurTweenId { get; private set; } = -1;
    private BallArgs m_args;


    public void Init(BallArgs args)
    {
        m_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        m_args = args;
    }
    public void UpdateColorToWhite()
    {
        m_spriteRenderer.color = Color.white;
    }
    public void CancelTween()
    {
        LeanTweenExt.LeanCancel(m_spriteRenderer.gameObject, CurTweenId);
        CurTweenId = -1;
    }
    public void UpdateColor(Color color)
    {
        m_curColor = color;
        m_spriteRenderer.color = m_curColor;
    }
    public void FadeOut()
    {
        LTDescr curTween =
        LeanTweenExt.MyLeanAlphaSpriteRenderer
        (m_spriteRenderer, 0, m_args.m_ballTimeFadeOut)
        .setEase(LeanTweenType.easeOutQuart)
        .setOnComplete(
            () =>
            { if (this) this.gameObject.SetActive(false); }
            );
        CurTweenId = curTween.id;
    }

    public void FadeIn(Action actionOnComplete)
    {
        Color curColor = m_spriteRenderer.color;
        curColor.a = 0.2f;
        m_spriteRenderer.color = curColor;
        LTDescr curTween =
        LeanTweenExt.MyLeanAlphaSpriteRenderer
        (m_spriteRenderer, 1, m_args.m_ballTimeFadeIn)
        .setEase(LeanTweenType.easeInSine)
        .setOnComplete(actionOnComplete);
        CurTweenId = curTween.id;
    }

    public Color GetColor()
    {
        return m_curColor;
    }
    public SpriteRenderer GetSprite()
    {
        return m_spriteRenderer;
    }

}
