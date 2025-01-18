using UnityEngine;

public class BoardParentAdjuster : MonoBehaviour
{
    public static BoardParentAdjuster Instance { get; private set; } // Singleton Instance

    public Transform boardParent; // Refer�ncia ao BoardParent

    private void Awake()
    {
        // Configura��o Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AdjustBoardParentPivot()
    {
        if (boardParent == null)
        {
            Debug.LogError("BoardParent n�o configurado.");
            return;
        }

        Bounds bounds = CalculateBounds();

        if (bounds.size == Vector3.zero)
        {
            Debug.LogError("Os limites dos filhos est�o incorretos. Verifique a configura��o do BoardParent.");
            return;
        }

        RepositionBoardParent(bounds);
    }


    private Bounds CalculateBounds()
    {
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        bool boundsInitialized = false;

        foreach (Transform child in boardParent)
        {
            Renderer childRenderer = child.GetComponent<Renderer>();
            if (childRenderer != null)
            {
                if (!boundsInitialized)
                {
                    bounds = new Bounds(childRenderer.bounds.center, childRenderer.bounds.size);
                    boundsInitialized = true;
                }
                else
                {
                    bounds.Encapsulate(childRenderer.bounds);
                }
            }
            else
            {
                if (!boundsInitialized)
                {
                    bounds = new Bounds(child.position, Vector3.zero);
                    boundsInitialized = true;
                }
                else
                {
                    bounds.Encapsulate(child.position);
                }
            }
        }

        if (!boundsInitialized)
        {
            Debug.LogError("Nenhum filho v�lido encontrado no BoardParent.");
        }

        Debug.Log($"Bounds calculados: Center = {bounds.center}, Size = {bounds.size}");
        return bounds;
    }


    private void RepositionBoardParent(Bounds bounds)
    {
        // Calcula o centro dos limites dos filhos
        Vector3 center = bounds.center;

        // Calcula o deslocamento entre a posi��o atual do BoardParent e o centro
        Vector3 offset = center - boardParent.position;

        // Move o BoardParent para o centro calculado
        boardParent.position = center;

        // Ajusta os filhos para compensar o movimento do BoardParent
        foreach (Transform child in boardParent)
        {
            child.position -= offset;
        }

        Debug.Log($"Piv� do BoardParent ajustado ao centro calculado: {center}");
    }

    public void PrintTilePositions()
    {
        foreach (Transform child in boardParent)
        {
            Debug.Log($"Tile: {child.name}, Global Position: {child.position}");
        }
    }


}
