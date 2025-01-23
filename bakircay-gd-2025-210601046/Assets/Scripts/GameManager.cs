using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshPro i�in gerekli
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject restartButton;
    [SerializeField] private int totalCubeCount = 6; // Toplam k�p say�s�
    [SerializeField] private int matchesBeforeRestart = 2; // Ka� e�le�meden sonra buton ��ks�n

    private int score = 0;
    private int destroyedCubeCount = 0;
    private int matchCount = 0; // E�le�me say�s�
    private static int persistentScore = 0; // Oyun yenilense bile korunacak skor

    void Awake()
    {
        Instance = this;
        if (restartButton != null)
            restartButton.SetActive(false);

        // �nceki skorun korunmas�
        score = persistentScore;
        UpdateScoreText();
    }

    public void AddScore(int points)
    {
        score += points;
        persistentScore = score; // Skoru sakla
        UpdateScoreText();

        destroyedCubeCount += 2;
        matchCount++; // E�le�me say�s�n� art�r

        // Belirli say�da e�le�meden sonra butonu g�ster
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
        // Skoru koruyarak sahneyi yeniden y�kle
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Skoru s�f�rlamak i�in (iste�e ba�l�)
    public void ResetScore()
    {
        score = 0;
        persistentScore = 0;
        UpdateScoreText();
    }
}