using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;


public class PlacementArea : MonoBehaviour
{
    private List<GameObject> placedCubes = new List<GameObject>();
    private Renderer areaRenderer;
    private int scorePerMatch = 10;
    private string firstCubeColor = "";
    private bool isProcessingMatch = false;

    [Header("Rejection Settings")]
    [SerializeField] private float rejectDistance = 2f;
    [SerializeField] private float moveSpeed = 5f;

    // Mevcut Header'lar�n alt�na ekleyin
    [Header("Match Animation Settings")]
    [SerializeField] private float matchAnimDuration = 1f;
    [SerializeField] private float scaleMultiplier = 1.3f;
    [SerializeField] private float rotateSpeed = 360f;
    [SerializeField] private Color matchColor = Color.yellow;

    [Header("Highlight Settings")]
    [SerializeField] private Color highlightColor = new Color(1f, 1f, 1f, 0.6f);
    [SerializeField] private float highlightIntensity = 1.5f;
    private List<GameObject> highlightedCubes = new List<GameObject>();

    [Header("Combo Settings")]
    [SerializeField] private float comboTimeout = 5f; // Combo s�resi
    [SerializeField] private int comboBonus = 5; // Her combo i�in bonus puan
    private int comboCount = 0; // Mevcut combo say�s�
    private float comboTimer = 0f; // Combo zamanlay�c�s�
    [SerializeField] private TextMeshProUGUI comboText; // Combo g�stergesi

    void Start()
    {
        areaRenderer = GetComponent<Renderer>();
        SetAreaColor(new Color(1, 0, 0, 0.5f));
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Draggable") || isProcessingMatch) return;

        string newCubeColor = GetCubeColor(other.gameObject);
        Debug.Log($"K�p alana girdi: {other.gameObject.name}, Renk: {newCubeColor}");

        // Alan bo�sa
        if (placedCubes.Count == 0)
        {
            AddCube(other.gameObject, newCubeColor);
        }
        // Alan doluysa ve gelen k�p farkl� renkse
        else if (newCubeColor != firstCubeColor)
        {
            Debug.Log($"Farkl� renk tespit edildi. Beklenen: {firstCubeColor}, Gelen: {newCubeColor}");
            StartCoroutine(RejectCube(other.gameObject));
        }
        // Ayn� renk k�p geliyorsa
        else if (!placedCubes.Contains(other.gameObject))
        {
            AddCube(other.gameObject, newCubeColor);
            CheckForMatches();
        }
    }
    private void AddCube(GameObject cube, string color)
    {
        if (placedCubes.Count == 0)
        {
            firstCubeColor = color;
            SetAreaColor(new Color(0, 1, 0, 0.5f));

            // �lk k�p eklendi�inde e�lerini vurgula
            HighlightMatchingCubes(color);
        }

        placedCubes.Add(cube);
        Debug.Log($"K�p eklendi: {cube.name}, Toplam k�p: {placedCubes.Count}");
    }

    private string GetCubeColor(GameObject obj)
    {
        string objName = obj.name.ToLower();

        // K�pler i�in
        if (objName.Contains("cube"))
        {
            if (objName.Contains("blue")) return "BlueCube";
            if (objName.Contains("red")) return "RedCube";
            if (objName.Contains("green")) return "GreenCube";
            if (objName.Contains("yellow")) return "YellowCube";
        }

        // Kaps�ller i�in
        if (objName.Contains("capsule"))
        {
            if (objName.Contains("pink")) return "PinkCapsule";
        }

        // Daireler i�in
        if (objName.Contains("sphere") || objName.Contains("circle"))
        {
            if (objName.Contains("orange")) return "OrangeSphere";
            if (objName.Contains("purple")) return "PurpleSphere";
        }

        return "";
    }

    private IEnumerator RejectCube(GameObject cube)
    {
        // Ba�lang�� pozisyonunu kaydet
        Vector3 startPosition = cube.transform.position;

        // �tme y�n�n� hesapla
        Vector3 directionFromCenter = (cube.transform.position - transform.position).normalized;
        directionFromCenter.y = 0; // Y eksenini s�f�rla

        // Hedef pozisyonu hesapla (daha uza�a)
        Vector3 targetPosition = transform.position + (directionFromCenter * rejectDistance * 2f);
        targetPosition.y = startPosition.y; // Y pozisyonunu koru

        // Rigidbody'yi ge�ici olarak devre d��� b�rak
        Rigidbody rb = cube.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // H�zl� itme hareketi
        float elapsedTime = 0;
        float duration = 0.2f; // Daha h�zl� itme

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;

            // Daha g��l� bir itme e�risi
            float smoothProgress = 1f - Mathf.Pow(1f - progress, 2f);
            cube.transform.position = Vector3.Lerp(startPosition, targetPosition, smoothProgress);

            yield return null;
        }

        // Son pozisyonu kesin olarak ayarla
        cube.transform.position = targetPosition;

        // Rigidbody'yi tekrar etkinle�tir ve bir itme kuvveti uygula
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(directionFromCenter * 5f, ForceMode.Impulse);
        }

        placedCubes.Remove(cube);
    }

    private void CheckForMatches()
    {
        if (placedCubes.Count < 2) return;

        List<GameObject> matchingCubes = new List<GameObject>();

        foreach (var cube in placedCubes)
        {
            if (GetCubeColor(cube) == firstCubeColor)
            {
                matchingCubes.Add(cube);
                if (matchingCubes.Count == 2) break;
            }
        }

        if (matchingCubes.Count >= 2)
        {
            Debug.Log($"E�le�me bulundu! Renk: {firstCubeColor}");
            StartCoroutine(DestroyMatchedCubes(matchingCubes[0], matchingCubes[1]));
        }
    }

    private IEnumerator DestroyMatchedCubes(GameObject cube1, GameObject cube2)
    {
        ClearHighlights(); // Vurgulamalar� temizle
        isProcessingMatch = true;


        Debug.Log("E�le�en k�pler animasyonla yok ediliyor...");

        // Rigidbody'leri devre d��� b�rak
        if (cube1.TryGetComponent<Rigidbody>(out Rigidbody rb1))
            rb1.isKinematic = true;
        if (cube2.TryGetComponent<Rigidbody>(out Rigidbody rb2))
            rb2.isKinematic = true;

        // Orijinal �zellikleri kaydet
        Vector3 originalScale1 = cube1.transform.localScale;
        Vector3 originalScale2 = cube2.transform.localScale;
        Material mat1 = cube1.GetComponent<Renderer>().material;
        Material mat2 = cube2.GetComponent<Renderer>().material;
        Color originalColor1 = mat1.color;
        Color originalColor2 = mat2.color;

        float elapsed = 0f;

        // B�y�me ve d�nme animasyonu
        while (elapsed < matchAnimDuration / 2)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (matchAnimDuration / 2);

            // Yumu�ak ge�i� i�in easing
            float easedProgress = 1f - Mathf.Cos(progress * Mathf.PI * 0.5f);

            // Scale
            float scale = 1f + (scaleMultiplier - 1f) * easedProgress;
            cube1.transform.localScale = originalScale1 * scale;
            cube2.transform.localScale = originalScale2 * scale;

            // Rotation
            float rotation = easedProgress * rotateSpeed;
            cube1.transform.Rotate(Vector3.up, rotation * Time.deltaTime);
            cube2.transform.Rotate(Vector3.up, rotation * Time.deltaTime);

            // Color lerp
            Color lerpColor = Color.Lerp(originalColor1, matchColor, easedProgress);
            mat1.color = lerpColor;
            mat2.color = lerpColor;

            yield return null;
        }

        // K���lme ve kaybolma animasyonu
        while (elapsed < matchAnimDuration)
        {
            elapsed += Time.deltaTime;
            float progress = (elapsed - matchAnimDuration / 2) / (matchAnimDuration / 2);

            // Yumu�ak ge�i�
            float easedProgress = Mathf.Sin(progress * Mathf.PI * 0.5f);

            // Scale down
            float scale = scaleMultiplier * (1f - easedProgress);
            cube1.transform.localScale = originalScale1 * scale;
            cube2.transform.localScale = originalScale2 * scale;

            // Fade out
            Color fadeColor1 = mat1.color;
            Color fadeColor2 = mat2.color;
            fadeColor1.a = 1f - easedProgress;
            fadeColor2.a = 1f - easedProgress;
            mat1.color = fadeColor1;
            mat2.color = fadeColor2;

            // Continue rotation
            cube1.transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
            cube2.transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);

            yield return null;
        }

        /// Combo sistemini g�ncelle
        comboCount++;
        comboTimer = 0f;

        // Toplam skoru hesapla
        int baseScore = scorePerMatch;
        int bonusScore = (comboCount - 1) * comboBonus;
        int totalScore = baseScore + bonusScore;

        // Debug.Log ile combo bilgisini g�ster
        Debug.Log($"Combo x{comboCount}! Base: {baseScore}, Bonus: {bonusScore}, Total: {totalScore}");

        // Skoru ekle
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(totalScore);
        }

        UpdateComboText(); // Combo text'ini g�ncelle

        // Skor ekle
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scorePerMatch);
        }

        // K�pleri yok et
        placedCubes.Remove(cube1);
        placedCubes.Remove(cube2);
        Destroy(cube1);
        Destroy(cube2);

        // Alan durumunu g�ncelle
        if (placedCubes.Count == 0)
        {
            firstCubeColor = "";
            SetAreaColor(new Color(1, 0, 0, 0.5f));
        }

        isProcessingMatch = false;
        Debug.Log($"E�le�me tamamland�. Kalan k�p say�s�: {placedCubes.Count}");
    }



    // E�le�en k�pleri vurgulama
    private void HighlightMatchingCubes(string colorToMatch)
    {
        ClearHighlights();

        GameObject[] allObjects = GameObject.FindGameObjectsWithTag("Draggable");

        foreach (GameObject obj in allObjects)
        {
            // Sadece ayn� tip ve renkteki objeleri vurgula
            string objColor = GetCubeColor(obj);
            if (!placedCubes.Contains(obj) && objColor == colorToMatch)
            {
                HighlightCube(obj);
                highlightedCubes.Add(obj);
            }
        }
    }

    // Tek bir k�p� vurgulama
    private void HighlightCube(GameObject cube)
    {
        Renderer renderer = cube.GetComponent<Renderer>();
        if (renderer != null)
        {
            // Orijinal materyali yedekle
            Material originalMaterial = renderer.material;

            // Parlakl��� art�r
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", highlightColor * highlightIntensity);
        }
    }

    // Vurgulamalar� temizle
    private void ClearHighlights()
    {
        foreach (GameObject cube in highlightedCubes)
        {
            if (cube != null)
            {
                Renderer renderer = cube.GetComponent<Renderer>();
                if (renderer != null)
                {
                    // Emisyonu kapat
                    renderer.material.DisableKeyword("_EMISSION");
                    renderer.material.SetColor("_EmissionColor", Color.black);
                }
            }
        }
        highlightedCubes.Clear();
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Draggable") && !isProcessingMatch)
        {
            placedCubes.Remove(other.gameObject);
            Debug.Log($"K�p alandan ��kt�: {other.gameObject.name}");

            if (placedCubes.Count == 0)
            {
                firstCubeColor = "";
                SetAreaColor(new Color(1, 0, 0, 0.5f));
                ClearHighlights(); // Vurgulamalar� temizle
            }
        }
    }

   
private void SetAreaColor(Color color)
    {
        if (areaRenderer && areaRenderer.material)
        {
            areaRenderer.material.color = color;
        }
    }
    private void OnDestroy()
    {
        ClearHighlights(); // Script yok edildi�inde vurgulamalar� temizle
    }

    private void Update()
    {
        // E�er aktif bir combo varsa
        if (comboCount > 0)
        {
            comboTimer += Time.deltaTime;

            // Combo s�resi doldu mu?
            if (comboTimer > comboTimeout)
            {
                ResetCombo();
            }

            // Combo text'ini g�ncelle
            UpdateComboText();
        }
    }

    private void ResetCombo()
    {
        comboCount = 0;
        comboTimer = 0f;
        UpdateComboText();
    }

    private void UpdateComboText()
    {
        if (comboText != null)
        {
            if (comboCount > 1)
            {
                comboText.text = $"Combo x{comboCount}!";
                comboText.gameObject.SetActive(true);
            }
            else
            {
                comboText.gameObject.SetActive(false);
            }
        }
    }
}

