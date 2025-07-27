using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("Botones Menú Principal")]
    public Button createRoomButton;      // Ahora será "JUGAR CON AMIGOS" 
    public Button joinRoomButton;        // Ahora será "PELEA RÁPIDA"
    public Button trainingButton;        // ✅ NUEVO: "ENTRENAMIENTO"
    
    [Header("Canvas")]
    public Canvas mainMenuCanvas;
    public Canvas createRoomCanvas;
    public Canvas joinRoomCanvas;
    public Canvas avatarSelectionCanvas;
    
    [Header("Botones Otras Pantallas")]
    public Button backButton1;           // CreateRoom back button
    public Button backButton2;           // JoinRoom back button
    public Button connectButton;         // JoinRoom connect button
    public Button startGameButton;       // CreateRoom start game button
    
    [Header("Textos")]
    public TextMeshProUGUI roomCodeDisplay;  // Mostrar código de sala
    
    void Start()
    {
        // ✅ NUEVO FLOW: INVERTIDO LA LÓGICA
        // joinRoomButton → PELEA RÁPIDA (auto-matchmaking)
        // createRoomButton → JUGAR CON AMIGOS (crear sala + código)
        
        createRoomButton.onClick.AddListener(StartQuickMatch);       // "PELEA RÁPIDA" (arriba)
        joinRoomButton.onClick.AddListener(StartPlayWithFriends);
        trainingButton.onClick.AddListener(StartTraining);  
        
        // Configurar botones de navegación (sin cambios)
        backButton1.onClick.AddListener(ShowMainMenu);
        backButton2.onClick.AddListener(ShowMainMenu);
        connectButton.onClick.AddListener(ConnectToRoom);
        startGameButton.onClick.AddListener(StartGame);
        
        // Mostrar menú principal al iniciar
        ShowMainMenu();
    }
    
    // ✅ NUEVA FUNCIÓN: PELEA RÁPIDA (auto-matchmaking)
    void StartQuickMatch()
    {
        Debug.Log("=== PELEA RÁPIDA INICIADA ===");
        
        // Guardar modo para avatar selection
        PlayerPrefs.SetString("LastMenuAction", "QuickMatch"); // En lugar de "PlayWithFriends"
        ShowAvatarSelection();

    }
    
    // ✅ NUEVA FUNCIÓN: JUGAR CON AMIGOS (crear sala con código)  
    void StartPlayWithFriends()
    {
        Debug.Log("=== JUGAR CON AMIGOS INICIADO ===");
        
        // Guardar modo para avatar selection
        PlayerPrefs.SetString("LastMenuAction", "PlayWithFriends"); // En lugar de "QuickMatch"
        ShowAvatarSelection();
        
    }
    
    // Mostrar menú principal
    void ShowMainMenu()
    {
        mainMenuCanvas.gameObject.SetActive(true);
        createRoomCanvas.gameObject.SetActive(false);
        joinRoomCanvas.gameObject.SetActive(false);
        avatarSelectionCanvas.gameObject.SetActive(false);
    }
    
    // ❌ DEPRECATED: Ya no usamos estas funciones
    // void ShowAvatarSelectionFromCreate() - REMOVED
    // void ShowAvatarSelectionFromJoin() - REMOVED
    
    // Mostrar pantalla de selección de avatar
    void ShowAvatarSelection()
    {
        mainMenuCanvas.gameObject.SetActive(false);
        createRoomCanvas.gameObject.SetActive(false);
        joinRoomCanvas.gameObject.SetActive(false);
        avatarSelectionCanvas.gameObject.SetActive(true);
    }
    
    // Mostrar pantalla crear sala (para JUGAR CON AMIGOS)
    void ShowCreateRoom()
    {
        mainMenuCanvas.gameObject.SetActive(false);
        createRoomCanvas.gameObject.SetActive(true);
        joinRoomCanvas.gameObject.SetActive(false);
        avatarSelectionCanvas.gameObject.SetActive(false);
        
        // Generar código de sala
        GenerateRoomCode();
    }
    
    // ❌ DEPRECATED: Ya no necesitamos join room screen para quick match
    void ShowJoinRoom()
    {
        mainMenuCanvas.gameObject.SetActive(false);
        createRoomCanvas.gameObject.SetActive(false);
        joinRoomCanvas.gameObject.SetActive(true);
        avatarSelectionCanvas.gameObject.SetActive(false);
    }
    
    // Generar código de sala - SOLO para JUGAR CON AMIGOS
    void GenerateRoomCode()
    {
        Debug.Log("GenerateRoomCode() llamado para JUGAR CON AMIGOS");
        
        if (MultiplayerManager.Instance != null)
        {
            Debug.Log("MultiplayerManager encontrado, creando sala...");
            MultiplayerManager.Instance.CreateRoom();
        }
        else
        {
            Debug.Log("MultiplayerManager NO encontrado, usando código local");
            int code = Random.Range(1000, 9999);
            roomCodeDisplay.text = code.ToString();
            Debug.Log("Código generado (local): " + code);
        }
    }
    
    // ❌ DEPRECATED: Ya no necesitamos connect para quick match
    void ConnectToRoom()
    {
        Debug.Log("ConnectToRoom() llamado - DEPRECATED en nuevo flow");
        
        // Obtener código del input field
        string roomCode = GetRoomCodeFromInput();
        Debug.Log("Código introducido: " + roomCode);
        
        if (MultiplayerManager.Instance != null)
        {
            Debug.Log("Conectando via MultiplayerManager...");
            MultiplayerManager.Instance.JoinRoom(roomCode);
        }
        else
        {
            Debug.Log("MultiplayerManager no encontrado, yendo directo a GameScene");
            SceneManager.LoadScene("GameScene");
        }
    }
    
    // Obtener código del input field en JoinRoom (para backward compatibility)
    string GetRoomCodeFromInput()
    {
        var inputField = GameObject.Find("RoomCodeInput")?.GetComponent<TMPro.TMP_InputField>();
        
        if (inputField != null)
        {
            return inputField.text;
        }
        
        Debug.LogWarning("RoomCodeInput no encontrado, usando código de prueba");
        return "1234"; // Fallback para testing
    }
    
    // Empezar juego desde CreateRoom (JUGAR CON AMIGOS)
    // ✅ ACTUALIZAR: Empezar juego - Solo para JUGAR CON AMIGOS
    void StartGame()
    {
        string lastAction = PlayerPrefs.GetString("LastMenuAction", "");
    
        if (lastAction == "QuickMatch")
        {
            Debug.Log("PELEA RÁPIDA - No puede empezar sin oponente");
            // No hacer nada, debe esperar oponente
            return;
        }
    
        Debug.Log("StartGame() llamado desde JUGAR CON AMIGOS");
        SceneManager.LoadScene("GameScene");
    }


    // ✅ NUEVA FUNCIÓN: Iniciar entrenamiento
    void StartTraining()
    {
        Debug.Log("=== ENTRENAMIENTO INICIADO ===");
    
        // Guardar modo para avatar selection
        PlayerPrefs.SetString("LastMenuAction", "Training");
    
        // Ir directo a avatar selection
        ShowAvatarSelection();
    }     
    
    // FUNCIONES PÚBLICAS para AvatarSelector (actualizadas)
    public void ShowCreateRoomPublic()
    {
        ShowCreateRoom();
    }
    
    public void ShowJoinRoomPublic()
    {
        ShowJoinRoom();
    }
}