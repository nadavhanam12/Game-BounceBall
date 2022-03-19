using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static PlayerScript;

public class SequenceMenuUI : MonoBehaviour
{
    private SequenceRowUI[] m_sequenceRowUI;

    private List<Sequence> m_currentSequences;

    private bool Initialized = false;

    public void Init()
    {
        if (!Initialized)
        {
            Initialized = true;
            InitSequenceUiList();
            foreach (SequenceRowUI seqRowUI in m_sequenceRowUI)
            {
                seqRowUI.Init();
            }
            //this.gameObject.SetActive(true);

        }

    }

    public bool isInitialized()
    {
        return Initialized;
    }

    void InitSequenceUiList()
    {
        m_sequenceRowUI = GetComponentsInChildren<SequenceRowUI>();
        foreach (SequenceRowUI sequenceRowUI in m_sequenceRowUI)
        {
            sequenceRowUI.gameObject.SetActive(false);
        }

    }


    public void UpdateSequenceUI(List<Sequence> newSequenceList)
    {
        m_currentSequences = newSequenceList;
        Sequence curSequence;
        for (int i = 0; i < m_currentSequences.Count; i++)
        //for (int i = 0; i < 1; i++)
        {
            curSequence = m_currentSequences[i];

            m_sequenceRowUI[i].UpdateSequenceQueue(curSequence);

        }



    }
}
