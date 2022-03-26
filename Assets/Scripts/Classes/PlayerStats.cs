using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerStats
{
    public float m_hitZoneRadius = 5;
    public float m_movingSpeed = 0.5f;
    public float m_jumpSpeed = 0.5f;
    public float m_maxHeight = 2;
    [Range(1, 100)]
    public int m_autoPlayDifficult = 50;
    public float AutoPlayBallDistance = 2f;
}
