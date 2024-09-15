using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;  // Ensure you're using UnityEngine.UI to control buttons

public class AudioManager : MonoBehaviour
{
    private AudioRecorder audioRecorder;
    private AudioSaver audioSaver;
    private AudioUploader audioUploader;
    private AudioFetcher audioFetcher;

    [SerializeField] private AudioPlayer audioPlayer;
    [SerializeField] private AnimatorController animatorController;
    [SerializeField] private Button recordButton; // Reference to the record button

    private string directoryPath = "Recordings";
    private string serverUrl = "https://xfojojrxiv9w43-5000.proxy.runpod.net";
    private string filePath = "recording.wav";
    private string ssid = null;

    private void Awake()
    {
        resetAudio();
    }

    public void resetAudio()
    {
        Debug.Log("Initializing AudioManager...");

        audioRecorder = new AudioRecorder();
        audioSaver = new AudioSaver(directoryPath);
        audioUploader = new AudioUploader(serverUrl);
        audioFetcher = new AudioFetcher(serverUrl);

        Debug.Log("AudioManager initialized successfully.");
    }

    private void Update()
    {
        audioRecorder.UpdateRecording();
    }

    public void StartRecording()
    {
        Debug.Log("Starting recording...");
        animatorController.StartListening();
        recordButton.interactable = false;  // Disable the record button when recording starts
        audioRecorder.StartRecording();
        audioRecorder.OnSilenceExceeded += StopRecording;

        Debug.Log("Recording started and waiting for silence detection.");
    }

    public void StopRecording()
    {
        animatorController.SetIdle();
        Debug.Log("Stopping recording due to silence or user command...");

        AudioClip clip = audioRecorder.StopRecording();
        if (clip != null)
        {
            Debug.Log("Recording stopped. Saving audio...");
            audioSaver.SaveAudio(filePath, clip);
            StartCoroutine(audioUploader.UploadAudio(Path.Combine(directoryPath, filePath), OnUploadSuccess, OnUploadError));
        }
        else
        {
            Debug.LogError("Recording failed or no audio to save.");
        }
    }

    private void OnUploadSuccess(string ssid)
    {
        Debug.Log("Audio uploaded successfully. Received SSID: " + ssid);

        this.ssid = ssid;
        Debug.Log("Fetching response audio from server...");
        recordButton.interactable = false; // Disable record button while fetching and playing responses
        StartCoroutine(audioFetcher.FetchAudioFiles(ssid, OnFetchComplete));
    }

    private void OnFetchComplete()
    {
        Debug.Log("All response audio files fetched successfully.");
        Queue<string> queuedFiles = audioFetcher.GetQueuedFiles();

        if (queuedFiles.Count > 0)
        {
            Debug.Log($"Playing {queuedFiles.Count} fetched audio file(s)...");
            animatorController.gameObject.GetComponent<Animator>().enabled = false;
            StartCoroutine(PlayResponses(queuedFiles));
        }
        else
        {
            Debug.LogWarning("No audio files found to play.");
            recordButton.interactable = true;  // Re-enable record button if no responses found
        }
    }

    private IEnumerator PlayResponses(Queue<string> queuedFiles)
    {
        yield return StartCoroutine(audioPlayer.PlayQueuedAudio(queuedFiles));
        // Re-enable the record button after responses finish playing
        recordButton.interactable = true;
        //animatorController.gameObject.GetComponent<Animator>().enabled = true;
    }

    private void OnUploadError(string error)
    {
        Debug.LogError("Upload failed with error: " + error);
        recordButton.interactable = true;  // Re-enable record button in case of error
    }

    public void stopAudio()
    {
        audioPlayer.stopAudio();  // Stop audio playback
        recordButton.interactable = true;  // Re-enable record button when stop is hit
        animatorController.SetIdle();
        //animatorController.gameObject.GetComponent<Animator>().enabled = true;
        resetAudio();  // Reset the audio manager
        Debug.Log("Audio stopped. Ready for new recording.");
    }

    public void TriggerCoffeeAnimation()
    {
        animatorController.TriggerCoffeeAnimation();
    }
}
