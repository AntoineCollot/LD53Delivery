using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music : MonoBehaviour
{
    public static Music Instance;
    // Start is called before the first frame update
    void Start()
    {
        if(Instance!=null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ToggleOnOff()
    {
        AudioSource source = GetComponent<AudioSource>();
        source.mute = !source.mute;
    }
}
