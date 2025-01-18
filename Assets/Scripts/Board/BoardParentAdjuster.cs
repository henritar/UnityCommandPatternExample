using UnityEngine;

public class BoardParentAdjuster : MonoBehaviour
{
    public static BoardParentAdjuster Instance { get; private set; } // Singleton Instance

    public Transform boardParent; // Referência ao BoardParent

    private void Awake()
    {
        // Configuração Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        AdjustBoardParentPivot();
    }

    public void AdjustBoardParentPivot()
    {
        if (boardParent == null)
        {
            Debug.LogError("BoardParent não configurado.");
            return;
        }

        // Calcula os limites dos filhos
        Bounds bounds = CalculateBounds();

        if (bounds.size == Vector3.zero)
        {
            Debug.LogError("Não foi possível calcular os limites dos filhos. Verifique se há objetos no BoardParent.");
            return;
        }

        // Reposiciona o BoardParent
        RepositionBoardParent(bounds);
    }

    private Bounds CalculateBounds()
    {
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero); // Inicializa os bounds

        foreach (Transform child in boardParent)
        {
            // Verifica se o filho tem Renderer
            Renderer childRenderer = child.GetComponent<Renderer>();

            if (childRenderer != null)
            {
                // Se o filho tiver um Renderer, encapsula os Bounds do Renderer no espaço mundial
                bounds.Encapsulate(childRenderer.bounds);
            }
            else
            {
                // Se não tiver Renderer, usamos a posição e escala do objeto
                bounds.Encapsulate(child.position);
            }
        }

        if (bounds.size == Vector3.zero)
        {
            Debug.LogError("Nenhum filho válido foi encontrado no BoardParent.");
            return new Bounds(Vector3.zero, Vector3.zero); // Retorna bounds inválido se não encontrar nada
        }

        // Log para depuração
        Debug.Log($"Bounds calculados: Center = {bounds.center}, Size = {bounds.size}");

        return bounds;
    }



    private void RepositionBoardParent(Bounds bounds)
    {
        // Calcula o centro do BoardParent
        Vector3 center = bounds.center;

        // Muda a posição do BoardParent para o centro
        Vector3 offset = boardParent.position - center;
        boardParent.position = center;

        // Compensa o movimento do BoardParent ajustando as posições dos filhos
        foreach (Transform child in boardParent)
        {
            child.position += offset;
        }

        // Log para depuração
        Debug.Log($"Pivô do BoardParent ajustado ao centro: {center}");
    }

}
