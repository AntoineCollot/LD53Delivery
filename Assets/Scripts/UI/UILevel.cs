using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UILevel : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<TextMeshProUGUI>().text = "Level : " + LevelManager.Instance.levelConstants.levelId;
    }
}
