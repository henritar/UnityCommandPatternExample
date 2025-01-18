using UnityEngine;

public class TileBehavior : MonoBehaviour
{
    [SerializeField] private Material activeMaterial;   // Material para tile ativa
    [SerializeField] private Material inactiveMaterial; // Material para tile inativa

    private Renderer tileRenderer;
    public bool IsActive { get; private set; } = false; // Rastreamento do estado da tile

    private void Start()
    {
        // Obtém o Renderer da tile
        tileRenderer = GetComponent<Renderer>();

        // Define o material inicial como inativo
        if (tileRenderer != null && inactiveMaterial != null)
        {
            tileRenderer.material = inactiveMaterial;
        }

        if (transform.position == Vector3.zero)
        {
            IsActive = true;
            tileRenderer.material = activeMaterial;
        }
    }

    public void ActivateTile()
    {
        // Troca para o material ativo
        if (tileRenderer != null && activeMaterial != null)
        {
            IsActive = true;

            tileRenderer.material = activeMaterial;
            Debug.Log($"Tile {gameObject.name} ativada.");
        }
    }

    public void DeactivateTile()
    {
        // Troca para o material inativo
        if (tileRenderer != null && inactiveMaterial != null)
        {
            IsActive = false;

            tileRenderer.material = inactiveMaterial;
            Debug.Log($"Tile {gameObject.name} desativada.");
        }
    }
}