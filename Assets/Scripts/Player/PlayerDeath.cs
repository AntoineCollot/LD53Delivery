using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public GameObject colliders;
    public CinemachineVirtualCamera virtualCam;
    public static PlayerDeath Instance;
    CompositeStateToken gameIsPlayingToken;
    public GameObject deathFX;
    public bool isInvincible;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        gameIsPlayingToken = new CompositeStateToken();
        PlayerState.Instance.freezeInputsState.Add(gameIsPlayingToken);
        PlayerState.Instance.freezeGroundDetectionState.Add(gameIsPlayingToken);
    }

    private void Update()
    {
        gameIsPlayingToken.SetOn(!GameManager.Instance.GameIsPlaying);
    }

    public void Kill()
    {
        if (!GameManager.Instance.GameIsPlaying || isInvincible)
            return;

        GetComponent<Rigidbody2D>().velocity = new Vector2(-1, 1) * 7;
        colliders.SetActive(false);
        spriteRenderer.material.SetFloat("_HitTime", Time.time);
        virtualCam.Follow = null;
        virtualCam.LookAt = null;
        deathFX.SetActive(true);
        SFXManager.PlaySound(GlobalSFX.PlayerDeath);
        GameManager.Instance.GameOver();

    }
}
