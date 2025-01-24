using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpSkill : MonoBehaviour
{
    public float jumpForce = 1000f; // Zýplama kuvveti
    public float cooldownTime = 5f; // Yetenek bekleme süresi
    private bool isCooldown = false; // Cooldown kontrolü

    public void ActivateJumpSkill()
    {
        if (isCooldown) return; // Eðer yetenek cooldown'daysa çalýþmaz

        Debug.Log("Jump Skill aktif!");
        StartCoroutine(ApplyJumpEffect());
    }

    private IEnumerator ApplyJumpEffect()
    {
        // Sahnedeki tüm Draggable nesneleri bulun
        GameObject[] draggableObjects = GameObject.FindGameObjectsWithTag("Draggable");

        if (draggableObjects.Length == 0)
        {
            Debug.LogWarning("Jump Skill: Sahnede 'Draggable' nesne bulunamadý!");
            yield break;
        }

        foreach (GameObject obj in draggableObjects)
        {
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false; // Kinematik mod devre dýþý
                rb.velocity = Vector3.zero; // Mevcut hýzýný sýfýrla
                rb.angularVelocity = Vector3.zero; // Mevcut dönme hýzýný sýfýrla

                Debug.Log($"Jump kuvveti uygulanýyor: {obj.name}");
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); // Zýplama kuvvetini uygula
            }
            else
            {
                Debug.LogWarning($"Jump Skill: {obj.name} üzerinde Rigidbody bulunamadý!");
            }
        }

        // Cooldown baþlat
        StartCoroutine(StartCooldown());
        yield return null;
    }

    private IEnumerator StartCooldown()
    {
        isCooldown = true;
        Debug.Log("Jump Skill cooldown baþladý...");
        yield return new WaitForSeconds(cooldownTime); // Cooldown süresi kadar bekle
        isCooldown = false;
        Debug.Log("Jump Skill tekrar kullanýlabilir.");
    }
}
