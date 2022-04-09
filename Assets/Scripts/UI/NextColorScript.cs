using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NextColorScript : MonoBehaviour
{
    private bool m_initialized;
    private bool m_inTween;
    [SerializeField] private RawImage m_ballImage;
    public void Init()
    {
        if (!m_initialized)
        {
            m_inTween = false;
            m_initialized = true;
        }

    }

    public void SetColor(Color color)
    {
        m_ballImage.color = color;
        Glow();
    }

    public void Glow()
    {
        if (!m_inTween)
        {
            m_inTween = true;
            LeanTween.scale(m_ballImage.gameObject, Vector3.one * 1.3f, 0.25f)
                    .setLoopPingPong(1)
                    .setEase(LeanTweenType.easeInOutBack)
                    .setOnComplete(FinishedTween);
        }

    }
    private void FinishedTween()
    {
        m_inTween = false;
    }
}
