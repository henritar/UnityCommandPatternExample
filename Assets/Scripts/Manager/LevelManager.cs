using System;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public float tileDistance = 5f; // Espaçamento entre as tiles

    [NonSerialized]
    public SO_Level CurrentLevel;
    [SerializeField]
    private GameObject TilePrefab;
    [SerializeField]
    private GameObject CollectablePrefab;
    [SerializeField]
    private Transform TileParent; // Pai das tiles
    [SerializeField]
    private Transform CollectableParent; // Pai dos colecionáveis
    public Transform boardParent;
    [SerializeField]
    private int levelIndex = 1; // Índice do nível, começando em 1

    private GameObject[,] tiles;

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
            GameObject collectable = Instantiate(CollectablePrefab, adjustedPosition, Quaternion.identity, CollectableParent);
            collectable.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f); // Ajuste a escala do coletável, se necessário
        }

        BoardParentAdjuster.Instance.PrintTilePositions();
        BoardParentAdjuster.Instance.AdjustBoardParentPivot();
        BoardCameraController.Instance.AdjustCamera();
    }

    public void LoadNextLevel()
    {
        levelIndex++;
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
}
