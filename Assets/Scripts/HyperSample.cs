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
        scaleMin = 0.2f;

    public MainMenu mainMenu;
    public InputField pathInputField;
    public Toggle rotationToggle, flipXToggle, flipYToggle;

    private SpriteRenderer spriteRenderer;
    private int outputIndex;
    private bool mouseLeftBt_Up, rightMouseBt, shiftModifier, escapeBt_Down;
    private float mouseScrollWheel;

    private void Start()
    {
        Screen.SetResolution((int)windowSize.x, (int)windowSize.y, false);
        QualitySettings.vSyncCount = 0;
        Screen.fullScreenMode = FullScreenMode.Windowed;
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = null;
        pathInputField.text = PlayerPrefs.GetString("folderPath");
    }


    public void AutomaticBatch()
    {
        outputIndex = 0;
        mainMenu.Hide();
        StartCoroutine(ExecuteAutomaticBatch());
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
        transform.localScale = Vector3.one * (GetMinLenghtFor(45) / (sample.height > sample.width ? sample.width : sample.height) * 100f);
    }


    public void ManualBatch()
    {
        outputIndex = 0;
        mainMenu.Hide();
        var samplePaths = GetFilesPaths(pathInputField.text);

        PlayerPrefs.SetString("folderPath", pathInputField.text);
        PlayerPrefs.Save();

        StartCoroutine(ManualImageSelection(samplePaths));
    }

    private IEnumerator ManualImageSelection(List<string> samplePaths)
    {
        var rotation = 0f;
        var scale = 1f;
        transform.localRotation = Quaternion.Euler(0, 0, rotation);
        transform.localScale = new Vector3(scale, scale, scale);

        for (int i = 0; i < samplePaths.Count; i++)
        {
            Texture2D sampleTexture = GetSampleTexture(samplePaths[i]);
            spriteRenderer.sprite = Sprite.Create(sampleTexture, new Rect(0f, 0f, sampleTexture.width, sampleTexture.height), Vector2.one * 0.5f);
            yield return new WaitForEndOfFrame();

            do
            {
                yield return new WaitForEndOfFrame();
                Vector2 mousePos = ((Vector2)Input.mousePosition + new Vector2(windowSize.x * -0.5f, windowSize.y * -0.5f)) * positionDragMultiplier;
                transform.localPosition = mousePos;
                if (mouseScrollWheel != 0f)
                {
                    if (shiftModifier)
                    {
                        rotation += mouseScrollWheel * rotationMultiplier;
                        transform.localRotation = Quaternion.Euler(0, 0, rotation);
                    }
                    else
                    {
                        if (mouseScrollWheel > 0)
                            scale += scale * scaleMultiplier;
                        else
                            scale -= scale * scaleMultiplier;

                        scale = Mathf.Max(scaleMin, scale);
                        transform.localScale = new Vector3(scale, scale, scale);
                    }

                    mouseScrollWheel = 0f; // used to reset the mouse scroll wheel value
                }

                if (mouseLeftBt_Up)
                {
                    ScreenToPng();
                    Sprite sprite = spriteRenderer.sprite;
                    spriteRenderer.sprite = null;
                    yield return new WaitForSeconds(0.1f);
                    spriteRenderer.sprite = sprite;
                    mouseLeftBt_Up = false;
                }

                if (escapeBt_Down)
                {
                    mainMenu.GetComponent<MainMenu>().EnterMainMenu();
                    escapeBt_Down = false;
                }
            } while (rightMouseBt == false); // Next image

            rightMouseBt = false;
        }
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

        shiftModifier = Input.GetKey(KeyCode.LeftShift);
        mouseScrollWheel = Input.GetAxis("Mouse ScrollWheel");
    }


    private void ScreenToPng(bool safeFolderIndexCheck = true)
    {
        // Set Camera
        //Camera.main.rect = new Rect(0.25f, 0.25f, 0.5f, 0.5f);

        var path = pathInputField.text + "/output/";
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

        var fileName = path + "dataSet_" + outputIndex + ".png";
        //ScreenCapture.CaptureScreenshot(fileName);
        //Texture2D screenShot = ScreenCapture.CaptureScreenshotAsTexture();
        // rectangular area of the screen to capture
        var rect = new Rect(sampleSize.x * 0.5f, sampleSize.y * 0.5f, sampleSize.x, sampleSize.y);
        // create a new texture with the size of the screen, and make it readable
        var screenShot = new Texture2D(sampleSize.x, sampleSize.y, TextureFormat.RGB24, false);
        // read the pixels from the screen shot texture into an array
        screenShot.ReadPixels(rect, 0, 0);
        // apply the array to the screen shot texture
        screenShot.Apply();
        // encode the screen shot to a PNG
        var bytes = screenShot.EncodeToPNG();
        // save the PNG to disk
        File.WriteAllBytes(fileName, bytes);

        outputIndex++;

        // Revert Camera
        //Camera.main.rect = new Rect(0f, 0f, 1f, 1f);
    }

    private static List<string> GetFilesPaths(string folder, bool recursive = false)
    {
        var samplePaths = new List<string>();
        samplePaths.AddRange(Directory.GetFiles(folder, "*.jpg", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
        samplePaths.AddRange(Directory.GetFiles(folder, "*.png", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
        return samplePaths;
    }

    private float GetMinLenghtFor(float angle)
    {
        return scaleRotateCurve.Evaluate(angle % 90f / 90f);
    }

    private static Texture2D GetSampleTexture(string path)
    {
        var bytes = File.ReadAllBytes(path);
        var tex = new Texture2D(0, 0);
        tex.LoadImage(bytes);
        return tex;
    }
}