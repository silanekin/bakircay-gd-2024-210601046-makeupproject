using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindSkill : MonoBehaviour
{
    public float pullForce = 10f; // Nesneleri hortuma çeken kuvvet
    public float spinSpeed = 360f; // Nesnelerin hortum çevresinde dönme hýzý
    public float duration = 3f; // Hortum etkisinin süresi
    public float releaseRadius = 5f; // Nesnelerin býrakýlacaðý alanýn yarýçapý
    public Transform vortexCenter; // Hortumun merkezi
    public ParticleSystem vortexEffect; // Hortumun görsellik efekti

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

        // Hortum efektini baþlat
        if (vortexEffect != null)
        {
            vortexEffect.Play();
        }

        // Sahnedeki tüm Draggable nesneleri bulun
        GameObject[] draggableObjects = GameObject.FindGameObjectsWithTag("Draggable");

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            foreach (GameObject obj in draggableObjects)
            {
                if (obj == null) continue; // Nesne yok edilmiþse atla

                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // Nesneleri hortumun merkezine doðru çek
                    Vector3 directionToCenter = (vortexCenter.position - obj.transform.position).normalized;
                    rb.AddForce(directionToCenter * pullForce);

                    // Nesneleri hortum çevresinde döndür
                    obj.transform.RotateAround(vortexCenter.position, Vector3.up, spinSpeed * Time.deltaTime);
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Nesneleri rastgele bir noktaya býrak
        foreach (GameObject obj in draggableObjects)
        {
            if (obj == null) continue; // Nesne yok edilmiþse atla

            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Rastgele bir noktaya býrak
                Vector3 randomPosition = vortexCenter.position + Random.insideUnitSphere * releaseRadius;
                randomPosition.y = 0.5f; // Yerde kalmasý için Y koordinatýný ayarla
                rb.velocity = Vector3.zero; // Mevcut hareketi sýfýrla
                rb.angularVelocity = Vector3.zero; // Döndürmeyi sýfýrla
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
