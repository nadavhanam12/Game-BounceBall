using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Bomb;

public class PickablesManager : MonoBehaviour
{
    private Bomb m_bombInstance;
    [SerializeField] private int m_bombsMaxNumber;
    [SerializeField] private float m_timeBetweenGenerates = 3f;
    [SerializeField] private float m_pickablesGravity;
    [SerializeField] private float m_pickUpDistance = 1f;
    [SerializeField] private float m_bombHitZoneRadius = 1f;

    [SerializeField] private float m_bombVelocityX;
    [SerializeField] private float m_bombVelocityY;

    private Bomb[] m_bombsArray;
    private GameObject m_ballsArrayGameObject;

    private PickablesManagerArgs m_args;

    private int m_bombsCountInScene = 0;
    private bool isGamePaused = false;
    private bool m_shouldGenerate = true;
    private bool m_initialized = false;

    public void Init(PickablesManagerArgs args)
    {
        m_args = args;
        m_ballsArrayGameObject = transform.Find("BombsArray").gameObject;
        m_bombsArray = new Bomb[m_bombsMaxNumber];
        InitBombs();

    }
    public void FinishInitialize()
    {
        m_initialized = true;
    }
    public void SetGamePause(bool isPause)
    {
        isGamePaused = isPause;
        m_shouldGenerate = !isGamePaused;
        if ((!isGamePaused) && (m_initialized))
        {
            StartCoroutine(GenerateBombs());
        }
    }


    public void GeneratePickables()
    {
        m_shouldGenerate = true;
        StartCoroutine(GenerateBombs());
    }

    private int GetFreeBombIndex()
    {
        for (int i = 0; i < m_bombsArray.Length; i++)
        {
            if (m_bombsArray[i].GetStatus() == Status.Disabled)
            {
                return i;
            }
        }
        return -1;
    }


    IEnumerator GenerateBombs()
    {

        while (m_shouldGenerate)
        {
            if (m_bombsCountInScene == m_bombsMaxNumber) //reached max Bombs
            {
                //print("Reached Max Bombs");
                m_shouldGenerate = false;
                break;
            }
            int bombIndex = GetFreeBombIndex();
            if (bombIndex == -1)    //no free bombs left
            {
                //print("no free bombs left");
                m_shouldGenerate = false;
                break;
            }

            GenerateBomb(bombIndex);
            yield return new WaitForSeconds(m_timeBetweenGenerates);

        }
        yield return new WaitForSeconds(m_timeBetweenGenerates);
    }

    private void InitBombs()
    {
        m_bombInstance = GetComponentInChildren<Bomb>();
        IPickableArgs pickableArgs = new IPickableArgs();
        pickableArgs.Gravity = m_pickablesGravity;
        pickableArgs.Bounds = m_args.GameBounds;
        pickableArgs.VelocityX = m_bombVelocityX;
        pickableArgs.VelocityY = m_bombVelocityY;
        pickableArgs.PickablesManager = this;
        pickableArgs.HitZoneRadius = m_bombHitZoneRadius;
        for (int i = 0; i < m_bombsArray.Length; i++)
        {
            pickableArgs.Index = i;
            m_bombsArray[i] = (Instantiate(m_bombInstance, m_ballsArrayGameObject.transform));
            m_bombsArray[i].Init(pickableArgs);
        }
        m_bombInstance.gameObject.SetActive(false);
    }

    private void GenerateBomb(int bombIndex)
    {
        float randomPositionX = Random.Range(m_args.GameBounds.GameLeftBound + 2f, m_args.GameBounds.GameRightBound - 2f);
        Vector3 bombGeneratePosition = m_ballsArrayGameObject.transform.position;
        bombGeneratePosition.x = randomPositionX;
        m_bombsCountInScene++;
        m_bombsArray[bombIndex].GenerateInScene(bombGeneratePosition);
    }

    public void ReparentPickable<T>(T pickable) where T : Bomb
    {
        pickable.gameObject.transform.parent = m_ballsArrayGameObject.transform;
    }
    public void PickableDisabled<T>(T pickable) where T : Bomb
    {
        m_bombsCountInScene--;
        GeneratePickables();
    }


}
