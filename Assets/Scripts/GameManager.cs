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
    public Image opponentAvatar;                    // Avatar del oponente (imagen)
    public TextMeshProUGUI opponentAvatarText;      // Nombre del avatar enemigo
    public TextMeshProUGUI playerScore;             // Score del jugador
    public TextMeshProUGUI gameTimer;               // Timer del juego
    
    [Header("Game State")]
    public int score = 0;                           // Puntuación actual
    public float timeLeft = 60f;                    // Tiempo restante
    
    [Header("Avatar Sprites")]
    public Sprite[] viejitoSprites = new Sprite[4];    // Fase 1-4
    public Sprite[] happySprites = new Sprite[4];
    public Sprite[] robotSprites = new Sprite[4];
    public Sprite[] alienSprites = new Sprite[4];
    public Sprite[] gatoSprites = new Sprite[4];
    public Sprite[] demonioSprites = new Sprite[4];
    public Sprite[] diabloSprites = new Sprite[4];
    
    // Variables privadas
    private string selectedAvatar = "HAPPY";        // Avatar del jugador actual
    private string opponentAvatarName = "ROBOT";    // Avatar del enemigo
    private bool gameEnded = false;                 // Para evitar múltiples llamadas a EndGame
    private bool needsOpponentUpdate = false;       // Flag para main thread update
    private string pendingOpponentAvatar = "";      // Avatar enemigo pendiente
    
    // Variables para damage system
    private int currentDamagePhase = 0;             // 0=perfect, 1=bruised, 2=battered, 3=destroyed
    private int hitCount = 0;                       // Número total de golpes
    private Dictionary<string, Sprite[]> avatarSpritesMap;  // Mapping de avatares a sprites
    
    void Start()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("PlayerPrefs limpiados!");

        if (!IsValidAvatar(PlayerPrefs.GetString("SelectedAvatar", "HAPPY")))
        {
            PlayerPrefs.SetString("SelectedAvatar", "HAPPY");
        }
        // Configurar botón de navegación
        backToMenuButton.onClick.AddListener(BackToMenu);
        
        // Setup avatar sprites mapping
        SetupAvatarSprites();
        
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
    
    // Setup mapping de avatares a sprites
    void SetupAvatarSprites()
    {
        avatarSpritesMap = new Dictionary<string, Sprite[]>
        {
            ["VIEJITO"] = viejitoSprites,
            ["HAPPY"] = happySprites,
            ["ROBOT"] = robotSprites,
            ["ALIEN"] = alienSprites,
            ["GATO"] = gatoSprites,
            ["DEMONIO"] = demonioSprites,
            ["DIABLO"] = diabloSprites
        };
        
        Debug.Log("Avatar sprites mapping configurado");
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
            Debug.Log("Modo single player - avatar aleatorio");
            // Solo jugador: avatar aleatorio
            opponentAvatarName = GetRandomOpponentAvatar();
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
        string[] avatars = {"VIEJITO", "HAPPY", "ROBOT", "ALIEN", "GATO", "DEMONIO", "DIABLO"};
        
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
        }
        
        // NUEVO: Cambiar sprite según damage phase
        if (opponentAvatar != null && avatarSpritesMap.ContainsKey(opponentAvatarName))
        {
            Sprite[] sprites = avatarSpritesMap[opponentAvatarName];
            if (sprites.Length > currentDamagePhase && sprites[currentDamagePhase] != null)
            {
                opponentAvatar.sprite = sprites[currentDamagePhase];
                Debug.Log("Avatar sprite cambiado: " + opponentAvatarName + " fase " + currentDamagePhase);
            }
            else
            {
                Debug.LogError("Sprite no encontrado para " + opponentAvatarName + " fase " + currentDamagePhase);
            }
        }
        else
        {
            Debug.LogError("Avatar " + opponentAvatarName + " no encontrado en sprites mapping");
        }
    }
    
    // Golpear avatar del enemigo
    void HitAvatar()
    {
        if (timeLeft > 0 && !gameEnded)
        {
            score++;
            hitCount++;
            
            // Calcular damage phase basado en hits
            int newDamagePhase = CalculateDamagePhase(hitCount);
            
            if (newDamagePhase != currentDamagePhase)
            {
                currentDamagePhase = newDamagePhase;
                DisplayOpponentAvatar();  // Actualizar sprite
                Debug.Log("¡DAMAGE PHASE CHANGE! Fase: " + currentDamagePhase);
            }
            
            Debug.Log("¡Golpe a " + opponentAvatarName + "! Hits: " + hitCount + ", Score: " + score + ", Fase: " + currentDamagePhase);
            
            // Sincronizar score con Firebase multijugador
            if (MultiplayerManager.Instance != null)
            {
                MultiplayerManager.Instance.SendHit(score);
            }
            
            // Efecto visual de golpe
            StartCoroutine(HitEffect());
            UpdateUI();
        }
    }
    
    // NUEVA FUNCIÓN: Calcular fase de damage
    int CalculateDamagePhase(int hits)
    {
        if (hits <= 5) return 0;        // Perfect (0-5 golpes)
        else if (hits <= 15) return 1; // Bruised (6-15 golpes)
        else if (hits <= 25) return 2; // Battered (16-25 golpes)
        else return 3;                  // Destroyed (26+ golpes)
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
        
        // Reset damage system
        currentDamagePhase = 0;
        hitCount = 0;
    }
    
    // Terminar juego - llamado cuando timer llega a 0
    void EndGame()
    {
        Debug.Log("¡GAME OVER! Score final: " + score + ", Hits totales: " + hitCount + ", Damage final: " + currentDamagePhase);
        
        // Guardar score para Results screen
        PlayerPrefs.SetInt("FinalScore", score);
        PlayerPrefs.SetInt("FinalHits", hitCount);
        PlayerPrefs.SetInt("FinalDamage", currentDamagePhase);
        PlayerPrefs.Save();
        
        // Actualizar estado en Firebase
        if (MultiplayerManager.Instance != null)
        {
            MultiplayerManager.Instance.SendHit(score); // Score final
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

    bool IsValidAvatar(string avatar)
    {
        string[] validAvatars = {"VIEJITO", "HAPPY", "ROBOT", "ALIEN", "GATO", "DEMONIO", "DIABLO"};
        return System.Array.Exists(validAvatars, x => x == avatar);
    }


}