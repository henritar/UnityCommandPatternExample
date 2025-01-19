using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    // Função chamada pelo botão StartGame
    public void StartGame()
    {
        // Carrega a cena chamada "GameScene"
        SceneManager.LoadScene("GameScene");
    }

    // Função chamada pelo botão Quit
    public void QuitGame()
    {
#if UNITY_EDITOR
        // Para o modo Play no editor
        EditorApplication.isPlaying = false;
#else
        // Sai do aplicativo no build
        Application.Quit();
#endif
        Debug.Log("Quit Game"); // Mensagem de depuração
    }
}
