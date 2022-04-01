using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

public class TurnsUI : MonoBehaviour
{

    [SerializeField] private TMP_Text m_yourTurnText;
    [SerializeField] private TMP_Text m_lostTurnText;
    private Vector3 m_initTextPosition;

    [SerializeField] private float m_playTime = 1.5f;

    private float m_MoveTextX = 300;
    public AnimationCurve animCurve;


    private List<string> yourTurnOptions = new List<string>();
    private List<string> lostTurnOptions = new List<string>();

    private int m_yourTurnTweenId = -1;
    private int m_lostTurnTweenId = -1;




    private void InitYourTurnOptions()
    {
        yourTurnOptions.Add("Your Turn!");
        yourTurnOptions.Add("Bounce!");
        yourTurnOptions.Add("Begin!");
        yourTurnOptions.Add("Play!");
        yourTurnOptions.Add("Go!");
    }
    private void InitLostTurnOptions()
    {
        lostTurnOptions.Add("Lost Turn!");
        lostTurnOptions.Add("Fight!");
        lostTurnOptions.Add("Stop Him!");
        lostTurnOptions.Add("Try Next Time!");
    }

    public void Init()
    {
        m_initTextPosition = m_yourTurnText.gameObject.transform.localPosition;
        m_lostTurnText.gameObject.transform.localPosition = m_initTextPosition;

        m_MoveTextX = (m_initTextPosition.x * 2);
        InitYourTurnOptions();
        InitLostTurnOptions();
        //Invoke("Activate", 1f);
    }


    private string GetRandomYourTurnText()
    {
        int length = yourTurnOptions.Count;
        int rnd = Random.Range(0, yourTurnOptions.Count);
        return yourTurnOptions[rnd];
    }
    private string GetRandomLostTurnText()
    {
        int length = lostTurnOptions.Count;
        int rnd = Random.Range(0, lostTurnOptions.Count);
        return lostTurnOptions[rnd];
    }


    public void Activate(bool isPlayerTurn)
    {
        if (isPlayerTurn)
        {
            if (m_yourTurnTweenId != -1)
            {
                LeanTweenExt.LeanCancel(m_yourTurnText.gameObject, m_yourTurnTweenId);
                m_yourTurnTweenId = -1;
            }
            m_yourTurnText.text = GetRandomYourTurnText();
            LeanTween.moveLocalX
                    (m_yourTurnText.gameObject, -m_initTextPosition.x, m_playTime)
                    .setEase(animCurve)
                    .setOnComplete(() => Deactivate(true));
        }
        else
        {
            if (m_lostTurnTweenId != -1)
            {
                LeanTweenExt.LeanCancel(m_lostTurnText.gameObject, m_yourTurnTweenId);
                m_yourTurnTweenId = -1;
            }
            m_lostTurnText.text = GetRandomLostTurnText();
            LeanTween.moveLocalX
                    (m_lostTurnText.gameObject, -m_initTextPosition.x, m_playTime)
                    .setEase(animCurve)
                    .setOnComplete(() => Deactivate(false));
        }
    }

    private void InitValues(bool isPlayerTurn)
    {
        GameObject textObject = isPlayerTurn ? m_yourTurnText.gameObject : m_lostTurnText.gameObject;

        textObject.gameObject.transform.localPosition = m_initTextPosition;
    }


    public void Deactivate(bool isPlayerTurn)
    {
        InitValues(isPlayerTurn);
    }


}

