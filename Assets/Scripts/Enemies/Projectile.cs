using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    float startTime;
    const float LIFETIME = 2;

    public GameObject explodeFXPrefab;

    // Start is called before the first frame update
    void Start()
    {
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (startTime + LIFETIME < Time.time)
            Explode();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.attachedRigidbody == null)
            return;
        if (collision.attachedRigidbody.transform == PlayerState.Instance.transform)
        {
            PlayerDeath.Instance.Kill();
        }

        Explode();
    }

    public void Explode()
    {
        GameObject explodeFX = Instantiate(explodeFXPrefab, transform.position, Quaternion.identity, null);
        Destroy(explodeFX, 2);
        SFXManager.PlaySound(GlobalSFX.ProjectileExplosion);

        Destroy(gameObject);
    }
}
