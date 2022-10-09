using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

public class CheerScript : MonoBehaviour
{

    [SerializeField] private TMP_Text m_cheerText;
    private bool m_isRunning = false;

    private Vector2 m_initTextPosition;
    private Vector3 m_initCrowdLeftPosition;
    private Vector3 m_initCrowdRightPosition;

    [SerializeField] private float m_playTime = 1.5f;
    [SerializeField] private int m_crowdMoveDistance = 480;

    public AnimationCurve animCurve;

    [SerializeField] private GameObject CrowdLeft;
    [SerializeField] private GameObject CrowdRight;


    private List<string> CheerOptions = new List<string>();
    private GameType m_gameType;


    private void InitCheerTextOptions()
    {
        CheerOptions.Add("Nice!");
        CheerOptions.Add("Good Job!");
        CheerOptions.Add("Champion!");
        CheerOptions.Add("Combo!");
        CheerOptions.Add("Incredible!");
        CheerOptions.Add("Wonderful!");
        CheerOptions.Add("Awesome!");


    }

    public void Init(GameType gameType)
    {
        m_gameType = gameType;
        m_initTextPosition = m_cheerText.rectTransform.anchoredPosition;
        m_initCrowdLeftPosition = CrowdLeft.gameObject.transform.localPosition;
        m_initCrowdRightPosition = CrowdRight.gameObject.transform.localPosition;
        InitCheerTextOptions();

        if (m_gameType == GameType.PvE || m_gameType == GameType.PvP)
        {
            m_cheerText.gameObject.SetActive(false);
        }
    }

    private string GetRandomCheerText()
    {
        int length = CheerOptions.Count;
        int rnd = Random.Range(0, CheerOptions.Count);
        return CheerOptions[rnd];
    }


    public void Activate(bool withTextAndInit)
    {
        if (!m_isRunning)
        {
            m_isRunning = true;

            this.gameObject.SetActive(true);
            //.setEase(LeanTweenType.easeOutQuad)
            LeanTween.moveLocalX
            (CrowdLeft.gameObject, m_crowdMoveDistance, m_playTime / 4)
            .setEase(LeanTweenType.easeOutBounce)
            .setOnComplete(
                () => LeanTween.moveLocalX
                (CrowdLeft.gameObject, m_initCrowdLeftPosition.x, m_playTime / 4)
                .setDelay(m_playTime / 2));

            LeanTween.moveLocalX
            (CrowdRight.gameObject, -m_crowdMoveDistance, m_playTime / 4)
            .setEase(LeanTweenType.easeOutBounce)
            .setOnComplete(
                () => LeanTween.moveLocalX
                (CrowdRight.gameObject, m_initCrowdRightPosition.x, m_playTime / 4)
                .setDelay(m_playTime / 2));

            if (withTextAndInit)
            {
                m_cheerText.text = GetRandomCheerText();
                LeanTween.moveLocalX
                           (m_cheerText.gameObject, -m_initTextPosition.x * 2, m_playTime)
                           .setEase(animCurve)
                           .setOnComplete(
                               () => Deactivate());
            }

        }
        else
        {
            //Deactivate();
            //Activate();
        }

    }

    private void InitValues()
    {
        m_cheerText.rectTransform.anchoredPosition = m_initTextPosition;
        CrowdLeft.gameObject.transform.localPosition = m_initCrowdLeftPosition;
        CrowdRight.gameObject.transform.localPosition = m_initCrowdRightPosition;

    }

    public bool IsRunning()
    {
        return m_isRunning;
    }

    public void Deactivate()
    {

        //this.gameObject.SetActive(false);

        InitValues();
        m_isRunning = false;
        // Invoke("Activate", 1f);

    }


}

