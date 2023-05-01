using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void ToggleOnOff()
    {
        AudioSource source = GetComponent<AudioSource>();
        source.mute = !source.mute;
    }
}
