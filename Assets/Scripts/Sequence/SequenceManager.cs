using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerScript;
using static Sequence;

[Serializable]
public class SequenceManager : MonoBehaviour
{
    /*
    private GameCanvasScript m_gameCanvas;
    [SerializeField] int m_parallelSequences;

    [SerializeField] private List<Sequence> m_sequencesDifficulty_1;
    [SerializeField] private List<Sequence> m_sequencesDifficulty_2;
    [SerializeField] private List<Sequence> m_sequencesDifficulty_3;


    private List<Sequence> m_curSequences = new List<Sequence>();

    private List<KickType> m_playrSequence;



    public void Init(GameCanvasScript gameCanvas)
    {
        m_gameCanvas = gameCanvas;
        m_playrSequence = new List<KickType>();

        // Sequence seq1 = new Sequence();
        // seq1.Init(1);
        // Sequence seq2 = new Sequence();
        // seq2.Init(2);
        // Sequence seq3 = new Sequence();
        // seq3.Init(3);
        // for (int i = 0; i < 8; i++)
        // {
        //     m_sequencesDifficulty_1.Add(seq1);
        //     m_sequencesDifficulty_2.Add(seq2);
        //     m_sequencesDifficulty_3.Add(seq3);
        // }
        InitAllSequences();
        FirstInitSequences();
        InitListeners();


    }

    private void InitAllSequences()
    {
        foreach (Sequence seq in m_sequencesDifficulty_1)
        {
            seq.InitSize();
        }
        foreach (Sequence seq in m_sequencesDifficulty_2)
        {
            seq.InitSize();
        }
        foreach (Sequence seq in m_sequencesDifficulty_3)
        {
            seq.InitSize();
        }
    }

    private void InitListeners()
    {
        EventManager.AddHandler(EVENT.EventNormalKick, EventAddPlayerKickRegular);
        EventManager.AddHandler(EVENT.EventPowerKick, EventAddPlayerKickPower);
        //EventManager.AddHandler(EVENT.EventOnBallLost, InitSequences);

    }
    private void RemoveListeners()
    {
        EventManager.RemoveHandler(EVENT.EventNormalKick, EventAddPlayerKickRegular);
        EventManager.RemoveHandler(EVENT.EventPowerKick, EventAddPlayerKickPower);
        EventManager.RemoveHandler(EVENT.EventOnBallLost, InitSequences);
    }
    void OnDestroy()
    {
        RemoveListeners();
    }

    private void EventAddPlayerKickRegular() { this.AddPlayerKick(KickType.Regular); }
    private void EventAddPlayerKickPower() { this.AddPlayerKick(KickType.Power); }


    private Sequence PullSequence(int difficulty)
    {
        List<Sequence> m_chosenList;
        Sequence chosenSequence;
        int rnd;
        switch (difficulty)
        {
            case 1:
                m_chosenList = m_sequencesDifficulty_1;
                break;
            case 2:
                m_chosenList = m_sequencesDifficulty_2;
                break;
            case 3:
                m_chosenList = m_sequencesDifficulty_3;
                break;
            default:
                m_chosenList = m_sequencesDifficulty_1;
                break;


        }
        rnd = UnityEngine.Random.Range(0, m_chosenList.Count);
        chosenSequence = m_chosenList[rnd];
        //m_chosenList.Remove(chosenSequence);

        return chosenSequence.clone();
    }
    private void FirstInitSequences()
    {
        for (int i = 0; i < m_parallelSequences; i++)
        {
            m_curSequences.Add(PullSequence(1));
        }
        m_gameCanvas.UpdateSequenceUI(m_curSequences);
    }

    private void InitSequences()
    {
        bool isFirstDifficult;
        bool isFullSequence;
        for (int i = 0; i < m_parallelSequences; i++)
        {
            isFirstDifficult = m_curSequences[i].Difficulty == 1;
            isFullSequence = m_curSequences[i].GetOriginalSize() == m_curSequences[i].KickSequence.Count;
            if ((!isFirstDifficult) || (!isFullSequence))
            {
                m_curSequences[i] = PullSequence(1);
            }

        }
        m_gameCanvas.UpdateSequenceUI(m_curSequences);
    }


    public void FinishSequence(int indexFinished)
    {
        Sequence sequenceFinished = m_curSequences[indexFinished];
        Sequence sequenceNew = PullSequence(sequenceFinished.Difficulty + 1);
        sequenceNew.Status = SequenceStatus.NewSequenceAfterSuccess;
        m_curSequences[indexFinished] = sequenceNew;
        EventManager.Broadcast(EVENT.EventCombo);

    }
    public void UpdateExistSequence(int indexFinished)
    {
        Sequence curSequence = m_curSequences[indexFinished];
        curSequence.Status = SequenceStatus.UpdateCurrentSequence;

    }
    public void ChangeSequence(int indexFinished)
    {
        //maybe in future to change to sequence, for now just update
        Sequence curSequence = m_curSequences[indexFinished];
        curSequence.Status = SequenceStatus.NoChange;
    }


    public void AddPlayerKick(KickType kickType)
    {
        m_playrSequence.Add(kickType);
        Sequence curSequence;
        for (int index = 0; index < m_curSequences.Count; index++)
        {
            curSequence = m_curSequences[index];
            if (curSequence.KickSequence[0] == kickType)
            {
                curSequence.KickSequence.RemoveAt(0);
                if (curSequence.KickSequence.Count == 0)
                {
                    //sequence completed
                    FinishSequence(index);

                }
                else
                {
                    //sequence incompleted yet
                    UpdateExistSequence(index);

                }
            }
            else
            {
                //need to apply new sequence
                ChangeSequence(index);

            }

        }
        m_gameCanvas.UpdateSequenceUI(m_curSequences);
    }
    */
}
