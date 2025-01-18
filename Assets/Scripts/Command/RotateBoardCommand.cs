using UnityEngine;
using System.Collections;

public class RotateBoardCommand : IAsyncCommand
{
    private Transform boardTransform;
    private bool clockwise;
    private float rotationAngle = 90f;
    private float rotationSpeed = 100f; // Velocidade da rotação em graus por segundo

    private bool isCompleted = false; // Indica se o comando foi concluído

    public RotateBoardCommand(Transform boardTransform, bool clockwise)
    {
        this.boardTransform = boardTransform;
        this.clockwise = clockwise;
    }

    public bool Execute()
    {
        isCompleted = false; // Reseta o estado de conclusão
        PlayerController.Instance.StartCoroutine(RotateBoardCoroutine(clockwise));
        return true;
    }

    public void Undo()
    {
        isCompleted = false; // Reseta o estado de conclusão
        PlayerController.Instance.StartCoroutine(RotateBoardCoroutine(!clockwise));
    }

    public bool IsCompleted => isCompleted;

    private IEnumerator RotateBoardCoroutine(bool rotateClockwise)
    {
        float currentRotation = 0f;
        float targetAngle = rotateClockwise ? rotationAngle : -rotationAngle;
        Vector3 rotationAxis = Vector3.up;

        while (Mathf.Abs(currentRotation) < Mathf.Abs(targetAngle))
        {
            float step = rotationSpeed * Time.deltaTime;
            float rotationThisFrame = Mathf.Min(step, Mathf.Abs(targetAngle - currentRotation));

            // Rotaciona as tiles
            boardTransform.Rotate(rotationAxis, rotationThisFrame * Mathf.Sign(targetAngle), Space.Self);
            currentRotation += rotationThisFrame;

            yield return null;
        }

        // Ajusta a rotação final para garantir precisão
        boardTransform.rotation = Quaternion.Euler(0, Mathf.Round(boardTransform.rotation.eulerAngles.y / 90f) * 90f, 0);

        // Verifica a posição do jogador e ativa a tile correspondente
        if (!CheckAndActivatePlayerTile())
        {
            Debug.LogWarning("Jogador caiu em uma tile já ativada. Desfazendo rotação...");
            PlayerController.Instance.UndoMove(); // Executa Undo
            yield break; // Encerra a corrotina após desfazer
        }

        isCompleted = true; // Marca o comando como concluído
    }



    private bool CheckAndActivatePlayerTile()
    {
        PlayerController player = PlayerController.Instance;

        // Ignora validação durante Undo
        if (player.IsUndoing)
        {
            return true; // Permite o movimento sem validar a tile
        }

        // Encontra a tile mais próxima do jogador
        TileBehavior closestTile = FindClosestTile(player.transform.position);

        if (closestTile == null)
        {
            Debug.LogError("Nenhuma tile encontrada para o jogador.");
            return false;
        }

        if (closestTile.IsActive)
        {
            // A tile já está ativada
            return false;
        }

        // Ativa a tile
        closestTile.ActivateTile();
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
