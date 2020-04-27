using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSoundManager : MonoBehaviour
{
    public AudioClip audioClip;
    public Transform soundPosition;

    void Start()
    {
        AudioSource.PlayClipAtPoint(audioClip, new Vector3(soundPosition.position.x, soundPosition.position.y, soundPosition.position.z), 1);
        AudioSource.PlayClipAtPoint(audioClip, new Vector3(soundPosition.position.x, soundPosition.position.y, soundPosition.position.z), 1);
    }   
}
