using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Firebase.Database;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Button backToMenuButton;                 // Botón volver al menú
    public Image opponentAvatar;                    // Avatar del oponente (imagen roja)
    public TextMeshProUGUI opponentAvatarText;      // Nombre del avatar enemigo
    public TextMeshProUGUI playerScore;             // Score del jugador
    public TextMeshProUGUI gameTimer;               // Timer del juego
    
    [Header("Game State")]
    public int score = 0;                           // Puntuación actual
    public float timeLeft = 60f;                    // Tiempo restante
    
    // Variables privadas
    private string selectedAvatar = "HAPPY";        // Avatar del jugador actual
    private string opponentAvatarName = "ROBOT";    // Avatar del enemigo
    private bool gameEnded = false;                 // Para evitar múltiples llamadas a EndGame
    private bool needsOpponentUpdate = false;       // Flag para main thread update
    private string pendingOpponentAvatar = "";      // Avatar enemigo pendiente
    
    void Start()
    {
        // Configurar botón de navegación
        backToMenuButton.onClick.AddListener(BackToMenu);
        
        // Cargar avatar seleccionado y enemigo
        LoadSelectedAvatar();
        
        // Hacer avatar clickeable para golpear
        Button avatarButton = opponentAvatar.gameObject.AddComponent<Button>();
        avatarButton.onClick.AddListener(HitAvatar);
        
        // Inicializar UI y timer
        UpdateUI();
        StartTimer();
        
        Debug.Log("GameManager iniciado correctamente");
    }
    
    void Update()
    {
        // Timer countdown
        if (timeLeft > 0 && !gameEnded)
        {
            timeLeft -= Time.deltaTime;
            UpdateUI();
        }
        else if (!gameEnded)
        {
            timeLeft = 0;
            gameEnded = true;
            Debug.Log("Timer llegó a 0, llamando EndGame()");
            EndGame();
        }
        
        // Actualizar avatar enemigo en main thread
        if (needsOpponentUpdate)
        {
            needsOpponentUpdate = false;
            opponentAvatarName = pendingOpponentAvatar;
            DisplayOpponentAvatar();
            Debug.Log("Avatar enemigo actualizado desde main thread: " + opponentAvatarName);
        }
    }
    
    // Cargar avatar del jugador y determinar enemigo
    void LoadSelectedAvatar()
    {
        // Obtener avatar del jugador actual
        selectedAvatar = PlayerPrefs.GetString("SelectedAvatar", "HAPPY");
        Debug.Log("Tu avatar: " + selectedAvatar);
        
        // Determinar avatar del ENEMIGO
        if (MultiplayerManager.Instance != null)
        {
            string roomId = MultiplayerManager.Instance.GetCurrentRoomId();
            bool isHost = MultiplayerManager.Instance.IsHost();
            
            Debug.Log("Modo multijugador - IsHost: " + isHost + ", RoomId: " + roomId);
            
            if (!string.IsNullOrEmpty(roomId))
            {
                // Obtener avatar del CONTRINCANTE desde Firebase
                GetOpponentAvatarFromFirebase(isHost, roomId);
            }
            else
            {
                Debug.LogWarning("RoomId vacío, usando avatar aleatorio");
                opponentAvatarName = GetRandomOpponentAvatar();
                DisplayOpponentAvatar();
            }
        }
        else
        {
            Debug.Log("Modo single player - auto-golpeo");
            // Solo jugador: golpear tu propio avatar
            opponentAvatarName = selectedAvatar;
            DisplayOpponentAvatar();
        }
    }
    
    // Obtener avatar del contrincante desde Firebase
    void GetOpponentAvatarFromFirebase(bool isHost, string roomId)
    {
        Debug.Log("Obteniendo avatar enemigo desde Firebase...");
        
        FirebaseDatabase.DefaultInstance.RootReference
            .Child("rooms").Child(roomId).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                var roomData = task.Result.Value as System.Collections.Generic.Dictionary<string, object>;
                
                // HOST ve avatar del GUEST, GUEST ve avatar del HOST
                string enemyAvatarField = isHost ? "guestAvatar" : "hostAvatar";
                
                if (roomData.ContainsKey(enemyAvatarField))
                {
                    string enemyAvatar = roomData[enemyAvatarField].ToString();
                    Debug.Log("Avatar enemigo obtenido de Firebase: " + enemyAvatar);
                    
                    // Programar actualización en main thread
                    pendingOpponentAvatar = enemyAvatar;
                    needsOpponentUpdate = true;
                }
                else
                {
                    Debug.LogError("Campo " + enemyAvatarField + " no encontrado en Firebase");
                    opponentAvatarName = GetRandomOpponentAvatar();
                    DisplayOpponentAvatar();
                }
            }
            else
            {
                Debug.LogError("Error obteniendo datos de sala desde Firebase");
                opponentAvatarName = GetRandomOpponentAvatar();
                DisplayOpponentAvatar();
            }
        });
    }
    
    // Generar avatar aleatorio para enemigo (fallback)
    string GetRandomOpponentAvatar()
    {
        string[] avatars = {"HAPPY", "ROBOT", "ALIEN", "TARGET", "FIRE", "STAR"};
        
        // Asegurar que enemigo tenga avatar diferente al jugador
        string opponentAvatar;
        do {
            opponentAvatar = avatars[Random.Range(0, avatars.Length)];
        } while (opponentAvatar == selectedAvatar);
        
        Debug.Log("Avatar enemigo aleatorio generado: " + opponentAvatar);
        return opponentAvatar;
    }
    
    // Mostrar avatar del enemigo en UI
    void DisplayOpponentAvatar()
    {
        if (opponentAvatarText != null)
        {
            opponentAvatarText.text = opponentAvatarName;
            Debug.Log("Avatar enemigo mostrado en UI: " + opponentAvatarName);
        }
        else
        {
            Debug.LogError("OpponentAvatarText no está asignado!");
        }
    }
    
    // Golpear avatar del enemigo
    void HitAvatar()
    {
        if (timeLeft > 0 && !gameEnded)
        {
            score++;
            Debug.Log("¡Golpe a " + opponentAvatarName + "! Score: " + score);
            
            // Sincronizar score con Firebase multijugador
            if (MultiplayerManager.Instance != null)
            {
                MultiplayerManager.Instance.SendHit(score);
                Debug.Log("Score enviado a Firebase: " + score);
            }
            
            // Efecto visual de golpe
            StartCoroutine(HitEffect());
            UpdateUI();
        }
    }
    
    // Efecto visual cuando golpeas
    System.Collections.IEnumerator HitEffect()
    {
        Color originalColor = opponentAvatar.color;
        
        // Flash rojo
        opponentAvatar.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        
        // Flash blanco
        opponentAvatar.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        
        // Restaurar color original
        opponentAvatar.color = originalColor;
    }
    
    // Actualizar interfaz de usuario
    void UpdateUI()
    {
        if (playerScore != null)
        {
            playerScore.text = "TU SCORE: " + score;
        }
        
        if (gameTimer != null)
        {
            gameTimer.text = Mathf.Ceil(timeLeft) + "s";
            
            // Cambiar color cuando queda poco tiempo
            if (timeLeft <= 10)
            {
                gameTimer.color = Color.red;
            }
            else
            {
                gameTimer.color = Color.white;
            }
        }
    }
    
    // Inicializar timer
    void StartTimer()
    {
        timeLeft = 60f;
        gameEnded = false;
    }
    
    // Terminar juego - llamado cuando timer llega a 0
    void EndGame()
    {
        Debug.Log("¡GAME OVER! Score final: " + score);
        
        // Guardar score para Results screen
        PlayerPrefs.SetInt("FinalScore", score);
        PlayerPrefs.Save();
        
        // Actualizar estado en Firebase
        if (MultiplayerManager.Instance != null)
        {
            MultiplayerManager.Instance.SendHit(score); // Score final
            // TODO: Implementar winner calculation
        }
        
        // Mostrar Results screen
        ShowResults();
    }
    
    // Mostrar pantalla de resultados
    void ShowResults()
    {
        Debug.Log("ShowResults() llamado");
        
        // Ocultar UI de GameScene
        HideGameUI();
        
        // Buscar ResultsManager incluso si está desactivado
        ResultsManager resultsManager = FindObjectOfType<ResultsManager>(true);
        Debug.Log("ResultsManager encontrado: " + (resultsManager != null));
        
        if (resultsManager != null)
        {
            // Activar ResultsCanvas
            resultsManager.transform.parent.gameObject.SetActive(true);
            Debug.Log("ResultsCanvas activado correctamente");
        }
        else
        {
            Debug.LogError("ResultsManager no encontrado en GameScene!");
            Invoke("BackToMenu", 2f);
        }
    }
    
    // Ocultar UI de GameScene
    void HideGameUI()
    {
        // Desactivar elementos principales de GameScene
        if (playerScore != null) playerScore.gameObject.SetActive(false);
        if (opponentAvatarText != null) opponentAvatarText.gameObject.SetActive(false);
        if (opponentAvatar != null) opponentAvatar.gameObject.SetActive(false);
        if (gameTimer != null) gameTimer.gameObject.SetActive(false);
        if (backToMenuButton != null) backToMenuButton.gameObject.SetActive(false);
        
        Debug.Log("GameScene UI ocultada");
    }
    
    // Volver al menú principal
    void BackToMenu()
    {
        Debug.Log("Regresando al menú principal...");
        SceneManager.LoadScene("MainMenu");
    }
}