using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Script : MonoBehaviour
{
    private ParticleSystem m_System;
    private bool m_TargetSet;

	void Start ()
    {
        m_System = GetComponent<ParticleSystem>();
	}
	
	void Update ()
    {
        if (!m_TargetSet && m_System.particleCount > 0)
        {
            List<Vector4> customData = new List<Vector4>();
            m_System.GetCustomParticleData(customData, 0);

            for (int i = 0; i < customData.Count; i++)
            {
                Vector3 targetPosition = Random.insideUnitSphere; // todo - replace with mesh location, etc
                customData[i] = new Vector4(targetPosition.x, targetPosition.y, targetPosition.z, 0.0f);
            }

            m_System.SetCustomParticleData(customData, 0);
            m_TargetSet = true;
        }
    }
}
