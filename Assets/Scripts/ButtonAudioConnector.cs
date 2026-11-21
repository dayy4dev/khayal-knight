using UnityEngine;
using UnityEngine.UI;

public class ButtonAudioController : MonoBehaviour
{
    [Header("Komponen Audio")]
    public AudioSource audioSource;
    public AudioClip clickSound;

    private void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource != null) audioSource.ignoreListenerPause = true;
    }

    // Fungsi ini akan dipanggil saat tombol ditekan
    public void PlaySound()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        // Mengecek apakah AudioSource dan AudioClip sudah terisi
        if (audioSource != null && clickSound != null)
        {
            audioSource.ignoreListenerPause = true;
            // Memainkan suara satu kali
            audioSource.PlayOneShot(clickSound);
        }
        else
        {
            Debug.LogWarning("AudioSource atau AudioClip belum dimasukkan di Inspector!");
        }
    }
}
