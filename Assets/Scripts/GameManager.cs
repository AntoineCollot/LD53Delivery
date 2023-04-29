using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public bool gameIsOver { get; private set; }
    public bool gameHasStarted { get; private set; }
    public bool GameIsPlaying => !gameIsOver && gameHasStarted;
    public bool autoStart = true;

    public UnityEvent onGameStart = new UnityEvent();
    public UnityEvent onGameOver = new UnityEvent();

    public static GameManager Instance;

    void Awake()
    {
        Instance = this;
        if(autoStart)
            StartGame();
    }

    public void StartGame()
    {
        gameHasStarted = true;
        onGameStart.Invoke();
    }

    public void GameOver()
    {
        gameIsOver = true;
        onGameOver.Invoke();
    }
}
