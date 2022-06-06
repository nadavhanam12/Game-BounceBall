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
    public float m_startHeight = 2;
    [Range(0, 20)]
    public int m_autoPlayDifficult = 50;
    public float AutoPlayBallDistance = 2f;
    [Range(0, 1f)] public float KickCooldown = 0.5f;
    [Range(1, 2)] public float SlideSpeed;
    public float PlayerBoundDistanceTrigger = 1f;
    public float PlayerBoundDistanceSpawn = 1f;


}
