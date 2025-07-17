using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Button backToMenuButton;
    public Image opponentAvatar;
    public TextMeshProUGUI playerScore;
    public TextMeshProUGUI gameTimer;
    
    [Header("Game State")]
    public int score = 0;
    public float timeLeft = 60f;
    
    void Start()
    {
        backToMenuButton.onClick.AddListener(BackToMenu);
        
        // Hacer avatar clickeable
        Button avatarButton = opponentAvatar.gameObject.AddComponent<Button>();
        avatarButton.onClick.AddListener(HitAvatar);
        
        UpdateUI();
        StartTimer();
    }
    
    void Update()
    {
        // Timer countdown
        if (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            UpdateUI();
        }
        else
        {
            // Game Over
            timeLeft = 0;
            Debug.Log("¡Tiempo terminado! Score final: " + score);
        }
    }
    
    void HitAvatar()
    {
        if (timeLeft > 0)
        {
            score++;
            Debug.Log("¡Golpe! Score: " + score);
            
            // Efecto visual simple
            StartCoroutine(HitEffect());
            UpdateUI();
        }
    }
    
    System.Collections.IEnumerator HitEffect()
    {
        // Cambiar color temporalmente
        Color originalColor = opponentAvatar.color;
        opponentAvatar.color = Color.white;
        
        yield return new WaitForSeconds(0.1f);
        
        opponentAvatar.color = originalColor;
    }
    
    void UpdateUI()
    {
        playerScore.text = "TU SCORE: " + score;
        gameTimer.text = Mathf.Ceil(timeLeft) + "s";
    }
    
    void StartTimer()
    {
        timeLeft = 60f;
    }
    
    void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}