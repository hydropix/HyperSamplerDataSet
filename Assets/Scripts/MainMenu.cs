using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MainMenu : MonoBehaviour
{
    public GameObject automaticUI, manualUI, backtoMainMenu, pathFolder;
    public SpriteRenderer spriteRenderer;
    
    private void Awake()
    {
        EnterMainMenu();
    }

    public void EnterMainMenu()
    {
        spriteRenderer.sprite = null;
        gameObject.SetActive(true);
        
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
        
        gameObject.SetActive(false);
        manualUI.SetActive(false);
    }
    
    public void OnClickManualUI()
    {
        manualUI.SetActive(true);
        backtoMainMenu.SetActive(true);
        pathFolder.SetActive(true);
        
        gameObject.SetActive(false);
        automaticUI.SetActive(false);
    }
}
