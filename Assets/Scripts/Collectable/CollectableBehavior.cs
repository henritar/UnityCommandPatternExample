using UnityEngine;

public class CollectableBehavior : MonoBehaviour
{
    public enum CollectableType
    {
        Regular,
        RotateBoard
        // Adicione mais tipos de coletáveis aqui
    }

    public CollectableType type = CollectableType.Regular; // Tipo padrão

    // Parâmetros de oscilação
    public float floatAmplitude = 0.5f; // Amplitude do movimento (altura)
    public float floatFrequency = 1f;  // Frequência do movimento (velocidade)

    private Vector3 startPosition; // Posição inicial do coletável

    private void Start()
    {
        // Salva a posição inicial
        startPosition = transform.position;

        // Obtém o número total de elementos no enum
        int enumLength = System.Enum.GetValues(typeof(CollectableType)).Length;

        // Escolhe aleatoriamente um índice válido no enum
        int randomValue = Random.Range(0, enumLength);

        // Atribui o tipo correspondente ao índice gerado
        type = (CollectableType)randomValue;
    }

    private void Update()
    {
        // Calcula a nova posição com base na senoide
        float newY = startPosition.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;

        // Aplica a nova posição
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
}
