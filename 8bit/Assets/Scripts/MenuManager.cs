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
    public Canvas avatarSelectionCanvas;  // ← Añadir esta línea
    
    [Header("Botones Otras Pantallas")]
    public Button backButton1;
    public Button backButton2;
    public Button connectButton;
    
    [Header("Textos")]
    public TextMeshProUGUI roomCodeDisplay;
    
    void Start()
    {
        createRoomButton.onClick.AddListener(ShowAvatarSelection);  // ← Cambiar aquí
        joinRoomButton.onClick.AddListener(ShowAvatarSelection);    // ← Cambiar aquí
        backButton1.onClick.AddListener(ShowMainMenu);
        backButton2.onClick.AddListener(ShowMainMenu);
        connectButton.onClick.AddListener(ConnectToRoom);
        
        ShowMainMenu();
    }
    
    void ShowMainMenu()
    {
        mainMenuCanvas.gameObject.SetActive(true);
        createRoomCanvas.gameObject.SetActive(false);
        joinRoomCanvas.gameObject.SetActive(false);
        avatarSelectionCanvas.gameObject.SetActive(false);  // ← Añadir línea
    }
    
    void ShowAvatarSelection()  // ← Nueva función
    {
        mainMenuCanvas.gameObject.SetActive(false);
        createRoomCanvas.gameObject.SetActive(false);
        joinRoomCanvas.gameObject.SetActive(false);
        avatarSelectionCanvas.gameObject.SetActive(true);
    }
    
    void ShowCreateRoom()
    {
        mainMenuCanvas.gameObject.SetActive(false);
        createRoomCanvas.gameObject.SetActive(true);
        joinRoomCanvas.gameObject.SetActive(false);
        avatarSelectionCanvas.gameObject.SetActive(false);  // ← Añadir línea
        
        GenerateRoomCode();
    }
    
    void ShowJoinRoom()
    {
        mainMenuCanvas.gameObject.SetActive(false);
        createRoomCanvas.gameObject.SetActive(false);
        joinRoomCanvas.gameObject.SetActive(true);
        avatarSelectionCanvas.gameObject.SetActive(false);  // ← Añadir línea
    }
    
    void GenerateRoomCode()
    {
        int code = Random.Range(1000, 9999);
        roomCodeDisplay.text = code.ToString();
        Debug.Log("Código generado: " + code);
    }
    
    void ConnectToRoom()
    {
        Debug.Log("Intentando conectar a sala...");
        SceneManager.LoadScene("GameScene");
    }
}