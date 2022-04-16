using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurPlayerUI : MonoBehaviour
{

    [SerializeField] RawImage m_image;
    private int m_curTweenId = -1;
    private Vector3 m_textInitScale;
    private int m_curCombo;
    private Vector3 m_imageScale;

    [Range(0.05f, 1f)] public float m_timeToTween = 0.2f;

    private Texture m_playerImage1;
    private Texture m_playerImage2;





    public void Init(Texture playerImage1, Texture playerImage2)
    {
        m_textInitScale = transform.localScale;
        m_curCombo = 0;
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
                m_image.rectTransform.localScale = m_imageScale;
            }
            else if ((!isPlayerTurn) && (m_imageScale.x != -1))
            {
                m_imageScale.x = -1;
                m_image.rectTransform.localScale = m_imageScale;
            }


            if ((m_curTweenId == -1) && (m_curCombo != 0))
            {
                ScaleUp();
            }
        }

    }
    private void ScaleUp()
    {
        m_curTweenId =
        LeanTween.scale(m_image.gameObject, m_textInitScale * 2, m_timeToTween)
                .setOnComplete(ScaleDown)
                .id;
    }
    private void ScaleDown()
    {
        m_curTweenId =
        LeanTween.scale(m_image.gameObject, m_textInitScale, m_timeToTween)
                .setOnComplete(FinishTween)
                .id;
    }

    private void FinishTween()
    {
        m_curTweenId = -1;
    }

}
