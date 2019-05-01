using UnityEngine;

public class DestroyParticle : MonoBehaviour
{
    private float DestroyCountdown = 1;

    // Update is called once per frame
    void Update()
    {
        DestroyCountdown -= Time.deltaTime;
        if (DestroyCountdown <= 0)
        {
            Destroy(gameObject);
        }
    }
}