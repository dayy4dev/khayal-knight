using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class TutorialBackstory : MonoBehaviour
{
    public static bool IsBackstoryRunning { get; private set; } = false;

    [Header("UI Komponen (Gambar & Narasi)")]
    [SerializeField] private GameObject backstoryCanvas;      // Canvas utama backstory
    [SerializeField] private Image backstoryBgImage;           // Gambar Latar/Ilustrasi yang akan berubah-ubah
    [SerializeField] private TextMeshProUGUI backstoryText;    // Teks Narasi Cerita
    [SerializeField] private Button skipButton;

    [Header("UI Gameplay Tutorial")]
    [SerializeField] private GameObject gameplayTutorialUI;    // UI asli game tutorial (HUD, dll)

    [Header("Pengaturan Cerita")]
    [SerializeField] private float textSpeed = 0.05f;         // Kecepatan mesin tik teks
    [SerializeField] private float gameplayDelay = 0.5f;       // Jeda waktu diam setelah layar terbuka lagi
    [SerializeField] private List<StoryBlockTutorial> backstorySequence; // List data Gambar + Teks

    private int currentLineIndex = 0;
    private bool isTyping = false;
    private bool hasFinished = false;

    private void Awake()
    {
        hasFinished = false;
        if (backstoryCanvas != null && backstorySequence != null && backstorySequence.Count > 0)
        {
            IsBackstoryRunning = true;
        }
        else
        {
            IsBackstoryRunning = false;
        }
    }

    private void Start()
    {
        if (skipButton == null && backstoryCanvas != null)
        {
            skipButton = backstoryCanvas.GetComponentInChildren<Button>(true);
        }
        if (skipButton != null)
        {
            skipButton.onClick.RemoveListener(EndBackstoryAndStartTutorial);
            skipButton.onClick.AddListener(EndBackstoryAndStartTutorial);
        }

        if (gameplayTutorialUI != null) gameplayTutorialUI.SetActive(false);

        if (backstoryCanvas != null && backstorySequence != null && backstorySequence.Count > 0)
        {
            backstoryCanvas.SetActive(true);
            currentLineIndex = 0;
            StartCoroutine(TypeStoryLine());
        }
        else
        {
            EndBackstoryAndStartTutorial();
        }
    }

    private void Update()
    {
        if (hasFinished) return;

        if (backstoryCanvas != null && backstoryCanvas.activeSelf)
        {
            // Escape to skip completely immediately
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                EndBackstoryAndStartTutorial();
                return;
            }

            // Klik atau spasi untuk memajukan narasi
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                OnScreenClicked();
            }
        }
    }

    private IEnumerator TypeStoryLine()
    {
        isTyping = true;
        if (backstoryText != null) backstoryText.text = "";

        if (currentLineIndex < 0 || currentLineIndex >= backstorySequence.Count)
        {
            isTyping = false;
            yield break;
        }

        StoryBlockTutorial currentBlock = backstorySequence[currentLineIndex];

        if (backstoryBgImage != null)
        {
            if (currentBlock.storyIllustration != null)
            {
                backstoryBgImage.gameObject.SetActive(true);
                backstoryBgImage.sprite = currentBlock.storyIllustration;
            }
            else
            {
                backstoryBgImage.gameObject.SetActive(false);
            }
        }

        if (backstoryText != null && !string.IsNullOrEmpty(currentBlock.dialogueText))
        {
            foreach (char letter in currentBlock.dialogueText.ToCharArray())
            {
                backstoryText.text += letter;
                yield return new WaitForSeconds(textSpeed);
            }
        }

        isTyping = false;
    }

    public void OnScreenClicked()
    {
        if (hasFinished) return;

        if (isTyping)
        {
            StopAllCoroutines();
            if (currentLineIndex < backstorySequence.Count && backstoryText != null)
            {
                backstoryText.text = backstorySequence[currentLineIndex].dialogueText;
            }
            isTyping = false;
        }
        else
        {
            currentLineIndex++;
            if (currentLineIndex < backstorySequence.Count)
            {
                StartCoroutine(TypeStoryLine());
            }
            else
            {
                EndBackstoryAndStartTutorial();
            }
        }
    }

    public void EndBackstoryAndStartTutorial()
    {
        if (hasFinished) return;
        hasFinished = true;
        IsBackstoryRunning = false;

        StopAllCoroutines();

        if (backstoryCanvas != null) backstoryCanvas.SetActive(false);

        StartCoroutine(StartGameplayWithDelay());
    }

    private IEnumerator StartGameplayWithDelay()
    {
        if (gameplayDelay > 0f)
        {
            yield return new WaitForSeconds(gameplayDelay);
        }

        if (gameplayTutorialUI != null) gameplayTutorialUI.SetActive(true);
    }

    private void OnDestroy()
    {
        IsBackstoryRunning = false;
    }
}

[System.Serializable]
public struct StoryBlockTutorial
{
    public Sprite storyIllustration;     // Kolom khusus gambar latar/suasana
    [TextArea(3, 10)]
    public string dialogueText;          // Teks cerita narasi
}
