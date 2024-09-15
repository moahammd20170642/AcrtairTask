using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AudioPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    private bool stopRequested = false;

    public void stopAudio()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop(); // Stop currently playing audio
        }
        stopRequested = true;  // Signal to stop queued playback
    }

    public IEnumerator PlayQueuedAudio(Queue<string> audioFileQueue)
    {
        int index = 0;

        while (audioFileQueue.Count > 0)
        {
            if (stopRequested)
            {
                stopRequested = false; // Reset stop flag for future use
                yield break;  // Exit the coroutine and stop playback
            }

            string filePath = audioFileQueue.Dequeue();
            AudioType audioType = (index == 0 || index == 1) ? AudioType.WAV : AudioType.MPEG;
            string fullPath = "file://" + filePath;

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fullPath, audioType))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    audioSource.clip = clip;
                    audioSource.Play();
                    yield return new WaitForSeconds(clip.length);
                }
                else
                {
                    Debug.LogError("Failed to download audio clip: " + www.error);
                }
            }

            index++;
        }
    }
}
