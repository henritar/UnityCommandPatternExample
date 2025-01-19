using UnityEngine;
using System.Collections;

public class RotateBoardCommand : IAsyncCommand
{
    public static bool isRotating = false;

    private Transform boardTransform;
    private bool clockwise;
    private float rotationAngle = 90f;
    private float rotationSpeed = 100f; // Velocidade da rota��o em graus por segundo

    private bool isCompleted = false; // Indica se o comando foi conclu�do
    private bool isAutoUndo = false; // Indica se o pr�ximo Undo � autom�tico

    public RotateBoardCommand(Transform boardTransform, bool clockwise)
    {
        this.boardTransform = boardTransform;
        this.clockwise = clockwise;
    }

    public bool Execute()
    {
        isCompleted = false; // Reseta o estado de conclus�o
        PlayerController.Instance.StartCoroutine(RotateBoardCoroutine(clockwise));
        return true;
    }

    public void Undo()
    {
        isCompleted = false; // Reseta o estado de conclus�o
        PlayerController player = PlayerController.Instance;

        // Desativa a tile atual
        if (!isAutoUndo)
        {
            player.LastActiveTile.DeactivateTile();
            Debug.Log($"Tile {player.LastActiveTile.name} desativada antes do Undo.");
        }

        isAutoUndo = false;
        PlayerController.Instance.StartCoroutine(RotateBoardCoroutine(!clockwise, true));
    }

    public bool IsCompleted => isCompleted;
    private IEnumerator RotateBoardCoroutine(bool rotateClockwise, bool isUndo = false)
    {
        isRotating = true;

        float currentRotation = 0f;
        float targetAngle = rotateClockwise ? rotationAngle : -rotationAngle;
        Vector3 rotationAxis = Vector3.up;

        while (Mathf.Abs(currentRotation) < Mathf.Abs(targetAngle))
        {
            float step = rotationSpeed * Time.deltaTime;
            float rotationThisFrame = Mathf.Min(step, Mathf.Abs(targetAngle - currentRotation));

            boardTransform.Rotate(rotationAxis, rotationThisFrame * Mathf.Sign(targetAngle), Space.Self);
            currentRotation += rotationThisFrame;

            yield return null;
        }

        boardTransform.rotation = Quaternion.Euler(0, Mathf.Round(boardTransform.rotation.eulerAngles.y / 90f) * 90f, 0);

        // Atualiza o estado l�gico e visual do tabuleiro
        LevelManager.Instance.RotateBoardState(rotateClockwise);

        if (!isUndo) // Apenas verifica tiles se n�o for Undo
        {
            // Verifica e ativa a tile do jogador
            if (!CheckAndActivatePlayerTile())
            {
                Debug.LogWarning("Jogador terminou em uma tile j� ativada. Executando Undo...");
                isAutoUndo = true;
                PlayerController.Instance.UndoMove();
                yield break; // Encerra a execu��o da rota��o
            }
        }

        PlayerController.Instance.AdjustPlayerPosition();

        isCompleted = true;
        isRotating = false;
    }




    private bool CheckAndActivatePlayerTile()
    {
        PlayerController player = PlayerController.Instance;

        // Encontra a tile mais pr�xima do jogador
        TileBehavior closestTile = FindClosestTile(player.transform.position);

        if (closestTile == null)
        {
            Debug.LogError("Nenhuma tile encontrada para o jogador.");
            return false;
        }

        // Permite permanecer na mesma tile
        if (closestTile == player.LastActiveTile)
        {
            closestTile.ActivateTile();
            Debug.Log("Jogador permaneceu na mesma tile. Rota��o permitida.");
            return true;
        }

        // Verifica se a tile est� ativa
        if (closestTile.IsActive)
        {
            Debug.LogWarning($"Tile {closestTile.name} j� ativada. Rota��o n�o permitida.");
            return false;
        }

        // Ativa a nova tile e atualiza a �ltima ativa
        closestTile.ActivateTile();
        player.LastActiveTile = closestTile;
        Debug.Log($"Nova tile ativada: {closestTile.name}");
        return true;
    }


    private TileBehavior FindClosestTile(Vector3 position)
    {
        TileBehavior closestTile = null;
        float closestDistance = float.MaxValue;

        foreach (TileBehavior tile in Object.FindObjectsOfType<TileBehavior>())
        {
            float distance = Vector3.Distance(position, tile.transform.position);
            if (distance < closestDistance)
            {
                closestTile = tile;
                closestDistance = distance;
            }
        }

        return closestTile;
    }

}
