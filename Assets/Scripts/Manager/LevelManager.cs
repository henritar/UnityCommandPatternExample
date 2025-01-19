using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public float tileDistance = 5f; // Espaçamento entre as tiles

    public GameObject GameOverMenu;

    [NonSerialized]
    public SO_Level CurrentLevel;
    [SerializeField]
    private GameObject TilePrefab;
    [SerializeField]
    private GameObject CollectablePrefab;
    [SerializeField]
    private GameObject verticalBarrierPrefab;
    [SerializeField]
    private GameObject horizontalBarrierPrefab;
    [SerializeField]
    private Transform TileParent; // Pai das tiles
    [SerializeField]
    private Transform CollectableParent; // Pai dos colecionáveis
    public Transform boardParent;
    [SerializeField]
    private int levelIndex = 1; // Índice do nível, começando em 1

    private GameObject[,] tiles;
    private List<Vector3> dynamicHorizontalBarriers;
    private List<Vector3> dynamicVerticalBarriers;
    public bool GameOver => GameOverMenu.activeSelf;

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
        LoadLevel();
    }

    private void Start()
    {
        GameOverMenu.SetActive(false);
        GenerateLevel();
    }

    void LoadLevel()
    {
        // Construir o nome do nível com base no índice
        string levelName = $"Level{levelIndex}";

        // Carregar o ScriptableObject da pasta Resources/Levels
        CurrentLevel = Resources.Load<SO_Level>($"Levels/{levelName}");
        if (CurrentLevel == null)
        {
            Debug.LogError($"Level '{levelName}' não encontrado em Resources/Levels!");
            return;
        }

        Debug.Log($"Level '{levelName}' carregado com sucesso.");
    }

    void GenerateLevel()
    {
        if (CurrentLevel == null)
        {
            Debug.LogError("Nenhum nível foi carregado!");
            return;
        }

        tiles = new GameObject[CurrentLevel.xSize, CurrentLevel.ySize];

        // Criar as tiles com as configurações especificadas
        for (int x = 0; x < CurrentLevel.xSize; x++)
        {
            for (int y = 0; y < CurrentLevel.ySize; y++)
            {
                // Posicionar cada tile com espaçamento absoluto de 5 unidades
                Vector3 position = new Vector3(x * 5, 0, y * 5);
                GameObject tile = Instantiate(TilePrefab, position, Quaternion.identity, TileParent);

                // Configurar a escala, rotação e nome da tile
                tile.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);
                tile.transform.localRotation = Quaternion.identity;
                tile.name = $"Tile {x}-{y}";

                tiles[x, y] = tile;
            }
        }

        // Colocar os colecionáveis
        foreach (Vector3 position in CurrentLevel.CollectablePosition)
        {
            // Ajustar a posição do colecionável para usar o mesmo sistema de grid absoluto
            Vector3 adjustedPosition = new Vector3(position.x * 5, position.y, position.z * 5);
            Quaternion rotation = new Quaternion(-90, 0 ,0, 1);
            GameObject collectable = Instantiate(CollectablePrefab, adjustedPosition, rotation, CollectableParent);
            collectable.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f); // Ajuste a escala do coletável, se necessário
        }

        // Gera as barreiras
        GenerateBarriers();
        AssignBarriersToTiles();
        InitializeDynamicBarriers();

        BoardParentAdjuster.Instance.PrintTilePositions();
        BoardParentAdjuster.Instance.AdjustBoardParentPivot();
        BoardCameraController.Instance.AdjustCamera();
    }

    private void GenerateBarriers()
    {
        // Barreiras Horizontais
        foreach (Vector3 barrierData in CurrentLevel.HorizontalBarriers)
        {
            // Calcula a posição global da barreira horizontal
            Vector3 worldPosition = new Vector3(
                barrierData.x * tileDistance,
                0,
                barrierData.z * tileDistance + (barrierData.y == 0 ? -tileDistance / 2 : tileDistance / 2)
            );

            // Instancia a barreira horizontal com rotação padrão (sem rotação necessária)
            Quaternion horizontalRotation = Quaternion.Euler(0, 90, 0);
            Instantiate(horizontalBarrierPrefab, worldPosition, horizontalRotation, boardParent);
        }

        // Barreiras Verticais
        foreach (Vector3 barrierData in CurrentLevel.VerticalBarriers)
        {
            // Calcula a posição global da barreira vertical
            Vector3 worldPosition = new Vector3(
                barrierData.x * tileDistance + (barrierData.y == 2 ? -tileDistance / 2 : tileDistance / 2),
                0,
                barrierData.z * tileDistance
            );

            Instantiate(verticalBarrierPrefab, worldPosition, Quaternion.identity, boardParent);
        }
    }
    private void InitializeDynamicBarriers()
    {
        dynamicHorizontalBarriers = new List<Vector3>(CurrentLevel.HorizontalBarriers);
        dynamicVerticalBarriers = new List<Vector3>(CurrentLevel.VerticalBarriers);
    }

    public TileBehavior GetTileAtPosition(Vector3 position)
    {
        // Converte a posição global para índices no grid
        int tileX = Mathf.RoundToInt(position.x / tileDistance);
        int tileZ = Mathf.RoundToInt(position.z / tileDistance);

        // Verifica se os índices estão dentro dos limites do tabuleiro
        if (tileX >= 0 && tileX < CurrentLevel.xSize && tileZ >= 0 && tileZ < CurrentLevel.ySize)
        {
            return tiles[tileX, tileZ].GetComponent<TileBehavior>();
        }

        Debug.LogWarning($"Posição fora dos limites do tabuleiro: {position}");
        return null; // Retorna nulo se a posição estiver fora dos limites
    }

    public void LoadNextLevel()
    {
        levelIndex++;
        if (levelIndex >= 4)
        {
            Debug.Log("GameOver!");
            GameOverMenu.SetActive(true);
            return;
        }
        LoadLevel();
        RestartLevel();
    }

    public void RestoreLevel()
    {
        LoadLevel();
        RestartLevel();
    }

    public void RestartLevel()
    {
        ClearLevel();
        GenerateLevel();
        PlayerController.Instance.RestartLevel();
        CollectableManager.Instance.RestartLevel();
    }

    void ClearLevel()
    {
        // Remover tiles e colecionáveis antigos
        foreach (Transform child in TileParent)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in CollectableParent)
        {
            Destroy(child.gameObject);
        }
    }

    private void AssignBarriersToTiles()
    {
        foreach (Vector3 barrier in CurrentLevel.HorizontalBarriers)
        {
            int x = Mathf.RoundToInt(barrier.x);
            int z = Mathf.RoundToInt(barrier.z);

            if (x < 0 || x >= CurrentLevel.xSize || z < 0 || z >= CurrentLevel.ySize) continue;

            TileBehavior tile = tiles[x, z].GetComponent<TileBehavior>();

            if (barrier.y == 0) tile.HasBarrierSouth = true;
            if (barrier.y == 1) tile.HasBarrierNorth = true;
        }

        foreach (Vector3 barrier in CurrentLevel.VerticalBarriers)
        {
            int x = Mathf.RoundToInt(barrier.x);
            int z = Mathf.RoundToInt(barrier.z);

            if (x < 0 || x >= CurrentLevel.xSize || z < 0 || z >= CurrentLevel.ySize) continue;

            TileBehavior tile = tiles[x, z].GetComponent<TileBehavior>();

            if (barrier.y == 2) tile.HasBarrierWest = true;
            if (barrier.y == 3) tile.HasBarrierEast = true;
        }
    }

    public void RotateBoardState(bool clockwise)
    {
        int sizeX = CurrentLevel.xSize;
        int sizeY = CurrentLevel.ySize;

        // Cria novas listas para armazenar barreiras rotacionadas
        List<Vector3> newHorizontalBarriers = new List<Vector3>();
        List<Vector3> newVerticalBarriers = new List<Vector3>();

        // Cria nova matriz para tiles rotacionadas
        GameObject[,] newTiles = new GameObject[sizeX, sizeY];

        // Rotaciona as tiles logicamente
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                int newX = clockwise ? y : sizeY - 1 - y;
                int newY = clockwise ? sizeX - 1 - x : x;

                newTiles[newX, newY] = tiles[x, y];
            }
        }

        // Atualiza as barreiras horizontais
        foreach (Vector3 barrier in dynamicHorizontalBarriers)
        {
            int x = Mathf.RoundToInt(barrier.x);
            int z = Mathf.RoundToInt(barrier.z);
            int side = Mathf.RoundToInt(barrier.y);

            int newX = clockwise ? z : sizeY - 1 - z;
            int newZ = clockwise ? sizeX - 1 - x : x;

            if (clockwise)
            {
                if (side == 0) newVerticalBarriers.Add(new Vector3(newX, 2, newZ)); // Sul -> Oeste
                if (side == 1) newVerticalBarriers.Add(new Vector3(newX, 3, newZ)); // Norte -> Leste
            }
            else
            {
                if (side == 0) newVerticalBarriers.Add(new Vector3(newX, 3, newZ)); // Sul -> Leste
                if (side == 1) newVerticalBarriers.Add(new Vector3(newX, 2, newZ)); // Norte -> Oeste
            }
        }

        // Atualiza as barreiras verticais
        foreach (Vector3 barrier in dynamicVerticalBarriers)
        {
            int x = Mathf.RoundToInt(barrier.x);
            int z = Mathf.RoundToInt(barrier.z);
            int side = Mathf.RoundToInt(barrier.y);

            int newX = clockwise ? z : sizeY - 1 - z;
            int newZ = clockwise ? sizeX - 1 - x : x;

            if (clockwise)
            {
                if (side == 2) newHorizontalBarriers.Add(new Vector3(newX, 1, newZ)); // Oeste -> Norte
                if (side == 3) newHorizontalBarriers.Add(new Vector3(newX, 0, newZ)); // Leste -> Sul
            }
            else
            {
                if (side == 2) newHorizontalBarriers.Add(new Vector3(newX, 0, newZ)); // Oeste -> Sul
                if (side == 3) newHorizontalBarriers.Add(new Vector3(newX, 1, newZ)); // Leste -> Norte
            }
        }

        // Atualiza o estado no LevelManager
        tiles = newTiles;
        dynamicHorizontalBarriers = newHorizontalBarriers;
        dynamicVerticalBarriers = newVerticalBarriers;

        // Atualiza as tiles visualmente
        UpdateTilesWithBarriers();

        Debug.Log("Estado do tabuleiro rotacionado.");
    }

    public void UpdateTilesWithBarriers()
    {
        // Reseta todas as barreiras em todas as tiles
        foreach (GameObject tileObject in tiles)
        {
            TileBehavior tile = tileObject.GetComponent<TileBehavior>();
            tile.ResetBarriers();
        }

        // Aplica barreiras horizontais
        foreach (Vector3 barrier in dynamicHorizontalBarriers)
        {
            int x = Mathf.RoundToInt(barrier.x);
            int z = Mathf.RoundToInt(barrier.z);

            if (x >= 0 && x < CurrentLevel.xSize && z >= 0 && z < CurrentLevel.ySize)
            {
                TileBehavior tile = tiles[x, z].GetComponent<TileBehavior>();
                if (barrier.y == 0) tile.HasBarrierSouth = true;
                if (barrier.y == 1) tile.HasBarrierNorth = true;
            }
        }

        // Aplica barreiras verticais
        foreach (Vector3 barrier in dynamicVerticalBarriers)
        {
            int x = Mathf.RoundToInt(barrier.x);
            int z = Mathf.RoundToInt(barrier.z);

            if (x >= 0 && x < CurrentLevel.xSize && z >= 0 && z < CurrentLevel.ySize)
            {
                TileBehavior tile = tiles[x, z].GetComponent<TileBehavior>();
                if (barrier.y == 2) tile.HasBarrierWest = true;
                if (barrier.y == 3) tile.HasBarrierEast = true;
            }
        }
    }

}
