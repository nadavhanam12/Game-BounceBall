using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using static PlayerScript;
using static Sequence;

public class SequenceRowUI : MonoBehaviour
{
    private List<RawImage> m_sequenceImages;
    private List<Vector2> m_initPositionImages;
    private Sequence m_curSequence;

    private float m_texturesSpaceBetween;
    [SerializeField] private TMP_Text m_prizeText;

    [SerializeField] private Texture m_textureRegular;
    [SerializeField] private Texture m_texturePower;
    [SerializeField] private Texture m_textureUp;
    [SerializeField] private ParticleSystem m_particleSystemRow;
    [SerializeField] private ParticleSystem m_particleSystemImage;
    [SerializeField] private TMP_Text m_cheerText;
    public float m_playTime = 0.2f;
    private PlayableDirector m_timeLine;
    private Vector2 m_cheerTextInitPosition;

    private bool Initialized = false;
    private bool m_openForUpdate = true;

    private Queue<Sequence> m_updatesQueue;
    private List<string> CheerOptions = new List<string>();


    private void InitCheerTextOptions()
    {
        CheerOptions.Add("Nice !");
        CheerOptions.Add("Good Job !");
        CheerOptions.Add("Champion !");
        CheerOptions.Add("Combo !");
        CheerOptions.Add("incredible !");
        CheerOptions.Add("wonderful !");
        CheerOptions.Add("awesome !");


    }
    private string GetRandomCheerText()
    {
        int length = CheerOptions.Count;
        int rnd = UnityEngine.Random.Range(0, CheerOptions.Count);
        return CheerOptions[rnd];
    }

    public void Init()
    {
        if (!Initialized)
        {
            Initialized = true;

            m_sequenceImages = GetComponentsInChildren<RawImage>(true).ToList();

            m_cheerTextInitPosition = m_cheerText.rectTransform.anchoredPosition;
            m_cheerText.gameObject.SetActive(false);
            m_timeLine = m_cheerText.GetComponent<PlayableDirector>();
            //m_timeLine.duration = (m_cheerTextTime);

            m_updatesQueue = new Queue<Sequence>();

            m_initPositionImages = new List<Vector2>();
            InitTextures();
            InitParticles();
            InitCheerTextOptions();
            //this.gameObject.SetActive(true);
        }



    }
    private void InitParticles()
    {
        m_particleSystemImage.Clear();
        m_particleSystemImage.GetComponent<RectTransform>().anchoredPosition = m_initPositionImages[0];

        m_particleSystemRow.Clear();
        m_particleSystemRow.GetComponent<RectTransform>().anchoredPosition = m_prizeText.rectTransform.anchoredPosition;

    }

    private void InitTextures()
    {
        foreach (RawImage Image in m_sequenceImages)
        {
            TurnOff(Image);
            m_initPositionImages.Add(Image.rectTransform.anchoredPosition);
        }
        m_texturesSpaceBetween = m_initPositionImages[1].x - m_initPositionImages[0].x;
    }

    private void InitImagesPositions()
    {
        //print("InitImagesPositions");
        for (int i = 0; i < m_sequenceImages.Count; i++)
        {
            TurnOff(m_sequenceImages[i]);
            m_sequenceImages[i].rectTransform.anchoredPosition = m_initPositionImages[i];

        }
        m_cheerText.rectTransform.anchoredPosition = m_cheerTextInitPosition;
        m_prizeText.gameObject.SetActive(false);



    }

    private void ApplyImageVFX()
    {
        //("ApplyImageVFX");
        m_particleSystemImage.Emit(100);
    }

    private void ApplyRowVFX()
    {
        //print("ApplyRowVFX");

        m_particleSystemRow.Stop();
        m_particleSystemRow.Clear();
        m_particleSystemRow.GetComponent<ParticlesToPoint>().Init();
        m_particleSystemRow.Emit(500);
    }

    private void InitFirstSequence(Sequence seq)
    {
        //print("InitFirstSequence");

        this.gameObject.SetActive(true);
        UpdateCurSeq(seq);
        m_prizeText.text = m_curSequence.Prize.ToString();
        List<KickType> kickSequence = m_curSequence.KickSequence;
        int kicksCount = kickSequence.Count;
        for (int i = 0; i < kicksCount; i++)
        {
            TurnOn(m_sequenceImages[i], KickToTexture(kickSequence[i]));
        }
        m_openForUpdate = true;
    }

    private void UpdateExistSequence(Sequence seq)
    {

        //print("UpdateExistSequence");
        UpdateCurSeq(seq);
        int texturesCount = m_sequenceImages.Count;


        //means we have update for this sequence
        bool foundFirstActive = false;
        RawImage curImage;
        for (int i = 0; i < texturesCount; i++)
        {
            curImage = m_sequenceImages[i];
            LeanTween.moveLocalX(curImage.gameObject, curImage.rectTransform.anchoredPosition.x - m_texturesSpaceBetween, m_playTime);

            if ((IsOn(curImage)) && (!foundFirstActive))
            {
                foundFirstActive = true;
                TurnOff(curImage);
            }
        }
        ApplyImageVFX();
        m_openForUpdate = true;

    }


    private void UpdateCurSeq(Sequence seq)
    {
        m_curSequence = seq.clone();
        //m_curSequence = seq;
        SetPrize();
    }


    private void UpdateNewSequenceAfterSuccess(Sequence seq)
    {
        //print("UpdateNewSequenceAfterSuccess");

        UpdateCurSeq(seq);

        InitImagesPositions();
        ApplyRowVFX();
        m_openForUpdate = false;
        m_cheerText.text = GetRandomCheerText();
        m_cheerText.gameObject.SetActive(true);
        m_timeLine.Play();

    }

    private void UpdateNewSequenceAfterFailure(Sequence seq)
    {
        //print("UpdateNewSequenceAfterFailure");
        UpdateCurSeq(seq);
        InitImagesPositions();
        m_openForUpdate = false;

        //TODO: anim for failure
        /*ApplyRowVFX();
        m_cheerText.gameObject.SetActive(true);
        m_timeLine.Play();*/

        UpdateNewSequenceAfterVFX();

    }



    public void UpdateNewSequenceAfterVFX()
    {
        //print("UpdateNewSequenceAfterVFX");


        InitImagesPositions();
        m_cheerText.gameObject.SetActive(false);
        m_prizeText.gameObject.SetActive(true);

        List<KickType> kickSequence = m_curSequence.KickSequence;
        int kicksCount = kickSequence.Count;
        for (int i = 0; i < kicksCount; i++)
        {
            TurnOn(m_sequenceImages[i], KickToTexture(kickSequence[i]));
        }

        m_openForUpdate = true;

    }

    private void UpdateExistSequenceNoChange(Sequence seq)
    {
        //print("UpdateExistSequenceNoChange");
        //TODO: anim for no change


        m_openForUpdate = true;
    }

    private void SetPrize()
    {
        //m_prizeText.gameObject.SetActive(true);
        m_prizeText.text = m_curSequence.Prize.ToString();
    }

    void Update()
    {
        //print("Update");
        /*if ((!m_openForUpdate) && (m_updatesQueue.Count() > 0))
        {
            PrintCurSeq();
        }*/
        if ((m_openForUpdate) && (m_updatesQueue.Count() > 0))
        {
            m_openForUpdate = false;
            Sequence seq = m_updatesQueue.Dequeue();
            //print("Dequeue m_updatesQueue");
            if (m_curSequence == null)
            {
                InitFirstSequence(seq);
            }
            else
            {
                switch (seq.Status)
                {
                    case (SequenceStatus.UpdateCurrentSequence):
                        UpdateExistSequence(seq);
                        break;
                    case (SequenceStatus.NewSequenceAfterSuccess):
                        UpdateNewSequenceAfterSuccess(seq);
                        break;
                    case (SequenceStatus.NewSequenceAfterFailure):
                        UpdateNewSequenceAfterFailure(seq);
                        break;
                    case (SequenceStatus.NoChange):
                        UpdateExistSequenceNoChange(seq);
                        break;


                }
            }
        }

    }
    public void UpdateSequenceQueue(Sequence seq)
    {
        //print("UpdateSequenceQueue");
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }
        m_updatesQueue.Enqueue(seq);
    }

    private void TurnOn(RawImage img, Texture texture)
    {
        img.texture = texture;
        img.gameObject.SetActive(true);
    }

    private void TurnOff(RawImage img)
    {
        img.gameObject.SetActive(false);
    }
    private bool IsOn(RawImage img)
    {
        return img.gameObject.activeInHierarchy;
    }

    private Texture KickToTexture(KickType kick)
    {
        switch (kick)
        {
            case KickType.Regular:
                return m_textureRegular;
            /*case KickType.Up:
                return m_textureUp;*/
            case KickType.Power:
                return m_texturePower;
            default:
                return m_textureRegular;
        }
    }



}
