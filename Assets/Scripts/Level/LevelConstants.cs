using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newLevelConstants", menuName = "ScriptableObjects/LevelConstants", order = 1)]
public class LevelConstants : ScriptableObject
{
    [Header("Level")]
    public Color backgroundElementsColor;

    [Header("Black Cats")]
    [Range(0,1)] public float blackCatRollIntervalMult = 1;
    [Range(1,10)] public float blackCatRollDurationMult = 1;
    [Range(1,10)] public float blackCatRollSpeedMult = 1;

    [Header("Eagles")]
    [Range(0, 1)] public float eagleFireIntervalMult = 1;
    [Range(1, 10)] public float eagleProjectileSpeedMult = 1;

    [Header("Score")]
    public int enemyCount;

}
