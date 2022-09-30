using System.Collections;
using UnityEngine;

public class CameraVFX : MonoBehaviour
{
    public float duration;
    public float magnitude;

    public void Shake()
    {
        StartCoroutine(ShakeCorutine());

    }

    IEnumerator ShakeCorutine()
    {
        float elapsed = 0.0f;
        Vector3 initPos = transform.position;
        while (elapsed < duration)
        {
            float xPos = Random.Range(-1 * magnitude, magnitude);
            float yPos = Random.Range(-1 * magnitude, magnitude);

            transform.position = new Vector3(xPos, initPos.y, initPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = initPos;
    }
}
