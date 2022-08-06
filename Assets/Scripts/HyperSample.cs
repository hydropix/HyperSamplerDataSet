using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SpriteRenderer))]
public class HyperSample : MonoBehaviour
{
    public Vector2Int sampleSize;
    public AnimationCurve scaleRotateCurve;
    
    public GameObject UIGo;
    public InputField pathInputField;
    public Toggle rotationToggle, flipXToggle, flipYToggle;

    private SpriteRenderer spriteRenderer;
    private int outputIndex;
    
    
    private void Start()
    {
        Screen.SetResolution(sampleSize.x, sampleSize.y, false);
        QualitySettings.vSyncCount = 0;
        Screen.fullScreenMode = FullScreenMode.Windowed;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Batch()
    {
        outputIndex = 0;
        UIGo.SetActive(false);
        StartCoroutine(Execute());
        UIGo.SetActive(true);
    }
    
    private IEnumerator  Execute()
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
            
            if(flipXToggle.isOn)
            {
                spriteRenderer.flipX = true;
                for (int j = 0; j < rotationCount; j++)
                {
                    ApplyTransformationToSample(sampleTexture, 45f * j);
                    yield return new WaitForEndOfFrame();
                    ScreenToPng();
                }
            }
            
            if(flipYToggle.isOn)
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
    }
    
    private void ApplyTransformationToSample(Texture sample, float angle)
    {
        transform.localRotation = Quaternion.Euler(0, 0, angle);
        transform.localScale = Vector3.one * (GetMinLenghtFor(45) / (sample.height > sample.width ? sample.width : sample.height) * 100f);
        
    }

    private void ScreenToPng()
    {   
        var path = pathInputField.text+"/output/";
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        var fileName = path + "dataSet_" + outputIndex + ".png";
        ScreenCapture.CaptureScreenshot(fileName);
        outputIndex++;
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
        return scaleRotateCurve.Evaluate(angle%90f/90f);
    }
    
    private static Texture2D GetSampleTexture(string path)
    {
        var bytes = File.ReadAllBytes(path);
        var tex = new Texture2D(0, 0);
        tex.LoadImage(bytes);
        return tex;
    }
}