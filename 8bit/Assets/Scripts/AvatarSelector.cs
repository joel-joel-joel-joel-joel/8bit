using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class AvatarSelector : MonoBehaviour
{
    [Header("UI Elements")]
    public Button[] avatarButtons;
    public TextMeshProUGUI avatarPreview;        // Preview del emoji/nombre
    public TextMeshProUGUI avatarDescription;    // ← NUEVA: Descripción del avatar
    public Button confirmButton;
    public Button backButton;
    
    [Header("Avatar Data")]
    public string[] avatarEmojis = {"HAPPY", "ROBOT", "ALIEN", "TARGET", "FIRE", "STAR"};
    public string[] avatarDescriptions = {       // ← NUEVO: Array de descripciones
        "Cara feliz clásica",
        "Robot futurista",  
        "Alienígena espacial",
        "Objetivo de tiro",
        "Elemento de fuego",
        "Estrella brillante"
    };
    private string selectedAvatar = "HAPPY";
    private int selectedIndex = 0;               // ← NUEVO: Índice seleccionado
    
    [Header("Navigation")]
    public Canvas mainMenuCanvas;
    public Canvas createRoomCanvas;
    public Canvas joinRoomCanvas;
    public Canvas avatarSelectionCanvas;
    
    void Start()
    {
        for (int i = 0; i < avatarButtons.Length; i++)
        {
            int index = i;
            avatarButtons[i].onClick.AddListener(() => SelectAvatar(index));
        }
        
        confirmButton.onClick.AddListener(ConfirmSelection);
        backButton.onClick.AddListener(BackToMenu);
        
        UpdatePreview();
    }
    
    void SelectAvatar(int index)
    {
        Debug.Log("SelectAvatar llamado con index: " + index);
        
        if (index < avatarEmojis.Length && avatarEmojis[index] != null)
        {
            selectedAvatar = avatarEmojis[index];
            selectedIndex = index;  // ← Guardar índice
            
            Debug.Log("Avatar seleccionado: " + selectedAvatar);
            
            UpdatePreview();
            HighlightButton(index);
        }
        else
        {
            Debug.LogError("Error: index fuera de rango o emoji null");
        }
    }
    
    void UpdatePreview()
    {
        if (avatarPreview != null)
        {
            avatarPreview.text = selectedAvatar;
            Debug.Log("Preview actualizado a: " + selectedAvatar);
        }
        
        // ← NUEVO: Actualizar descripción también
        if (avatarDescription != null && selectedIndex < avatarDescriptions.Length)
        {
            avatarDescription.text = avatarDescriptions[selectedIndex];
            Debug.Log("Descripción actualizada a: " + avatarDescriptions[selectedIndex]);
        }
    }
    
    void HighlightButton(int selectedIndex)
    {
        for (int i = 0; i < avatarButtons.Length; i++)
        {
            Color buttonColor = (i == selectedIndex) ? Color.green : Color.white;
            avatarButtons[i].GetComponent<Image>().color = buttonColor;
        }
    }
    
    void ConfirmSelection()
    {
        PlayerPrefs.SetString("SelectedAvatar", selectedAvatar);
        PlayerPrefs.Save();
        
        Debug.Log("Avatar confirmado: " + selectedAvatar);
        ShowOptionsMenu();
    }
    
    void ShowOptionsMenu()
    {
        mainMenuCanvas.gameObject.SetActive(true);
        createRoomCanvas.gameObject.SetActive(false);
        joinRoomCanvas.gameObject.SetActive(false);
        avatarSelectionCanvas.gameObject.SetActive(false);
    }
    
    void BackToMenu()
    {
        ShowOptionsMenu();
    }
}