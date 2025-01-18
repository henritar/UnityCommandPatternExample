using UnityEngine;

public class Barrier : MonoBehaviour
{
    public enum BarrierType { Horizontal, Vertical }
    public BarrierType barrierType;

    public bool BlocksMovement(Vector3 currentPosition, Vector3 targetPosition)
    {
        // Calcula a direção do movimento
        Vector3 direction = targetPosition - currentPosition;

        if (barrierType == BarrierType.Horizontal)
        {
            // Bloqueia movimento no eixo Z (norte/sul) se a barreira está alinhada
            return Mathf.Approximately(transform.position.z, currentPosition.z + direction.z / 2) &&
                   Mathf.Approximately(transform.position.x, currentPosition.x);
        }
        else if (barrierType == BarrierType.Vertical)
        {
            // Bloqueia movimento no eixo X (leste/oeste) se a barreira está alinhada
            return Mathf.Approximately(transform.position.x, currentPosition.x + direction.x / 2) &&
                   Mathf.Approximately(transform.position.z, currentPosition.z);
        }

        return false;
    }
}