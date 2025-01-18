using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    public float moveSpeed = 5f;

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

        if (Input.GetKeyDown(KeyCode.W)) ExecuteCommand(new MoveCommand(this, Vector3.forward));
        if (Input.GetKeyDown(KeyCode.S)) ExecuteCommand(new MoveCommand(this, Vector3.back));
        if (Input.GetKeyDown(KeyCode.A)) ExecuteCommand(new MoveCommand(this, Vector3.left));
        if (Input.GetKeyDown(KeyCode.D)) ExecuteCommand(new MoveCommand(this, Vector3.right));

        if (Input.GetKeyDown(KeyCode.Z)) UndoMove();
        if (Input.GetKeyDown(KeyCode.R)) LevelManager.Instance.RestartLevel();
    }

    void MovePlayer()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
    }

    public void SetTargetPosition(Vector3 newPosition)
    {
        targetPosition = newPosition;
        originalPosition = transform.position; // Atualiza a posição original
    }

    public void UndoMove()
    {
        if (commandHistory.Count > 0)
        {
            ICommand lastCommand = commandHistory.Pop();
            lastCommand.Undo();
        }
    }

    public void ExecuteCommand(ICommand command)
    {
        if (command.Execute())
        {
            commandHistory.Push(command);
        }
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
}
