using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Toggle = UnityEngine.UI.Toggle;

[RequireComponent(typeof(SpriteRenderer))]
public class HyperSample : MonoBehaviour
{
    public Vector2Int sampleSize;
    public Vector2 windowSize;
    public AnimationCurve scaleRotateCurve;
    public float
        positionDragMultiplier = 0.001f,
        rotationMultiplier = 100f,
        scaleMultiplier = 50f,
        scaleMin = 0.1f,
        scaleStart = 0.25f;

    public MainMenu mainMenu;
    public InputField pathInputField;
    public Text grabInfoText, pixelSizeWarningText;
    public Toggle rotationToggle, flipXToggle, flipYToggle;
    public GameObject grabFrame, pathWarning, pixelSizeWarningGO;

    private const float pixelSizeWarning = 100f / 1024 ;
    private SpriteRenderer spriteRenderer;
    private int outputIndex;
    private bool
        mouseLeftBt_Up,
        rightMouseBt,
        shiftModifier,
        escapeBt_Down,
        mouseNextBt_Up,
        mouseBackBt_Up;

    private float mouseScrollWheel;

    private void Start()
    {
        Screen.SetResolution((int)windowSize.x, (int)windowSize.y, false);
        QualitySettings.vSyncCount = 0;
        Screen.fullScreenMode = FullScreenMode.Windowed;
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = null;
        grabFrame.SetActive(false);

        // read last user settings
        pathInputField.text = PlayerPrefs.GetString("folderPath");
    }

    //  █████╗  ██╗   ██╗ ████████╗  ██████╗ 
    // ██╔══██╗ ██║   ██║ ╚══██╔══╝ ██╔═══██╗
    // ███████║ ██║   ██║    ██║    ██║   ██║
    // ██╔══██║ ██║   ██║    ██║    ██║   ██║
    // ██║  ██║ ╚██████╔╝    ██║    ╚██████╔╝
    // ╚═╝  ╚═╝  ╚═════╝     ╚═╝     ╚═════╝ 
    public void AutomaticBatch()
    {
        if (CheckPath(pathInputField.text))
        {
            outputIndex = 0;
            mainMenu.Hide();
            StartCoroutine(ExecuteAutomaticBatch());
        }
    }

    
    private IEnumerator ExecuteAutomaticBatch()
    {
        var samplePaths = GetFilesPaths(pathInputField.text);
        for (int i = 0; i < samplePaths.Count; i++)
        {
            Texture2D sampleTexture = GetSampleTexture(samplePaths[i]);
            spriteRenderer.sprite = Sprite.Create(sampleTexture, new Rect(0f, 0f, sampleTexture.width, sampleTexture.height), Vector2.one * 0.5f);

            spriteRenderer.flipX = false;
            spriteRenderer.flipY = false;
            var rotationCount = rotationToggle.isOn ? 8 : 1;
            for (int j = 0; j < rotationCount; j++)
            {
                ApplyTransformationToSample(sampleTexture, 45f * j);
                yield return new WaitForEndOfFrame();
                ScreenToPng();
            }

            if (flipXToggle.isOn)
            {
                spriteRenderer.flipX = true;
                for (int j = 0; j < rotationCount; j++)
                {
                    ApplyTransformationToSample(sampleTexture, 45f * j);
                    yield return new WaitForEndOfFrame();
                    ScreenToPng();
                }
            }

            if (flipYToggle.isOn)
            {
                spriteRenderer.flipY = true;
                for (int j = 0; j < rotationCount; j++)
                {
                    ApplyTransformationToSample(sampleTexture, 45f * j);
                    yield return new WaitForEndOfFrame();
                    ScreenToPng();
                }
            }
        }
        mainMenu.EnterMainMenu();
    }

    private void ApplyTransformationToSample(Texture sample, float angle)
    {
        transform.localRotation = Quaternion.Euler(0, 0, angle);
        transform.localScale = Vector3.one * (GetMinLenghtFor(45) / (sample.height > sample.width ? sample.width : sample.height) * 50f);
    }


    // ███╗   ███╗  █████╗  ███╗   ██╗ ██╗   ██╗  █████╗  ██╗     
    // ████╗ ████║ ██╔══██╗ ████╗  ██║ ██║   ██║ ██╔══██╗ ██║     
    // ██╔████╔██║ ███████║ ██╔██╗ ██║ ██║   ██║ ███████║ ██║     
    // ██║╚██╔╝██║ ██╔══██║ ██║╚██╗██║ ██║   ██║ ██╔══██║ ██║     
    // ██║ ╚═╝ ██║ ██║  ██║ ██║ ╚████║ ╚██████╔╝ ██║  ██║ ███████╗
    // ╚═╝     ╚═╝ ╚═╝  ╚═╝ ╚═╝  ╚═══╝  ╚═════╝  ╚═╝  ╚═╝ ╚══════╝
    public void ManualBatch()
    {
        if (CheckPath(pathInputField.text))
        {
            outputIndex = 0;
            mainMenu.Hide();

            var samplePaths = GetFilesPaths(pathInputField.text);

            PlayerPrefs.SetString("folderPath", pathInputField.text);
            PlayerPrefs.Save();

            StartCoroutine(ManualImageSelection(samplePaths));
        }
    }

    private IEnumerator ManualImageSelection(List<string> samplePaths)
    {
        grabFrame.SetActive(true);


        var rotation = 0f;
        var scale = scaleStart;
        transform.localRotation = Quaternion.Euler(0, 0, rotation);
        transform.localScale = new Vector3(scale, scale, scale);

        for (int i = 0; i < samplePaths.Count; i++)
        {
            // Update info text
            grabInfoText.text = $"{i + 1}/{samplePaths.Count}";
            grabInfoText.text += $"\nFilename: {Path.GetFileName(samplePaths[i])}";

            // Load sample image
            Texture2D sampleImage = GetSampleTexture(samplePaths[i]);
            grabInfoText.text += $"\nSize: {sampleImage.width}x{sampleImage.height}";
            spriteRenderer.sprite = Sprite.Create(sampleImage, new Rect(0f, 0f, sampleImage.width, sampleImage.height), Vector2.one * 0.5f);
            
            // Start zoom to pixel size 1:1 screen
            scale = pixelSizeWarning;
            transform.localScale = new Vector3(scale, scale, scale);
            
            yield return new WaitForEndOfFrame();
            ResetAllButtons();

            do
            {
                yield return new WaitForEndOfFrame();
                
                scale = Mathf.Min(pixelSizeWarning * 2f, scale);
                
                if (pixelSizeWarning *2f < scale)
                {
                    ShowPixelSizeWarningMessage("Pixel Size is too large for 256x256");
                }
                else if (pixelSizeWarning < scale )
                {
                    ShowPixelSizeWarningMessage("Pixel Size is too large for 512x512, but still OK for 256x256");
                }
                else
                {
                    HidePixelSizeWarningMessage();
                }
                
                pixelSizeWarningGO.SetActive(pixelSizeWarning < transform.localScale.x);
   
                // Move image
                Vector2 mousePos = ((Vector2)Input.mousePosition + new Vector2(windowSize.x * -0.5f, windowSize.y * -0.5f)) * positionDragMultiplier;
                transform.localPosition = mousePos;

                if (mouseScrollWheel != 0f)
                {
                    // Rotate image
                    if (shiftModifier)
                    {
                        rotation += mouseScrollWheel * rotationMultiplier;
                        transform.localRotation = Quaternion.Euler(0, 0, rotation);
                    }
                    else
                    {
                        // Zoom image
                        if (mouseScrollWheel > 0)
                            scale += scale * scaleMultiplier;
                        else
                            scale -= scale * scaleMultiplier;

                        scale = Mathf.Max(scaleMin, scale);
                        transform.localScale = new Vector3(scale, scale, scale);
                    }

                    mouseScrollWheel = 0f; // used
                }

                // Grab and Save
                if (mouseLeftBt_Up)
                {
                    ScreenToPng();
                    Sprite sprite = spriteRenderer.sprite;
                    spriteRenderer.sprite = null;
                    yield return new WaitForSeconds(0.1f);
                    spriteRenderer.sprite = sprite;
                    mouseLeftBt_Up = false;
                }

                // Exit, back to main menu
                if (escapeBt_Down)
                {
                    grabFrame.SetActive(false);
                    mainMenu.EnterMainMenu();
                    escapeBt_Down = false;
                    yield break;
                }

                // Rewind index
                if (mouseBackBt_Up)
                {
                    i = i > 1 ? i - 2 : -1;
                }
            } while (rightMouseBt == false && mouseNextBt_Up == false && mouseBackBt_Up == false); // Next image

            rightMouseBt = mouseNextBt_Up = mouseBackBt_Up = false;
        }

        // Hide grab frame
        grabFrame.SetActive(false);
    }


    private void Update()
    {
        // Get inputs and register them
        if (Input.GetMouseButtonUp(0))
            mouseLeftBt_Up = true;

        if (Input.GetMouseButtonUp(1))
            rightMouseBt = true;

        if (Input.GetKey(KeyCode.Escape))
            escapeBt_Down = true;

        // optional mouse buttons
        if (Input.GetMouseButtonUp(3)) // back 
            mouseBackBt_Up = true;

        if (Input.GetMouseButtonUp(4)) // next 
            mouseNextBt_Up = true;

        shiftModifier = Input.GetKey(KeyCode.LeftShift);
        mouseScrollWheel = Input.GetAxis("Mouse ScrollWheel");
    }

    private void ResetAllButtons()
    {
        mouseLeftBt_Up = false;
        rightMouseBt = false;
        escapeBt_Down = false;
        mouseBackBt_Up = false;
        mouseNextBt_Up = false;
    }

    private void ScreenToPng(bool safeFolderIndexCheck = true)
    {
        var path = pathInputField.text + "/output/";

        // Create output folder if it doesn't exist
        // Prevents overwriting of old .png files
        if (safeFolderIndexCheck)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            else
            {
                // Get max index of existing files
                var existingFiles = Directory.GetFiles(path, "*.png", SearchOption.TopDirectoryOnly);
                outputIndex = 0;
                foreach (var f in existingFiles)
                {
                    var index = int.Parse(f.Split('/').Last().Split('_', '.')[1]);
                    if (outputIndex < index)
                    {
                        outputIndex = index;
                    }
                }

                outputIndex = existingFiles.Length > 0 ? ++outputIndex : 0;
            }
        }

        // screen capture
        var fileName = path + "dataSet_" + outputIndex + ".png";
        var rect = new Rect(sampleSize.x * 0.5f, sampleSize.y * 0.5f, sampleSize.x, sampleSize.y);
        var screenShot = new Texture2D(sampleSize.x, sampleSize.y, TextureFormat.RGB24, false);
        screenShot.ReadPixels(rect, 0, 0);
        screenShot.Apply();

        // encode the screen shot to a PNG
        var bytes = screenShot.EncodeToPNG();

        // save the PNG to disk
        File.WriteAllBytes(fileName, bytes);

        outputIndex++;
    }

    /// <summary>
    /// Get the list of png and jpg path files in a folder
    /// </summary>
    private static List<string> GetFilesPaths(string folder, bool recursive = false)
    {
        var samplePaths = new List<string>();
        samplePaths.AddRange(Directory.GetFiles(folder, "*.jpg", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
        samplePaths.AddRange(Directory.GetFiles(folder, "*.png", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
        return samplePaths;
    }

    private bool CheckPath(string path)
    {
        if (!Directory.Exists(path))
        {
            StartCoroutine(ShowPathWarning(pathWarning));
            return false;
        }
        return true;
    }

    private IEnumerator ShowPathWarning(GameObject warningGO)
    {
        warningGO.SetActive(true);
        yield return new WaitForSeconds(1f);
        warningGO.SetActive(false);
    }

    private float GetMinLenghtFor(float angle)
    {
        return scaleRotateCurve.Evaluate(angle % 90f / 90f);
    }

    private void ShowPixelSizeWarningMessage(string message)
    {
        pixelSizeWarningText.text = message;
        pixelSizeWarningGO.gameObject.SetActive(true);
    }
    
    private void HidePixelSizeWarningMessage()
    {
        pixelSizeWarningGO.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Get the texture of a sample image
    /// </summary>
    /// <param name="path">file path</param>
    /// <returns>Texture2D</returns>
    private static Texture2D GetSampleTexture(string path)
    {
        var bytes = File.ReadAllBytes(path);
        var tex = new Texture2D(0, 0);
        tex.LoadImage(bytes);
        return tex;
    }
}