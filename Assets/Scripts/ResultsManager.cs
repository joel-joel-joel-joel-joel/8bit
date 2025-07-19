using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ResultsManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI victoryTitle;           // "¡VICTORIA!"
    public TextMeshProUGUI celebrationAvatar;      // Avatar del jugador
    public TextMeshProUGUI finalScoreText;         // "SCORE FINAL: X"
    public TextMeshProUGUI celebrationMessage;     // Mensaje personalizado
    public Button playAgainButton;                 // JUGAR DE NUEVO
    public Button backToMenuButton;                // MENÚ PRINCIPAL
    
    void Start()
    {
        // Configurar botones
        playAgainButton.onClick.AddListener(PlayAgain);
        backToMenuButton.onClick.AddListener(BackToMenu);
        
        // Cargar y mostrar resultados
        LoadGameResults();
    }
    
    // Cargar resultados del juego anterior
    void LoadGameResults()
    {
        // Obtener datos guardados de GameScene
        string playerAvatar = PlayerPrefs.GetString("SelectedAvatar", "HAPPY");
        int finalScore = PlayerPrefs.GetInt("FinalScore", 0);
        
        // Actualizar UI con resultados
        celebrationAvatar.text = playerAvatar;
        finalScoreText.text = "SCORE FINAL: " + finalScore;
        
        // Mensaje personalizado según el score
        string message = GetCelebrationMessage(playerAvatar, finalScore);
        celebrationMessage.text = message;
        
        Debug.Log("Resultados cargados - Avatar: " + playerAvatar + ", Score: " + finalScore);
    }
    
    // Generar mensaje de celebración personalizado
    string GetCelebrationMessage(string avatar, int score)
    {
        if (score >= 50)
        {
            return "¡INCREÍBLE! ¡HAS DESTRUIDO A " + avatar + " " + score + " VECES!";
        }
        else if (score >= 25)
        {
            return "¡EXCELENTE! ¡" + avatar + " HA SUFRIDO " + score + " GOLPES!";
        }
        else if (score >= 10)
        {
            return "¡BIEN HECHO! ¡" + avatar + " HA RECIBIDO " + score + " GOLPES!";
        }
        else
        {
            return "¡" + avatar + " HA SOBREVIVIDO CON SOLO " + score + " GOLPES!";
        }
    }
    
    // Jugar de nuevo - reiniciar GameScene
    void PlayAgain()
    {
        Debug.Log("Jugando de nuevo...");
        SceneManager.LoadScene("GameScene");
    }
    
    // Volver al menú principal
    void BackToMenu()
    {
        Debug.Log("Regresando al menú principal...");
        SceneManager.LoadScene("MainMenu");
    }
}