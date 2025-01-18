using Cinemachine;
using UnityEngine;

public class CameraZoom3D : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera; // Refer�ncia � Cinemachine Virtual Camera
    public float zoomSpeed = 10f; // Velocidade do zoom
    public float minFOV = 20f; // Limite m�nimo de FOV
    public float maxFOV = 60f; // Limite m�ximo de FOV

    void Update()
    {
        // Obt�m a entrada da rodinha do mouse
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            // Pega o FOV atual da Cinemachine Virtual Camera
            float currentFOV = virtualCamera.m_Lens.FieldOfView;

            // Ajusta o FOV
            float newFOV = Mathf.Clamp(currentFOV - scroll * zoomSpeed, minFOV, maxFOV);

            // Aplica o novo FOV
            virtualCamera.m_Lens.FieldOfView = newFOV;
        }
    }
}
