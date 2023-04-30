using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreSystem : MonoBehaviour
{
    public float levelTime { get; private set; }
    public int killCount { get; private set; }

    public float BloodLevel => killCount / LevelManager.Instance.levelConstants.enemyCount;

    public string Bloodiness
    {
        get
        {
            if (BloodLevel < 0.1f)
                return "Peacefull";
            else if (BloodLevel < 0.3f)
                return "Meh";
            else if (BloodLevel < 0.5f)
                return "Pretty good ngl";
            else if (BloodLevel < 0.75f)
                return "Wow";
            else if (BloodLevel < 0.9f)
                return "Serious Damages";
            else return "AHAHAHAH";
        }
    }

    public static ScoreSystem Instance;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.onGameStart.AddListener(OnGameStart);
    }

    private void OnGameStart()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.GameIsPlaying)
            levelTime += Time.deltaTime;
    }

    public void AddKill()
    {
        killCount++;
    }
}