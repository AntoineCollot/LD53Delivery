using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LetterBox : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.attachedRigidbody == null) return;
        if (collision.attachedRigidbody.transform == PlayerState.Instance.transform)
        {
            GameManager.Instance.ClearLevel();

            GetComponentInChildren<ParticleSystem>().Emit(20);
            SFXManager.PlaySound(GlobalSFX.LevelCleared);
        }
    }
}
