using UnityEngine;
using UnityEngine.UI;
using TMPro;  // ← Añadir esta línea

public class MenuManager : MonoBehaviour
{
    [Header("Botones Menú Principal")]
    public Button createRoomButton;
    public Button joinRoomButton;
    
    [Header("Canvas")]
    public Canvas mainMenuCanvas;
    public Canvas createRoomCanvas;
    public Canvas joinRoomCanvas;
    
    [Header("Botones Otras Pantallas")]
    public Button backButton1;
    public Button backButton2;
    public Button connectButton;
    
    [Header("Textos")]
    public TextMeshProUGUI roomCodeDisplay;  // ← Añadir esta línea
    
    void Start()
    {
        createRoomButton.onClick.AddListener(ShowCreateRoom);
        joinRoomButton.onClick.AddListener(ShowJoinRoom);
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
    }
    
    void ShowCreateRoom()
    {
        mainMenuCanvas.gameObject.SetActive(false);
        createRoomCanvas.gameObject.SetActive(true);
        joinRoomCanvas.gameObject.SetActive(false);
        
        GenerateRoomCode();
    }
    
    void ShowJoinRoom()
    {
        mainMenuCanvas.gameObject.SetActive(false);
        createRoomCanvas.gameObject.SetActive(false);
        joinRoomCanvas.gameObject.SetActive(true);
    }
    
    void GenerateRoomCode()
    {
        int code = Random.Range(1000, 9999);
        roomCodeDisplay.text = code.ToString();  // ← Cambiar esta línea
        Debug.Log("Código generado: " + code);
    }
    
    void ConnectToRoom()
    {
        Debug.Log("Intentando conectar a sala...");
    }
}