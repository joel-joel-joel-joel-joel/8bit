using UnityEngine;
using Firebase.Database;
using System.Collections.Generic;
using TMPro;

public class MultiplayerManager : MonoBehaviour
{
    [Header("Multiplayer Settings")]
    public static MultiplayerManager Instance;
    
    [Header("UI References")]
    public TextMeshProUGUI roomCodeDisplayText;
    
    private DatabaseReference database;
    private string currentRoomId;
    private string playerId;
    private bool isHost = false;
    
    // Variables para main thread operations
    private bool needsUIUpdate = false;
    private string pendingRoomCode = "";
    private bool needsSceneChange = false;  // ← NUEVO
    
    void Awake()
    {
        Debug.Log("MultiplayerManager Awake() called");
        
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("✅ MultiplayerManager singleton created");
        }
        else
        {
            Debug.Log("❌ MultiplayerManager duplicate detected, destroying");
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        database = FirebaseDatabase.DefaultInstance.RootReference;
        playerId = "Player_" + System.DateTime.Now.Ticks.ToString().Substring(10);
        
        Debug.Log("🎮 MultiplayerManager iniciado");
        Debug.Log("👤 Player ID: " + playerId);
        
        FindRoomCodeDisplay();
    }
    
    void Update()
    {
        // UI update en main thread
        if (needsUIUpdate)
        {
            needsUIUpdate = false;
            Debug.Log("Actualizando UI desde main thread con código: " + pendingRoomCode);
            
            currentRoomId = pendingRoomCode;
            UpdateRoomCodeDisplay();
        }
        
        // NUEVO: Scene change en main thread
        if (needsSceneChange)
        {
            needsSceneChange = false;
            Debug.Log("Cambiando a GameScene desde main thread...");
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        }
    }
    
    void FindRoomCodeDisplay()
    {
        var allTexts = FindObjectsOfType<TextMeshProUGUI>();
        Debug.Log("=== TODOS LOS TEXTMESHPRO ENCONTRADOS ===");
        foreach (var text in allTexts)
        {
            Debug.Log("Nombre: " + text.name + " | Texto: '" + text.text + "' | Activo: " + text.gameObject.activeInHierarchy);
        }
    }
    
    public void CreateRoom()
    {
        Debug.Log("=== INICIANDO CreateRoom() ===");
        
        string newRoomId = Random.Range(1000, 9999).ToString();
        isHost = true;
        
        Debug.Log("Código generado: " + newRoomId);
        
        string selectedAvatar = PlayerPrefs.GetString("SelectedAvatar", "HAPPY");
        Debug.Log("Avatar seleccionado: " + selectedAvatar);
        
        var roomData = new Dictionary<string, object>
        {
            ["host"] = playerId,
            ["hostAvatar"] = selectedAvatar,
            ["guest"] = "",
            ["guestAvatar"] = "",
            ["status"] = "waiting",
            ["gameAvatar"] = selectedAvatar,
            ["hostScore"] = 0,
            ["guestScore"] = 0,
            ["lastUpdate"] = ServerValue.Timestamp
        };
        
        Debug.Log("Datos preparados, enviando a Firebase...");
        
        database.Child("rooms").Child(newRoomId).SetValueAsync(roomData).ContinueWith(task => 
        {
            Debug.Log("=== FIREBASE TASK COMPLETADO ===");
            Debug.Log("Task.IsCompleted: " + task.IsCompleted);
            Debug.Log("Task.IsCompletedSuccessfully: " + task.IsCompletedSuccessfully);
            Debug.Log("Task.IsFaulted: " + task.IsFaulted);
            Debug.Log("Task.IsCanceled: " + task.IsCanceled);
            
            if (task.IsCompleted && !task.IsFaulted)
            {
                Debug.Log("✅ SALA CREADA EXITOSAMENTE: " + newRoomId);
                
                pendingRoomCode = newRoomId;
                needsUIUpdate = true;
                Debug.Log("UI update programado para main thread");
            }
            else if (task.IsFaulted)
            {
                Debug.LogError("❌ ERROR CREANDO SALA: " + task.Exception);
                if (task.Exception.InnerException != null)
                {
                    Debug.LogError("Inner Exception: " + task.Exception.InnerException);
                }
            }
            else
            {
                Debug.LogError("❌ TASK NO COMPLETADO CORRECTAMENTE");
            }
        });
        
        Debug.Log("SetValueAsync llamado, esperando resultado...");
    }
    
    public void JoinRoom(string roomCode)
    {
        Debug.Log("Intentando unirse a sala: " + roomCode);
        
        currentRoomId = roomCode;
        isHost = false;

        string selectedAvatar = PlayerPrefs.GetString("SelectedAvatar", "HAPPY");
        Debug.Log("Avatar pre-obtenido: " + selectedAvatar);
        
        database.Child("rooms").Child(roomCode).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                var roomData = task.Result.Value as Dictionary<string, object>;
                string roomStatus = roomData["status"].ToString();
                string guest = roomData["guest"].ToString();

                Debug.Log("=== SALA ENCONTRADA ===");
                Debug.Log("Room Status: " + roomStatus);
                Debug.Log("Guest: '" + guest + "'");
                Debug.Log("Guest empty?: " + string.IsNullOrEmpty(guest));
                Debug.Log("Status is waiting?: " + (roomStatus == "waiting"));

                if (roomStatus == "waiting" && string.IsNullOrEmpty(guest))
                {
                    Debug.Log("✅ Sala disponible, procediendo a unirse...");
                    JoinAvailableRoom(roomCode, selectedAvatar);
                }
                else
                {
                    Debug.LogError("❌ Sala no disponible - Status: " + roomStatus + ", Guest: '" + guest + "'");
                }
            }
            else
            {
                Debug.LogError("❌ Sala no encontrada: " + roomCode);
            }
        });
    }
    
    private void JoinAvailableRoom(string roomCode, string selectedAvatar)
    {
        Debug.Log("=== JoinAvailableRoom() INICIADO ===");
        Debug.Log("RoomCode recibido: " + roomCode);
        Debug.Log("Avatar recibido como parámetro: " + selectedAvatar);
        Debug.Log("PlayerId actual: " + playerId);
        Debug.Log("Database es null?: " + (database == null));
        Debug.Log("Continuando con updates...");
        
        Debug.Log("Uniéndose a sala: " + roomCode + " con avatar: " + selectedAvatar);
        
        var updates = new Dictionary<string, object>
        {
            ["guest"] = playerId,
            ["guestAvatar"] = selectedAvatar,
            ["status"] = "playing"
        };
        
        Debug.Log("Datos para actualizar preparados:");
        Debug.Log("- Guest: " + playerId);
        Debug.Log("- Guest Avatar: " + selectedAvatar);
        Debug.Log("- Status: playing");
        Debug.Log("Enviando UpdateChildrenAsync...");
        
        database.Child("rooms").Child(roomCode).UpdateChildrenAsync(updates).ContinueWith(task =>
        {
            Debug.Log("=== UpdateChildrenAsync COMPLETADO ===");
            Debug.Log("Task.IsCompleted: " + task.IsCompleted);
            Debug.Log("Task.IsFaulted: " + task.IsFaulted);
            
            if (task.IsCompleted && !task.IsFaulted)
            {
                Debug.Log("🎮 Unido a sala exitosamente: " + roomCode);
                StartMultiplayerGame();
            }
            else
            {
                Debug.LogError("Error uniéndose a sala: " + task.Exception);
            }
        });
        
        Debug.Log("UpdateChildrenAsync enviado, esperando respuesta...");
    }
    
    private void UpdateRoomCodeDisplay()
    {
        Debug.Log("UpdateRoomCodeDisplay llamado con código: " + currentRoomId);
        
        if (roomCodeDisplayText != null)
        {
            roomCodeDisplayText.text = currentRoomId;
            Debug.Log("✅ Código actualizado via referencia directa: " + currentRoomId);
        }
        else
        {
            Debug.LogWarning("⚠️ roomCodeDisplayText no está asignado en Inspector");
            
            var roomCodeText = GameObject.Find("roomCodeDisplay")?.GetComponent<TextMeshProUGUI>();
            if (roomCodeText != null)
            {
                roomCodeText.text = currentRoomId;
                Debug.Log("✅ Código actualizado via GameObject.Find(): " + currentRoomId);
            }
            else
            {
                Debug.LogError("❌ roomCodeDisplay no encontrado por ningún método");
                
                var allTexts = FindObjectsOfType<TextMeshProUGUI>();
                Debug.Log("Buscando en " + allTexts.Length + " elementos TextMeshProUGUI activos:");
                foreach (var text in allTexts)
                {
                    Debug.Log("- " + text.name + " (texto: '" + text.text + "')");
                    if (text.name == "roomCodeDisplay")
                    {
                        text.text = currentRoomId;
                        Debug.Log("✅ Código actualizado via FindObjectsOfType: " + currentRoomId);
                        break;
                    }
                }
            }
        }
    }
    
    [ContextMenu("Test Update UI")]
    public void TestUpdateUI()
    {
        currentRoomId = "TEST";
        UpdateRoomCodeDisplay();
    }
    
    // ACTUALIZADO: StartMultiplayerGame() con main thread flag
    private void StartMultiplayerGame()
    {
        Debug.Log("Iniciando juego multijugador...");
        
        // Marcar para cambio de escena en main thread
        needsSceneChange = true;
    }
    
    public void SendHit(int newScore)
    {
        if (string.IsNullOrEmpty(currentRoomId)) return;
        
        string scoreField = isHost ? "hostScore" : "guestScore";
        database.Child("rooms").Child(currentRoomId).Child(scoreField).SetValueAsync(newScore);
        
        Debug.Log("Golpe enviado - Score: " + newScore);
    }
    
    public void StartListeningToRoom()
    {
        if (string.IsNullOrEmpty(currentRoomId)) return;
        
        database.Child("rooms").Child(currentRoomId).ValueChanged += HandleRoomDataChanged;
        Debug.Log("Escuchando cambios en sala: " + currentRoomId);
    }
    
    private void HandleRoomDataChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("Error en listener: " + args.DatabaseError.Message);
            return;
        }
        
        var roomData = args.Snapshot.Value as Dictionary<string, object>;
        if (roomData != null)
        {
            Debug.Log("Datos de sala actualizados en tiempo real");
        }
    }
    
    public void LeaveRoom()
    {
        if (string.IsNullOrEmpty(currentRoomId)) return;
        
        database.Child("rooms").Child(currentRoomId).ValueChanged -= HandleRoomDataChanged;
        database.Child("rooms").Child(currentRoomId).Child("status").SetValueAsync("finished");
        
        Debug.Log("Sala abandonada: " + currentRoomId);
        currentRoomId = "";
    }
    
    public string GetCurrentRoomId() => currentRoomId;
    public bool IsHost() => isHost;
    public string GetPlayerId() => playerId;
    
    void OnDestroy()
    {
        LeaveRoom();
    }
}