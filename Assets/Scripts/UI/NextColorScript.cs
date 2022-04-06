using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NextColorScript : MonoBehaviour
{
    private bool m_initialized;
    [SerializeField] private RawImage m_ballImage;
    public void Init()
    {
        if (!m_initialized)
        {




            m_initialized = true;
        }

    }

    public void SetColor(Color color)
    {
        m_ballImage.color = color;
    }

    public void Glow(bool glow)
    {
        if (glow)
        {
            LeanTween.scale(m_ballImage.gameObject, Vector3.one * 2f, 1f)
                    .setLoopPingPong()
                    .setEase(LeanTweenType.easeInOutBack);
        }

    }
}
