using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class AvatarSelector : MonoBehaviour
{
    [Header("UI Elements")]
    public Button[] avatarButtons;
    public TextMeshProUGUI avatarPreview;
    public TextMeshProUGUI avatarDescription;
    public Button confirmButton;
    public Button backButton;
    
    [Header("Avatar Data")]
    public string[] avatarEmojis = {"HAPPY", "ROBOT", "ALIEN", "TARGET", "FIRE", "STAR"};
    public string[] avatarDescriptions = {
        "Cara feliz clásica",
        "Robot futurista",  
        "Alienígena espacial",
        "Objetivo de tiro",
        "Elemento de fuego",
        "Estrella brillante"
    };
    
    [Header("Navigation")]
    public Canvas mainMenuCanvas;
    public Canvas createRoomCanvas;
    public Canvas joinRoomCanvas;
    public Canvas avatarSelectionCanvas;
    
    // Variables privadas
    private string selectedAvatar = "HAPPY";
    private int selectedIndex = 0;
    private bool cameFromCreateRoom = false;
    
    void Start()
    {
        // Configurar botones de avatar
        for (int i = 0; i < avatarButtons.Length; i++)
        {
            int index = i; // Capturar índice para closure
            avatarButtons[i].onClick.AddListener(() => SelectAvatar(index));
        }
        
        // Configurar botones de navegación
        confirmButton.onClick.AddListener(ConfirmSelection);
        backButton.onClick.AddListener(BackToMenu);
        
        // Detectar de dónde venimos
        string lastAction = PlayerPrefs.GetString("LastMenuAction", "");
        cameFromCreateRoom = (lastAction == "CreateRoom");
        
        // Mostrar avatar por defecto
        UpdatePreview();
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
        if (avatarPreview != null)
        {
            avatarPreview.text = selectedAvatar;
            Debug.Log("Preview actualizado a: " + selectedAvatar);
        }
        
        if (avatarDescription != null && selectedIndex < avatarDescriptions.Length)
        {
            avatarDescription.text = avatarDescriptions[selectedIndex];
            Debug.Log("Descripción actualizada a: " + avatarDescriptions[selectedIndex]);
        }
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
    
   
   void ConfirmSelection()
    {
        PlayerPrefs.SetString("SelectedAvatar", selectedAvatar);
        PlayerPrefs.Save();
    
        Debug.Log("Avatar confirmado: " + selectedAvatar);
    
        string lastAction = PlayerPrefs.GetString("LastMenuAction", "");
        Debug.Log("LastMenuAction: " + lastAction);
    
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
        
        // AÑADIR: Generar código de sala
         GenerateRoomCodeFromAvatar();
        }
     else
        {
            Debug.LogError("CreateRoomCanvas es NULL!");
     }
    }

// NUEVA FUNCIÓN: Llamar a MenuManager para generar código
    void GenerateRoomCodeFromAvatar()
    {
      MenuManager menuManager = FindObjectOfType<MenuManager>();
      if (menuManager != null)
      {
        // Llamar función privada desde público
         menuManager.SendMessage("GenerateRoomCode");
          Debug.Log("GenerateRoomCode() llamado desde AvatarSelector");
        }
      else
     {
         Debug.LogError("MenuManager no encontrado!");
        }
    }








// Mostrar pantalla Unirse a Sala
    void ShowJoinRoom()
    {
      mainMenuCanvas.gameObject.SetActive(false);
     createRoomCanvas.gameObject.SetActive(false);
     joinRoomCanvas.gameObject.SetActive(true);
     avatarSelectionCanvas.gameObject.SetActive(false);
    
     Debug.Log("Mostrando JoinRoom screen");
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