using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    public TileBehavior LastActiveTile { get; set; } // Última tile onde o jogador estava
    public Animator animator; // Referência ao Animator

    public float moveSpeed = 5f;
    private bool isUndoing = false; // Flag para evitar reentrância no Undo
    public bool IsUndoing => isUndoing;
    private Vector3 targetPosition;
    private Vector3 originalPosition; // Para armazenar a posição inicial durante o tremor
    private Stack<ICommand> commandHistory = new Stack<ICommand>();

    private bool isShaking = false; // Para evitar tremores sobrepostos
    private float shakeDuration = 0.2f; // Duração do tremor
    private float shakeMagnitude = 0.1f; // Intensidade do tremor

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        LastActiveTile = FindClosestTile(transform.position);
        targetPosition = transform.position; // Posição inicial
        originalPosition = transform.position;
    }

    void Update()
    {
        HandleInput();
        if (!isShaking) // Apenas move o jogador se não estiver tremendo
        {
            MovePlayer();
        }
    }

    void HandleInput()
    {
        if (Vector3.Distance(transform.position, targetPosition) > 0.1f) return;
        if (RotateBoardCommand.isRotating) return;


        if (Input.GetKeyDown(KeyCode.W)) ExecuteCommand(new MoveCommand(this, Vector3.forward));
        if (Input.GetKeyDown(KeyCode.S)) ExecuteCommand(new MoveCommand(this, Vector3.back));
        if (Input.GetKeyDown(KeyCode.A)) ExecuteCommand(new MoveCommand(this, Vector3.left));
        if (Input.GetKeyDown(KeyCode.D)) ExecuteCommand(new MoveCommand(this, Vector3.right));
        if (Input.GetKeyDown(KeyCode.Q)) ExecuteCommand(new RotateBoardCommand(BoardParentAdjuster.Instance.boardParent, false));
        if (Input.GetKeyDown(KeyCode.E)) ExecuteCommand(new RotateBoardCommand(BoardParentAdjuster.Instance.boardParent, true));

        if (Input.GetKeyDown(KeyCode.Z)) UndoMove();
        if (Input.GetKeyDown(KeyCode.R)) LevelManager.Instance.RestoreLevel();
    }

    void MovePlayer()
    {
        // Atualiza a posição
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);

        // Calcula a velocidade atual
        float speed = Vector3.Distance(transform.position, targetPosition);

        // Atualiza o parâmetro no Animator
        animator.SetFloat("Speed", speed);

        // Rotaciona na direção do movimento
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // Garante que o personagem pare exatamente na tile
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            transform.position = targetPosition;
        }
    }


    public void SetTargetPosition(Vector3 newPosition)
    {
        targetPosition = newPosition;

        // Atualiza a última tile ativa
        LastActiveTile = FindClosestTile(newPosition);
    }

    public void UndoMove()
    {
        if (isUndoing || commandHistory.Count == 0) return; // Bloqueia se já está desfazendo ou não há comandos

        StartCoroutine(ProcessUndoMove());
    }

    private IEnumerator ProcessUndoMove()
    {
        isUndoing = true;

        ICommand lastCommand = commandHistory.Pop(); // Remove o último comando da pilha

        if (lastCommand is IAsyncCommand asyncCommand)
        {
            asyncCommand.Undo();

            // Aguarda a conclusão do comando assíncrono
            while (!asyncCommand.IsCompleted)
            {
                yield return null;
            }
        }
        else
        {
            lastCommand.Undo(); // Comando síncrono
        }

        isUndoing = false; // Libera o estado de Undo após a conclusão
    }

    public void ExecuteCommand(ICommand command)
    {
        if (command.Execute())
        {
            commandHistory.Push(command);
        }

        CollectableManager.Instance.CheckLevelComplete();
    }

    public void Shake(Vector3 startPosition)
    {
        if (!isShaking) // Evita tremores sobrepostos
        {
            StartCoroutine(PerformShake(startPosition));
        }
    }

    private System.Collections.IEnumerator PerformShake(Vector3 startPosition)
    {
        isShaking = true;

        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            // Gera um deslocamento aleatório apenas no plano XZ
            Vector3 randomOffset = new Vector3(
                Random.Range(-shakeMagnitude, shakeMagnitude), // Deslocamento no eixo X
                0,                                            // Sem deslocamento no eixo Y
                Random.Range(-shakeMagnitude, shakeMagnitude) // Deslocamento no eixo Z
            );

            // Aplica o tremor ao redor da posição inicial
            transform.position = startPosition + randomOffset;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Retorna ao centro da tile inicial
        transform.position = startPosition;
        isShaking = false;
    }

    public void RestartLevel()
    {
        // Reiniciar o nível aqui (limpar histórico e resetar posição)
        commandHistory.Clear();
        transform.position = new Vector3(0, transform.position.y, 0); // Posição inicial (ajuste conforme necessário)
        targetPosition = transform.position;
    }

    public void AdjustPlayerPosition()
    {
        // Garante que o jogador permaneça centralizado na tile mais próxima após a rotação
        TileBehavior closestTile = FindClosestTile(transform.position);
        if (closestTile != null)
        {
            var closestTilePosition = new Vector3(closestTile.transform.position.x, transform.position.y, closestTile.transform.position.z);
            SetTargetPosition(closestTilePosition);
        }
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
