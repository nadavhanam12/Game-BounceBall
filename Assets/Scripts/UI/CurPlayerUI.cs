using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurPlayerUI : MonoBehaviour
{

    [SerializeField] RawImage m_image;
    private int m_curTweenId = -1;
    private Vector3 m_imageScale;

    [Range(0.05f, 1f)] public float m_timeToTween = 0.2f;

    private Texture m_playerImage1;
    private Texture m_playerImage2;





    public void Init(Texture playerImage1, Texture playerImage2)
    {
        m_imageScale = m_image.rectTransform.localScale;
        m_playerImage1 = playerImage1;
        m_playerImage2 = playerImage2;
    }

    public void SetImage(bool isPlayerTurn)
    {
        Texture curImage = isPlayerTurn ? m_playerImage1 : m_playerImage2;
        if (m_image.texture != curImage)
        {

            m_image.texture = curImage;
            if ((isPlayerTurn) && (m_imageScale.x != 1))
            {
                m_imageScale.x = 1;
            }
            else if ((!isPlayerTurn) && (m_imageScale.x != -1))
            {
                m_imageScale.x = -1;
            }


            if ((m_curTweenId == -1))
            {
                ScaleUp();
            }
        }

    }
    private void ScaleUp()
    {
        m_curTweenId = LeanTween.scale(m_image.gameObject, m_imageScale * 2, m_timeToTween)
                .setOnComplete(ScaleDown)
                .id;
    }
    private void ScaleDown()
    {
        m_curTweenId =
        LeanTween.scale(m_image.gameObject, m_imageScale, m_timeToTween)
                .setOnComplete(FinishTween)
                .id;
    }

    private void FinishTween()
    {
        m_curTweenId = -1;
    }

}
