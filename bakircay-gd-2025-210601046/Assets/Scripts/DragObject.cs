using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class DragObject : MonoBehaviour
{
    private Vector3 offset;
    private float zCoordinate;
    private Rigidbody rb;
    private bool isDragging = false;
    private Vector3 lastValidPosition;
    public bool IsDragging { get; private set; }


    [Header("Drag Settings")]
    [SerializeField] private float dragHeight = 0.5f; // S�r�kleme y�ksekli�i
    [SerializeField] private float returnSpeed = 10f; // Ge�ersiz konuma d�n�� h�z�

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        lastValidPosition = transform.position; // Ba�lang�� pozisyonunu kaydet
    }

    void OnMouseDown()
    {
        IsDragging = true;
        if (rb == null) return;

        isDragging = true;
        zCoordinate = Camera.main.WorldToScreenPoint(transform.position).z;
        offset = transform.position - GetMouseWorldPosition();

        // S�r�kleme ba�lad���nda objeyi yukar� kald�r
        Vector3 newPosition = transform.position;
        newPosition.y = dragHeight;
        transform.position = newPosition;
    }

    void OnMouseDrag()
    {
        IsDragging = false;
        if (!isDragging || rb == null) return;

        Vector3 targetPosition = GetMouseWorldPosition() + offset;
        targetPosition.y = dragHeight; // Sabit y�kseklikte tut
        transform.position = targetPosition;
    }

    void OnMouseUp()
    {
        if (rb == null) return;

        isDragging = false;

        // Placement area kontrol�
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 0.5f);
        bool isInValidArea = false;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.GetComponent<PlacementArea>() != null)
            {
                lastValidPosition = transform.position;
                lastValidPosition.y = 0.5f; // Placement area y�ksekli�i
                isInValidArea = true;
                break;
            }
        }

        if (!isInValidArea)
        {
            // Ge�erli bir alana b�rak�lmad�ysa son ge�erli pozisyona geri d�n
            StartCoroutine(ReturnToLastValidPosition());
        }
        else
        {
            transform.position = lastValidPosition;
        }
    }

    private System.Collections.IEnumerator ReturnToLastValidPosition()
    {
        float elapsedTime = 0;
        Vector3 startPosition = transform.position;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * returnSpeed;
            transform.position = Vector3.Lerp(startPosition, lastValidPosition, elapsedTime);
            yield return null;
        }

        transform.position = lastValidPosition;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = zCoordinate;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    void OnTriggerEnter(Collider other)
    {
        // Placement area'ya girdi�inde pozisyonu kaydet
        if (other.GetComponent<PlacementArea>() != null)
        {
            Vector3 newPosition = transform.position;
            newPosition.y = 0.5f;
            lastValidPosition = newPosition;
        }
    }
}