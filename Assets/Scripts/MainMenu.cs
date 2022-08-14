using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MainMenu : MonoBehaviour
{
    public GameObject mainMenu, automaticUI, manualUI, backtoMainMenu, pathFolder;
    public SpriteRenderer spriteRenderer;
    
    private void Awake()
    {
        EnterMainMenu();
    }

    public void EnterMainMenu()
    {
        spriteRenderer.sprite = null;
        mainMenu.SetActive(true);
        
        pathFolder.SetActive(false);
        automaticUI.SetActive(false);
        manualUI.SetActive(false);
        backtoMainMenu.SetActive(false);
    }

    public void OnClickAutomaticUI()
    {
        automaticUI.SetActive(true);
        backtoMainMenu.SetActive(true);
        pathFolder.SetActive(true);
        
        mainMenu.SetActive(false);
        manualUI.SetActive(false);
    }
    
    public void OnClickManualUI()
    {
        manualUI.SetActive(true);
        backtoMainMenu.SetActive(true);
        pathFolder.SetActive(true);
        
        mainMenu.SetActive(false);
        automaticUI.SetActive(false);
    }

    public void Hide()
    {
        spriteRenderer.sprite = null;
        mainMenu.SetActive(false);
        pathFolder.SetActive(false);
        automaticUI.SetActive(false);
        manualUI.SetActive(false);
        backtoMainMenu.SetActive(false);
    }
}
