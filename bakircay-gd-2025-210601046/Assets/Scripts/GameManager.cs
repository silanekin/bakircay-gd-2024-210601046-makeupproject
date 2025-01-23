using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshPro için gerekli
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject restartButton;
    [SerializeField] private int totalCubeCount = 6; // Toplam küp sayýsý
    [SerializeField] private int matchesBeforeRestart = 2; // Kaç eþleþmeden sonra buton çýksýn

    private int score = 0;
    private int destroyedCubeCount = 0;
    private int matchCount = 0; // Eþleþme sayýsý
    private static int persistentScore = 0; // Oyun yenilense bile korunacak skor

    void Awake()
    {
        Instance = this;
        if (restartButton != null)
            restartButton.SetActive(false);

        // Önceki skorun korunmasý
        score = persistentScore;
        UpdateScoreText();
    }

    public void AddScore(int points)
    {
        score += points;
        persistentScore = score; // Skoru sakla
        UpdateScoreText();

        destroyedCubeCount += 2;
        matchCount++; // Eþleþme sayýsýný artýr

        // Belirli sayýda eþleþmeden sonra butonu göster
        if (matchCount >= matchesBeforeRestart)
        {
            ShowRestartButton();
        }
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Skor: {score}";
        }
    }

    private void ShowRestartButton()
    {
        if (restartButton != null)
        {
            restartButton.SetActive(true);
        }
    }

    public void RestartGame()
    {
        // Skoru koruyarak sahneyi yeniden yükle
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Skoru sýfýrlamak için (isteðe baðlý)
    public void ResetScore()
    {
        score = 0;
        persistentScore = 0;
        UpdateScoreText();
    }
}