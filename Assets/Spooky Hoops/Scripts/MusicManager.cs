using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private AudioSource musicPlayer;
    void Start()
    {
        musicPlayer = GetComponent<AudioSource>();
        RestartSound();
    }

    void FixedUpdate()
    {
        if(musicPlayer.isPlaying == false)
        {
            RestartSound();
        }
    }

    private void RestartSound()
    {
        musicPlayer.Play();
    }
}
