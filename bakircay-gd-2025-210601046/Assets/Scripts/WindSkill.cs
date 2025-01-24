using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindSkill : MonoBehaviour
{
    public float pullForce = 10f; // Nesneleri hortuma �eken kuvvet
    public float spinSpeed = 360f; // Nesnelerin hortum �evresinde d�nme h�z�
    public float duration = 3f; // Hortum etkisinin s�resi
    public float releaseRadius = 5f; // Nesnelerin b�rak�laca�� alan�n yar��ap�
    public Transform vortexCenter; // Hortumun merkezi
    public ParticleSystem vortexEffect; // Hortumun g�rsellik efekti

    private bool isActive = false;

    public void ActivateWindSkill()
    {
        if (isActive) return;

        Debug.Log("Hortum etkisi aktif!");
        StartCoroutine(VortexEffect());
    }

    private IEnumerator VortexEffect()
    {
        isActive = true;

        // Hortum efektini ba�lat
        if (vortexEffect != null)
        {
            vortexEffect.Play();
        }

        // Sahnedeki t�m Draggable nesneleri bulun
        GameObject[] draggableObjects = GameObject.FindGameObjectsWithTag("Draggable");

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            foreach (GameObject obj in draggableObjects)
            {
                if (obj == null) continue; // Nesne yok edilmi�se atla

                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // Nesneleri hortumun merkezine do�ru �ek
                    Vector3 directionToCenter = (vortexCenter.position - obj.transform.position).normalized;
                    rb.AddForce(directionToCenter * pullForce);

                    // Nesneleri hortum �evresinde d�nd�r
                    obj.transform.RotateAround(vortexCenter.position, Vector3.up, spinSpeed * Time.deltaTime);
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Nesneleri rastgele bir noktaya b�rak
        foreach (GameObject obj in draggableObjects)
        {
            if (obj == null) continue; // Nesne yok edilmi�se atla

            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Rastgele bir noktaya b�rak
                Vector3 randomPosition = vortexCenter.position + Random.insideUnitSphere * releaseRadius;
                randomPosition.y = 0.5f; // Yerde kalmas� i�in Y koordinat�n� ayarla
                rb.velocity = Vector3.zero; // Mevcut hareketi s�f�rla
                rb.angularVelocity = Vector3.zero; // D�nd�rmeyi s�f�rla
                obj.transform.position = randomPosition;
            }
        }

        // Hortum efektini durdur
        if (vortexEffect != null)
        {
            vortexEffect.Stop();
        }

        Debug.Log("Hortum etkisi sona erdi.");
        isActive = false;
    }
}
