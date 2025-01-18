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

        // Verifica se a posição é válida e se há uma barreira bloqueando
        if (!IsValidPosition(newPosition) || IsBlockedByBarrier(previousPosition, newPosition))
        {
            player.Shake(previousPosition); // Feedback visual de movimento bloqueado
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
        // Calcula o ponto médio entre a posição atual e a posição de destino
        Vector3 midpoint = (currentPosition + targetPosition) / 2;

        // Verifica colisões no ponto médio
        Collider[] hits = Physics.OverlapSphere(midpoint, 0.1f); // Checa barreiras no meio do caminho
        foreach (var hit in hits)
        {
            Barrier barrier = hit.GetComponent<Barrier>();
            if (barrier != null && barrier.BlocksMovement(currentPosition, targetPosition))
            {
                return true;
            }
        }

        return false;
    }

    private GameObject GetCollectableAtPosition(Vector3 position)
    {
        Collider[] hits = Physics.OverlapSphere(position, 1f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Collectable"))
            {
                return hit.gameObject;
            }
        }
        return null;
    }
}
