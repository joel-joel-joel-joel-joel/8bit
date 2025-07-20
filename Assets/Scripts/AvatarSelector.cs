using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class AvatarSelector : MonoBehaviour
{
    [Header("UI Elements")]
    public Button[] avatarButtons;
    public Image avatarPreview;                     // CORREGIDO: Image en lugar de TextMeshProUGUI
    public TextMeshProUGUI avatarDescription;
    public Button confirmButton;
    public Button backButton;
    
    [Header("Avatar Data")]
    public string[] avatarEmojis = {"ALIEN", "DEMONIO", "DIABLO", "GATO", "HAPPY", "ROBOT"};
    public string[] avatarDescriptions = {
        "Alienígena espacial",       // ALIEN
        "Demonio travieso",          // DEMONIO  
        "Diablo intimidante",        // DIABLO
        "Gato adorable",             // GATO
        "Cara feliz clásica",        // HAPPY
        "Robot futurista"            // ROBOT
    };
    
    [Header("Navigation")]
    public Canvas mainMenuCanvas;
    public Canvas createRoomCanvas;
    public Canvas joinRoomCanvas;
    public Canvas avatarSelectionCanvas;
    
    [Header("Avatar Preview Sprites")]
    public Sprite[] alienSprites = new Sprite[4];
    public Sprite[] demonioSprites = new Sprite[4];
    public Sprite[] diabloSprites = new Sprite[4];
    public Sprite[] gatoSprites = new Sprite[4];
    public Sprite[] happySprites = new Sprite[4];
    public Sprite[] robotSprites = new Sprite[4];
    
    // Variables privadas
    private string selectedAvatar = "ALIEN";
    private int selectedIndex = 0;
    private Dictionary<string, Sprite[]> avatarSpritesMap;
    
    void Start()
    {
        // Setup avatar sprites mapping
        SetupAvatarSprites();
        
        // Configurar botones de avatar
        for (int i = 0; i < avatarButtons.Length; i++)
        {
            int index = i; // Capturar índice para closure
            avatarButtons[i].onClick.AddListener(() => SelectAvatar(index));
        }
        
        // Configurar botones de navegación
        confirmButton.onClick.AddListener(ConfirmSelection);
        backButton.onClick.AddListener(BackToMenu);
        
        // Mostrar avatar por defecto
        UpdatePreview();
    }
    
    // Setup mapping de avatares a sprites
    void SetupAvatarSprites()
    {
        avatarSpritesMap = new Dictionary<string, Sprite[]>
        {
            ["ALIEN"] = alienSprites,
            ["DEMONIO"] = demonioSprites,
            ["DIABLO"] = diabloSprites,
            ["GATO"] = gatoSprites,
            ["HAPPY"] = happySprites,
            ["ROBOT"] = robotSprites
        };
        
        Debug.Log("Avatar sprites mapping configurado para preview");
    }
    
    // Seleccionar avatar específico
    void SelectAvatar(int index)
    {
        Debug.Log("SelectAvatar llamado con index: " + index);
        
        if (index < avatarEmojis.Length && avatarEmojis[index] != null)
        {
            selectedAvatar = avatarEmojis[index];
            selectedIndex = index;
            
            Debug.Log("Avatar seleccionado: " + selectedAvatar);
            
            UpdatePreview();
            HighlightButton(index);
        }
        else
        {
            Debug.LogError("Error: index fuera de rango o emoji null");
        }
    }
    
    // Actualizar preview y descripción
    void UpdatePreview()
    {
        // CORREGIDO: Solo imagen, no texto
        if (avatarPreview != null)
        {
            Sprite previewSprite = GetPreviewSprite(selectedAvatar);
            if (previewSprite != null)
            {
                avatarPreview.sprite = previewSprite;
                Debug.Log("Preview image actualizado a: " + selectedAvatar);
            }
            else
            {
                Debug.LogError("No se encontró sprite para preview: " + selectedAvatar);
            }
        }
        else
        {
            Debug.LogError("avatarPreview es NULL - no asignado en Inspector");
        }
        
        if (avatarDescription != null && selectedIndex < avatarDescriptions.Length)
        {
            avatarDescription.text = avatarDescriptions[selectedIndex];
            Debug.Log("Descripción actualizada a: " + avatarDescriptions[selectedIndex]);
        }
    }
    
    // Obtener sprite para preview (fase 1)
    Sprite GetPreviewSprite(string avatarName)
    {
        if (avatarSpritesMap.ContainsKey(avatarName))
        {
            Sprite[] sprites = avatarSpritesMap[avatarName];
            if (sprites.Length > 0 && sprites[0] != null)
            {
                return sprites[0]; // Fase 1 (perfect) para preview
            }
        }
        
        Debug.LogWarning("No se encontró sprite para avatar: " + avatarName);
        return null;
    }
    
    // Resaltar botón seleccionado
    void HighlightButton(int selectedIndex)
    {
        for (int i = 0; i < avatarButtons.Length; i++)
        {
            // Botón seleccionado: verde, otros: blanco
            Color buttonColor = (i == selectedIndex) ? Color.green : Color.white;
            avatarButtons[i].GetComponent<Image>().color = buttonColor;
        }
    }
    
    // Confirmar selección de avatar
    void ConfirmSelection()
    {
        // Guardar avatar seleccionado en PlayerPrefs
        PlayerPrefs.SetString("SelectedAvatar", selectedAvatar);
        PlayerPrefs.Save();
        
        Debug.Log("Avatar confirmado: " + selectedAvatar);
        
        // Ir a la pantalla correcta según el flujo
        string lastAction = PlayerPrefs.GetString("LastMenuAction", "");
        
        if (lastAction == "CreateRoom")
        {
            Debug.Log("Mostrando CreateRoom...");
            ShowCreateRoom();
        }
        else
        {
            Debug.Log("Mostrando JoinRoom...");
            ShowJoinRoom();
        }
    }
    
    // Mostrar pantalla Crear Sala
    void ShowCreateRoom()
    {
        Debug.Log("ShowCreateRoom llamado");
        
        if (createRoomCanvas != null)
        {
            mainMenuCanvas.gameObject.SetActive(false);
            createRoomCanvas.gameObject.SetActive(true);
            joinRoomCanvas.gameObject.SetActive(false);
            avatarSelectionCanvas.gameObject.SetActive(false);
            Debug.Log("CreateRoom activado");
        }
        else
        {
            Debug.LogError("CreateRoomCanvas es NULL!");
        }
    }
    
    // Mostrar pantalla Unirse a Sala
    void ShowJoinRoom()
    {
        mainMenuCanvas.gameObject.SetActive(false);
        createRoomCanvas.gameObject.SetActive(false);
        joinRoomCanvas.gameObject.SetActive(true);
        avatarSelectionCanvas.gameObject.SetActive(false);
    }
    
    // Volver al menú principal
    void BackToMenu()
    {
        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.gameObject.SetActive(true);
            createRoomCanvas.gameObject.SetActive(false);
            joinRoomCanvas.gameObject.SetActive(false);
            avatarSelectionCanvas.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("MainMenuCanvas no está asignado en AvatarSelector!");
        }
    }
}