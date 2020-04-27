using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadScreenSoundControl : MonoBehaviour
{
    public AudioManager audioManager;
    

    // Update is called once per frame
    void Update()
    {
        if (gameObject.activeSelf)
        {
            audioManager.StopAllSounds();
        }
    }
}
