using TMPro;
using UnityEngine;

public class MoveCommand : ICommand
{
    private PlayerController player;
    private Vector3 direction;
    private Vector3 previousPosition;
    private TileBehavior previousTile;
    private TileBehavior currentTile;
    private GameObject collectedItem;

    private float tileDistance = 5f; // Espaçamento entre as tiles

    public MoveCommand(PlayerController player, Vector3 direction)
    {
        this.player = player;
        this.direction = direction;
    }

    public bool Execute()
    {
        previousPosition = player.transform.position;

        // Calcula a nova posição do jogador com base no espaçamento das tiles
        Vector3 newPosition = CalculateNewPosition();

        Vector3 snappedCurrentPosition = SnapToTileCenter(previousPosition);
        Vector3 snappedTargetPosition = SnapToTileCenter(newPosition);
        // Verifica se a posição é válida e se há uma barreira bloqueando
        if (!IsValidPosition(newPosition) || IsBlockedByBarrier(previousPosition, newPosition))
        {
            player.Shake(snappedCurrentPosition); // Feedback visual de movimento bloqueado
            return false;
        }

        // Lida com a tile de destino
        if (!HandleTile(newPosition)) return false;

        // Atualiza a posição do jogador
        player.SetTargetPosition(newPosition);

        // Coleta o item na posição, se houver
        HandleCollectable(newPosition);

        return true;
    }

    public void Undo()
    {
        // Volta o jogador para a posição anterior
        player.SetTargetPosition(previousPosition);

        // Desativa a tile atual
        if (currentTile != null)
        {
            currentTile.DeactivateTile();
        }

        // Restaura o colecionável (se houver)
        if (collectedItem != null)
        {
            collectedItem.SetActive(true);
            CollectableManager.Instance.RestoreItem();
        }
    }

    private Vector3 CalculateNewPosition()
    {
        float tileDistance = LevelManager.Instance.tileDistance; // Acessa o tileDistance do LevelManager
        return new Vector3(
            Mathf.RoundToInt(previousPosition.x / tileDistance) * tileDistance + direction.x * tileDistance,
            player.transform.position.y,
            Mathf.RoundToInt(previousPosition.z / tileDistance) * tileDistance + direction.z * tileDistance
        );
    }


    private bool HandleTile(Vector3 newPosition)
    {
        currentTile = GetTileAtPosition(newPosition);
        if (currentTile != null)
        {
            if (currentTile.IsActive)
            {
                player.Shake(previousPosition); // Feedback visual
                return false;
            }
            currentTile.ActivateTile();
        }
        return true;
    }

    private void HandleCollectable(Vector3 newPosition)
    {
        collectedItem = GetCollectableAtPosition(newPosition);
        if (collectedItem == null) return;

        collectedItem.SetActive(false);
        var collectable = collectedItem.GetComponent<CollectableBehavior>();

        if (collectable != null)
        {
            HandleCollectableType(collectable);
        }
    }

    private void HandleCollectableType(CollectableBehavior collectable)
    {
        // Sempre atualiza o contador de coletáveis
        CollectableManager.Instance.CollectItem();

        // Executa ações específicas com base no tipo
        switch (collectable.type)
        {
            case CollectableBehavior.CollectableType.RotateBoard:
            default:
                break;
        }
    }


    private bool IsValidPosition(Vector3 position)
    {
        SO_Level currentLevel = LevelManager.Instance.CurrentLevel;
        return position.x >= 0 && position.x < currentLevel.xSize * tileDistance &&
               position.z >= 0 && position.z < currentLevel.ySize * tileDistance;
    }

    private TileBehavior GetTileAtPosition(Vector3 position)
    {
        RaycastHit hit;
        Vector3 rayOrigin = position + Vector3.up * 1f;
        float rayDistance = 3f;

        int tileLayerMask = LayerMask.GetMask("Tile");
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, rayDistance, tileLayerMask))
        {
            return hit.collider.GetComponent<TileBehavior>();
        }
        return null;
    }


    private bool IsBlockedByBarrier(Vector3 currentPosition, Vector3 targetPosition)
    {
        // Obtém as tiles atual e de destino
        TileBehavior currentTile = LevelManager.Instance.GetTileAtPosition(currentPosition);
        TileBehavior targetTile = LevelManager.Instance.GetTileAtPosition(targetPosition);

        if (currentTile == null || targetTile == null)
        {
            Debug.LogWarning("Uma das tiles é nula. Movimento não pode ser realizado.");
            return true; // Impede movimento se uma das tiles for inválida
        }

        // Calcula a direção do movimento
        Vector3 direction = targetPosition - currentPosition;

        // Define uma tolerância para comparações de direção
        float tolerance = 0.1f;

        // Verifica barreiras na tile atual (saída)
        if (Mathf.Abs(direction.z) > tolerance)
        {
            if (direction.z > 0 && currentTile.HasBarrierNorth) return true; // Norte
            if (direction.z < 0 && currentTile.HasBarrierSouth) return true; // Sul
        }

        if (Mathf.Abs(direction.x) > tolerance)
        {
            if (direction.x > 0 && currentTile.HasBarrierEast) return true;  // Leste
            if (direction.x < 0 && currentTile.HasBarrierWest) return true;  // Oeste
        }

        // Verifica barreiras na tile de destino (entrada)
        if (Mathf.Abs(direction.z) > tolerance)
        {
            if (direction.z > 0 && targetTile.HasBarrierSouth) return true; // Entrando pelo Sul
            if (direction.z < 0 && targetTile.HasBarrierNorth) return true; // Entrando pelo Norte
        }

        if (Mathf.Abs(direction.x) > tolerance)
        {
            if (direction.x > 0 && targetTile.HasBarrierWest) return true;  // Entrando pelo Oeste
            if (direction.x < 0 && targetTile.HasBarrierEast) return true;  // Entrando pelo Leste
        }

        return false; // Nenhuma barreira bloqueia o movimento
    }


    private GameObject GetCollectableAtPosition(Vector3 position)
    {
        Collider[] hits = Physics.OverlapSphere(position, 2f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Collectable"))
            {
                return hit.gameObject;
            }
        }
        return null;
    }

    private Vector3 SnapToTileCenter(Vector3 position)
    {
        float tileDistance = LevelManager.Instance.tileDistance;
        float x = Mathf.Round(position.x / tileDistance) * tileDistance;
        float z = Mathf.Round(position.z / tileDistance) * tileDistance;
        return new Vector3(x, position.y, z);
    }
}
