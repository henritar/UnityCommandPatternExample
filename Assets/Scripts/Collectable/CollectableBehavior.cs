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

    private void Start()
    {
        // Obtém o número total de elementos no enum
        int enumLength = System.Enum.GetValues(typeof(CollectableType)).Length;

        // Escolhe aleatoriamente um índice válido no enum
        int randomValue = Random.Range(0, enumLength);

        // Atribui o tipo correspondente ao índice gerado
        type = (CollectableType)randomValue;

    }
}
