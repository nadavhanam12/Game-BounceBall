using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlesToPoint : MonoBehaviour
{
    private ParticleSystem m_System;
    private bool m_TargetSet;
    [SerializeField] private GameObject m_target;

    void Start()
    {
        m_System = GetComponent<ParticleSystem>();
        Init();
    }

    void Update()
    {
        if (!m_TargetSet && m_System.particleCount > 0)
        {
            List<Vector4> customData = new List<Vector4>();
            m_System.GetCustomParticleData(customData, 0);

            for (int i = 0; i < customData.Count; i++)
            {
                Vector3 targetPosition = m_target.transform.position;
                targetPosition.y = targetPosition.y * 0.9f;
                customData[i] = new Vector4(targetPosition.x, targetPosition.y, targetPosition.z, 0.0f);
            }

            m_System.SetCustomParticleData(customData, 0);
            m_TargetSet = true;
        }
        else
        {
            if ((m_System.particleCount == 0) && (m_TargetSet))
            {
                //print("init m_TargetSet");
                Init();
            }
        }
    }

    public void Init()
    {
        m_TargetSet = false;
    }
}
