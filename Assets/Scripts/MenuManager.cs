using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("Botones Menú Principal")]
    public Button createRoomButton;
    public Button joinRoomButton;
    
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
        // Configurar botones principales
        createRoomButton.onClick.AddListener(ShowAvatarSelectionFromCreate);
        joinRoomButton.onClick.AddListener(ShowAvatarSelectionFromJoin);
        
        // Configurar botones de navegación
        backButton1.onClick.AddListener(ShowMainMenu);
        backButton2.onClick.AddListener(ShowMainMenu);
        connectButton.onClick.AddListener(ConnectToRoom);
        startGameButton.onClick.AddListener(StartGame);
        
        // Mostrar menú principal al iniciar
        ShowMainMenu();
    }
    
    // Mostrar menú principal
    void ShowMainMenu()
    {
        mainMenuCanvas.gameObject.SetActive(true);
        createRoomCanvas.gameObject.SetActive(false);
        joinRoomCanvas.gameObject.SetActive(false);
        avatarSelectionCanvas.gameObject.SetActive(false);
    }
    
    // Ir a avatar selection desde CREAR SALA
    void ShowAvatarSelectionFromCreate()
    {
        PlayerPrefs.SetString("LastMenuAction", "CreateRoom");
        ShowAvatarSelection();
    }
    
    // Ir a avatar selection desde UNIRSE A SALA
    void ShowAvatarSelectionFromJoin()
    {
        PlayerPrefs.SetString("LastMenuAction", "JoinRoom");
        ShowAvatarSelection();
    }
    
    // Mostrar pantalla de selección de avatar
    void ShowAvatarSelection()
    {
        mainMenuCanvas.gameObject.SetActive(false);
        createRoomCanvas.gameObject.SetActive(false);
        joinRoomCanvas.gameObject.SetActive(false);
        avatarSelectionCanvas.gameObject.SetActive(true);
    }
    
    // Mostrar pantalla crear sala
    void ShowCreateRoom()
    {
        mainMenuCanvas.gameObject.SetActive(false);
        createRoomCanvas.gameObject.SetActive(true);
        joinRoomCanvas.gameObject.SetActive(false);
        avatarSelectionCanvas.gameObject.SetActive(false);
        
        // Generar código de sala
        GenerateRoomCode();
    }
    
    // Mostrar pantalla unirse a sala
    void ShowJoinRoom()
    {
        mainMenuCanvas.gameObject.SetActive(false);
        createRoomCanvas.gameObject.SetActive(false);
        joinRoomCanvas.gameObject.SetActive(true);
        avatarSelectionCanvas.gameObject.SetActive(false);
    }
    
    // Generar código de sala - INTEGRADO CON MULTIPLAYER
    void GenerateRoomCode()
    {
        Debug.Log("GenerateRoomCode() llamado");
        
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
    
    // Conectar a sala - INTEGRADO CON MULTIPLAYER
    void ConnectToRoom()
    {
        Debug.Log("ConnectToRoom() llamado");
        
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
    
    // Obtener código del input field en JoinRoom
    string GetRoomCodeFromInput()
    {
        // Buscar input field en JoinRoom canvas
        var inputField = GameObject.Find("RoomCodeInput")?.GetComponent<TMPro.TMP_InputField>();
        
        if (inputField != null)
        {
            return inputField.text;
        }
        
        Debug.LogWarning("RoomCodeInput no encontrado, usando código de prueba");
        return "1234"; // Fallback para testing
    }
    
    // Empezar juego desde CreateRoom
    void StartGame()
    {
        Debug.Log("StartGame() llamado desde CreateRoom");
        SceneManager.LoadScene("GameScene");
    }
    
    // FUNCIONES PÚBLICAS para AvatarSelector
    public void ShowCreateRoomPublic()
    {
        ShowCreateRoom();
    }
    
    public void ShowJoinRoomPublic()
    {
        ShowJoinRoom();
    }
}