using UnityEngine;

public class LipSyncManager : MonoBehaviour
{
    [SerializeField] private Animator characterAnimator;  // Reference to the Animator
    [SerializeField] private OVRLipSyncContext lipSyncContext;  // Reference to the Lip Sync context (OVR Lip Sync Script)
    [SerializeField] private bool useLipSync = true;  // Enable/Disable lip-sync

    private void Start()
    {
        if (lipSyncContext == null)
        {
            lipSyncContext = GetComponent<OVRLipSyncContext>();
        }
    }

    public void PlayLipSync(AudioClip audioClip)
    {
        if (useLipSync)
        {
            lipSyncContext.audioSource.clip = audioClip;
            lipSyncContext.audioSource.Play();
        }

        // Ensure animator is running for non-facial animations
        if (characterAnimator != null)
        {
            characterAnimator.enabled = true;  // Keep the animator running
        }
    }

    public void StopLipSync()
    {
        if (lipSyncContext.audioSource.isPlaying)
        {
            lipSyncContext.audioSource.Stop();
        }

        // No need to disable animator, just let it continue
        Debug.Log("Lip-sync stopped.");
    }

    public void Update()
    {
        // You can put additional logic here to handle blending between Animator and Lip Sync
        // E.g., reduce blend weights of facial animations from Animator when lip-sync is active
    }
}
