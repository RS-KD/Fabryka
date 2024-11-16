using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f; // Prędkość ruchu kamery
    public float rotationSpeed = 1000f; // Prędkość obrotu kamery

    private float pitch = 0f; // Kąt pochylenia kamery (góra-dół)
    private float yaw = 0f; // Kąt obrotu kamery (lewo-prawo)

    void Update()
    {
        HandleMovement();
        HandleRotation();
    }

    void HandleMovement()
    {
        // Pobierz wejścia od użytkownika
        float horizontal = Input.GetAxis("Horizontal"); // Klawisze A/D
        float vertical = Input.GetAxis("Vertical"); // Klawisze W/S

        // Oblicz ruch w lokalnym układzie współrzędnych kamery
        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;

        // Przesuń kamerę
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    void HandleRotation()
    {
        // Sprawdź, czy prawy przycisk myszy jest wciśnięty
        if (Input.GetMouseButton(1))
        {
            // Pobierz ruch myszy
            float mouseX = Input.GetAxis("Mouse X"); // Ruch w osi X
            float mouseY = Input.GetAxis("Mouse Y"); // Ruch w osi Y

            // Zaktualizuj kąty obrotu
            yaw += mouseX * rotationSpeed * Time.deltaTime;
            pitch -= mouseY * rotationSpeed * Time.deltaTime;

            // Ogranicz kąt pochylenia kamery, aby uniknąć dziwnych efektów
            pitch = Mathf.Clamp(pitch, -90f, 90f);

            // Ustaw obrót kamery
            transform.eulerAngles = new Vector3(pitch, yaw, 0f);
        }
    }
}
