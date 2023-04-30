using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UITimer : MonoBehaviour
{
    TextMeshProUGUI text;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        TimeSpan span = TimeSpan.FromSeconds((double)(new decimal(ScoreSystem.Instance.levelTime)));
        text.text = span.ToString(@"mm\:ss\:ff");
    }
}
