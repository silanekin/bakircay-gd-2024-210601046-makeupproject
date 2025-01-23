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

    // Mevcut Header'larýn altýna ekleyin
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
    [SerializeField] private float comboTimeout = 5f; // Combo süresi
    [SerializeField] private int comboBonus = 5; // Her combo için bonus puan
    private int comboCount = 0; // Mevcut combo sayýsý
    private float comboTimer = 0f; // Combo zamanlayýcýsý
    [SerializeField] private TextMeshProUGUI comboText; // Combo göstergesi

    void Start()
    {
        areaRenderer = GetComponent<Renderer>();
        SetAreaColor(new Color(1, 0, 0, 0.5f));
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Draggable") || isProcessingMatch) return;

        string newCubeColor = GetCubeColor(other.gameObject);
        Debug.Log($"Küp alana girdi: {other.gameObject.name}, Renk: {newCubeColor}");

        // Alan boþsa
        if (placedCubes.Count == 0)
        {
            AddCube(other.gameObject, newCubeColor);
        }
        // Alan doluysa ve gelen küp farklý renkse
        else if (newCubeColor != firstCubeColor)
        {
            Debug.Log($"Farklý renk tespit edildi. Beklenen: {firstCubeColor}, Gelen: {newCubeColor}");
            StartCoroutine(RejectCube(other.gameObject));
        }
        // Ayný renk küp geliyorsa
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

            // Ýlk küp eklendiðinde eþlerini vurgula
            HighlightMatchingCubes(color);
        }

        placedCubes.Add(cube);
        Debug.Log($"Küp eklendi: {cube.name}, Toplam küp: {placedCubes.Count}");
    }

    private string GetCubeColor(GameObject obj)
    {
        string objName = obj.name.ToLower();

        // Küpler için
        if (objName.Contains("cube"))
        {
            if (objName.Contains("blue")) return "BlueCube";
            if (objName.Contains("red")) return "RedCube";
            if (objName.Contains("green")) return "GreenCube";
            if (objName.Contains("yellow")) return "YellowCube";
        }

        // Kapsüller için
        if (objName.Contains("capsule"))
        {
            if (objName.Contains("pink")) return "PinkCapsule";
        }

        // Daireler için
        if (objName.Contains("sphere") || objName.Contains("circle"))
        {
            if (objName.Contains("orange")) return "OrangeSphere";
            if (objName.Contains("purple")) return "PurpleSphere";
        }

        return "";
    }

    private IEnumerator RejectCube(GameObject cube)
    {
        // Baþlangýç pozisyonunu kaydet
        Vector3 startPosition = cube.transform.position;

        // Ýtme yönünü hesapla
        Vector3 directionFromCenter = (cube.transform.position - transform.position).normalized;
        directionFromCenter.y = 0; // Y eksenini sýfýrla

        // Hedef pozisyonu hesapla (daha uzaða)
        Vector3 targetPosition = transform.position + (directionFromCenter * rejectDistance * 2f);
        targetPosition.y = startPosition.y; // Y pozisyonunu koru

        // Rigidbody'yi geçici olarak devre dýþý býrak
        Rigidbody rb = cube.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Hýzlý itme hareketi
        float elapsedTime = 0;
        float duration = 0.2f; // Daha hýzlý itme

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;

            // Daha güçlü bir itme eðrisi
            float smoothProgress = 1f - Mathf.Pow(1f - progress, 2f);
            cube.transform.position = Vector3.Lerp(startPosition, targetPosition, smoothProgress);

            yield return null;
        }

        // Son pozisyonu kesin olarak ayarla
        cube.transform.position = targetPosition;

        // Rigidbody'yi tekrar etkinleþtir ve bir itme kuvveti uygula
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
            Debug.Log($"Eþleþme bulundu! Renk: {firstCubeColor}");
            StartCoroutine(DestroyMatchedCubes(matchingCubes[0], matchingCubes[1]));
        }
    }

    private IEnumerator DestroyMatchedCubes(GameObject cube1, GameObject cube2)
    {
        ClearHighlights(); // Vurgulamalarý temizle
        isProcessingMatch = true;


        Debug.Log("Eþleþen küpler animasyonla yok ediliyor...");

        // Rigidbody'leri devre dýþý býrak
        if (cube1.TryGetComponent<Rigidbody>(out Rigidbody rb1))
            rb1.isKinematic = true;
        if (cube2.TryGetComponent<Rigidbody>(out Rigidbody rb2))
            rb2.isKinematic = true;

        // Orijinal özellikleri kaydet
        Vector3 originalScale1 = cube1.transform.localScale;
        Vector3 originalScale2 = cube2.transform.localScale;
        Material mat1 = cube1.GetComponent<Renderer>().material;
        Material mat2 = cube2.GetComponent<Renderer>().material;
        Color originalColor1 = mat1.color;
        Color originalColor2 = mat2.color;

        float elapsed = 0f;

        // Büyüme ve dönme animasyonu
        while (elapsed < matchAnimDuration / 2)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (matchAnimDuration / 2);

            // Yumuþak geçiþ için easing
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

        // Küçülme ve kaybolma animasyonu
        while (elapsed < matchAnimDuration)
        {
            elapsed += Time.deltaTime;
            float progress = (elapsed - matchAnimDuration / 2) / (matchAnimDuration / 2);

            // Yumuþak geçiþ
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

        /// Combo sistemini güncelle
        comboCount++;
        comboTimer = 0f;

        // Toplam skoru hesapla
        int baseScore = scorePerMatch;
        int bonusScore = (comboCount - 1) * comboBonus;
        int totalScore = baseScore + bonusScore;

        // Debug.Log ile combo bilgisini göster
        Debug.Log($"Combo x{comboCount}! Base: {baseScore}, Bonus: {bonusScore}, Total: {totalScore}");

        // Skoru ekle
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(totalScore);
        }

        UpdateComboText(); // Combo text'ini güncelle

        // Skor ekle
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scorePerMatch);
        }

        // Küpleri yok et
        placedCubes.Remove(cube1);
        placedCubes.Remove(cube2);
        Destroy(cube1);
        Destroy(cube2);

        // Alan durumunu güncelle
        if (placedCubes.Count == 0)
        {
            firstCubeColor = "";
            SetAreaColor(new Color(1, 0, 0, 0.5f));
        }

        isProcessingMatch = false;
        Debug.Log($"Eþleþme tamamlandý. Kalan küp sayýsý: {placedCubes.Count}");
    }



    // Eþleþen küpleri vurgulama
    private void HighlightMatchingCubes(string colorToMatch)
    {
        ClearHighlights();

        GameObject[] allObjects = GameObject.FindGameObjectsWithTag("Draggable");

        foreach (GameObject obj in allObjects)
        {
            // Sadece ayný tip ve renkteki objeleri vurgula
            string objColor = GetCubeColor(obj);
            if (!placedCubes.Contains(obj) && objColor == colorToMatch)
            {
                HighlightCube(obj);
                highlightedCubes.Add(obj);
            }
        }
    }

    // Tek bir küpü vurgulama
    private void HighlightCube(GameObject cube)
    {
        Renderer renderer = cube.GetComponent<Renderer>();
        if (renderer != null)
        {
            // Orijinal materyali yedekle
            Material originalMaterial = renderer.material;

            // Parlaklýðý artýr
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", highlightColor * highlightIntensity);
        }
    }

    // Vurgulamalarý temizle
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
            Debug.Log($"Küp alandan çýktý: {other.gameObject.name}");

            if (placedCubes.Count == 0)
            {
                firstCubeColor = "";
                SetAreaColor(new Color(1, 0, 0, 0.5f));
                ClearHighlights(); // Vurgulamalarý temizle
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
        ClearHighlights(); // Script yok edildiðinde vurgulamalarý temizle
    }

    private void Update()
    {
        // Eðer aktif bir combo varsa
        if (comboCount > 0)
        {
            comboTimer += Time.deltaTime;

            // Combo süresi doldu mu?
            if (comboTimer > comboTimeout)
            {
                ResetCombo();
            }

            // Combo text'ini güncelle
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

