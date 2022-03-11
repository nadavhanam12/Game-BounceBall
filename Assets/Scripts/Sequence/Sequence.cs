using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManagerScript;
using static PlayerScript;

[Serializable]
public class Sequence
{
    public enum SequenceStatus
    {
        NoChange,
        NewSequenceAfterSuccess,
        NewSequenceAfterFailure,
        UpdateCurrentSequence,


    };
    public List<KickType> KickSequence;
    public int Prize;
    public int Difficulty;
    private int SequenceSize;

    public SequenceStatus Status = SequenceStatus.NoChange;

    public int GetOriginalSize()
    {
        return SequenceSize;
    }
    public void InitSize()
    {
        SequenceSize = KickSequence.Count;
    }

    public Sequence clone()
    {
        Sequence newSeq = new Sequence();
        newSeq.Difficulty = this.Difficulty;
        newSeq.Prize = this.Prize;
        newSeq.Status = this.Status;
        newSeq.SequenceSize = this.SequenceSize;
        newSeq.KickSequence = new List<KickType>();
        foreach (KickType kickType in this.KickSequence)
        {
            newSeq.KickSequence.Add(kickType);
        }
        //newSeq.KickSequence.Reverse();

        return newSeq;

    }


}
