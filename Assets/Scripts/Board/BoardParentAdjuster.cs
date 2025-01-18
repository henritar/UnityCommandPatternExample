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

    private void Start()
    {
        AdjustBoardParentPivot();
    }

    public void AdjustBoardParentPivot()
    {
        if (boardParent == null)
        {
            Debug.LogError("BoardParent n�o configurado.");
            return;
        }

        // Calcula os limites dos filhos
        Bounds bounds = CalculateBounds();

        if (bounds.size == Vector3.zero)
        {
            Debug.LogError("N�o foi poss�vel calcular os limites dos filhos. Verifique se h� objetos no BoardParent.");
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
                // Se o filho tiver um Renderer, encapsula os Bounds do Renderer no espa�o mundial
                bounds.Encapsulate(childRenderer.bounds);
            }
            else
            {
                // Se n�o tiver Renderer, usamos a posi��o e escala do objeto
                bounds.Encapsulate(child.position);
            }
        }

        if (bounds.size == Vector3.zero)
        {
            Debug.LogError("Nenhum filho v�lido foi encontrado no BoardParent.");
            return new Bounds(Vector3.zero, Vector3.zero); // Retorna bounds inv�lido se n�o encontrar nada
        }

        // Log para depura��o
        Debug.Log($"Bounds calculados: Center = {bounds.center}, Size = {bounds.size}");

        return bounds;
    }



    private void RepositionBoardParent(Bounds bounds)
    {
        // Calcula o centro do BoardParent
        Vector3 center = bounds.center;

        // Muda a posi��o do BoardParent para o centro
        Vector3 offset = boardParent.position - center;
        boardParent.position = center;

        // Compensa o movimento do BoardParent ajustando as posi��es dos filhos
        foreach (Transform child in boardParent)
        {
            child.position += offset;
        }

        // Log para depura��o
        Debug.Log($"Piv� do BoardParent ajustado ao centro: {center}");
    }

}
