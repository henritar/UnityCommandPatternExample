using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    // Fun��o chamada pelo bot�o StartGame
    public void StartGame()
    {
        // Carrega a cena chamada "GameScene"
        SceneManager.LoadScene("GameScene");
    }

    // Fun��o chamada pelo bot�o Quit
    public void QuitGame()
    {
#if UNITY_EDITOR
        // Para o modo Play no editor
        EditorApplication.isPlaying = false;
#else
        // Sai do aplicativo no build
        Application.Quit();
#endif
        Debug.Log("Quit Game"); // Mensagem de depura��o
    }
}
