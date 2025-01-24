using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpSkill : MonoBehaviour
{
    public float jumpForce = 1000f; // Z�plama kuvveti
    public float cooldownTime = 5f; // Yetenek bekleme s�resi
    private bool isCooldown = false; // Cooldown kontrol�

    public void ActivateJumpSkill()
    {
        if (isCooldown) return; // E�er yetenek cooldown'daysa �al��maz

        Debug.Log("Jump Skill aktif!");
        StartCoroutine(ApplyJumpEffect());
    }

    private IEnumerator ApplyJumpEffect()
    {
        // Sahnedeki t�m Draggable nesneleri bulun
        GameObject[] draggableObjects = GameObject.FindGameObjectsWithTag("Draggable");

        if (draggableObjects.Length == 0)
        {
            Debug.LogWarning("Jump Skill: Sahnede 'Draggable' nesne bulunamad�!");
            yield break;
        }

        foreach (GameObject obj in draggableObjects)
        {
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false; // Kinematik mod devre d���
                rb.velocity = Vector3.zero; // Mevcut h�z�n� s�f�rla
                rb.angularVelocity = Vector3.zero; // Mevcut d�nme h�z�n� s�f�rla

                Debug.Log($"Jump kuvveti uygulan�yor: {obj.name}");
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); // Z�plama kuvvetini uygula
            }
            else
            {
                Debug.LogWarning($"Jump Skill: {obj.name} �zerinde Rigidbody bulunamad�!");
            }
        }

        // Cooldown ba�lat
        StartCoroutine(StartCooldown());
        yield return null;
    }

    private IEnumerator StartCooldown()
    {
        isCooldown = true;
        Debug.Log("Jump Skill cooldown ba�lad�...");
        yield return new WaitForSeconds(cooldownTime); // Cooldown s�resi kadar bekle
        isCooldown = false;
        Debug.Log("Jump Skill tekrar kullan�labilir.");
    }
}
