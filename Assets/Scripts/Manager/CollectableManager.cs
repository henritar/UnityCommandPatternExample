using UnityEngine;

public class CollectableManager : MonoBehaviour
{
    public static CollectableManager Instance { get; private set; }
    private int totalCollectables;
    private int collectedCount = 0;

    private void Awake()
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

    private void Start()
    {
        // Conta todos os colecionáveis na cena
        totalCollectables = LevelManager.Instance.CurrentLevel.CollectablePosition.Count;
    }

    public void CollectItem()
    {
        collectedCount++;
    }

    public void RestoreItem()
    {
        collectedCount = Mathf.Max(0, collectedCount - 1); // Evita números negativos
    }

    public void CheckLevelComplete()
    {
        if (collectedCount >= totalCollectables)
        {
            CompleteLevel();
        }
    }

    public void CompleteLevel()
    {
        Debug.Log("Level Complete!");
        LevelManager.Instance.LoadNextLevel(); // Avança para o próximo nível
    }

    public void RestartLevel()
    {
        collectedCount = 0;
        totalCollectables = LevelManager.Instance.CurrentLevel.CollectablePosition.Count;
    }
}
