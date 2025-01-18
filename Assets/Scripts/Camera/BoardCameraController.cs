using Cinemachine;
using UnityEngine;

public class BoardCameraController : MonoBehaviour
{
    public static BoardCameraController Instance { get; private set; }

    public CinemachineVirtualCamera virtualCamera; // Referência à Cinemachine Virtual Camera
    public Transform boardParent;                 // Referência ao BoardParent
    public float zoomSpeed = 10f;                 // Velocidade do zoom
    public float minFOV = 20f;                    // Limite mínimo de FOV
    public float maxFOV = 60f;                    // Limite máximo de FOV
    public float padding = 2f;                    // Espaço extra ao redor do tabuleiro
    public float distanceMultiplier = 1.2f;       // Fator de ajuste para o Framing Transposer

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Update()
    {
        HandleZoomInput();
    }

    private void HandleZoomInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            float currentFOV = virtualCamera.m_Lens.FieldOfView;
            float newFOV = Mathf.Clamp(currentFOV - scroll * zoomSpeed, minFOV, maxFOV);
            virtualCamera.m_Lens.FieldOfView = newFOV;
        }
    }

    public void AdjustCamera()
    {
        if (boardParent == null || virtualCamera == null)
        {
            Debug.LogError("BoardParent ou VirtualCamera não configurado no BoardCameraController.");
            return;
        }

        // Calcula os limites do tabuleiro
        Bounds bounds = CalculateBoardBounds();

        if (bounds.size == Vector3.zero)
        {
            Debug.LogError("Os limites do tabuleiro estão incorretos. Verifique a configuração.");
            return;
        }

        // Centraliza a câmera no centro do tabuleiro
        Vector3 center = bounds.center;

        virtualCamera.LookAt = boardParent;
        virtualCamera.Follow = boardParent;

        // Ajusta a distância da câmera
        CinemachineFramingTransposer framingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        if (framingTransposer != null)
        {
            float boardWidth = bounds.size.x + padding;
            float boardHeight = bounds.size.z + padding;
            float maxDimension = Mathf.Max(boardWidth, boardHeight);

            // Calcula a distância proporcional ao tabuleiro
            framingTransposer.m_CameraDistance = Mathf.Max(maxDimension * distanceMultiplier, framingTransposer.m_CameraDistance);
        }

        // Ajusta o FOV inicial
        AdjustInitialFOV(bounds);
    }

    private void AdjustInitialFOV(Bounds bounds)
    {
        float boardWidth = bounds.size.x + padding;
        float boardHeight = bounds.size.z + padding;
        float maxDimension = Mathf.Max(boardWidth, boardHeight);

        // Define o FOV inicial baseado no maior lado do tabuleiro
        virtualCamera.m_Lens.FieldOfView = Mathf.Clamp(maxDimension, minFOV, maxFOV);

        Debug.Log($"Câmera ajustada: Centro={bounds.center}, FOV={virtualCamera.m_Lens.FieldOfView}");
    }

    private Bounds CalculateBoardBounds()
    {
        Bounds bounds = new Bounds(boardParent.position, Vector3.zero);

        foreach (Transform child in boardParent)
        {
            Renderer childRenderer = child.GetComponent<Renderer>();
            if (childRenderer != null)
            {
                bounds.Encapsulate(childRenderer.bounds);
            }
            else
            {
                bounds.Encapsulate(child.position);
            }
        }

        return bounds;
    }
}
