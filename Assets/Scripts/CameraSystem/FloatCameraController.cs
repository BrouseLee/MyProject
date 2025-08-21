using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using static SharkBehaviour;

public class FloatCameraController : MonoBehaviour
{
    private Boolean cameraOn = false;

    [Header("Camera Components")]
    public GameObject floatCameraRig;
    public Camera virtualCamera;
    public RenderTexture renderTexture;

    [Header("Zoom Settings")]
    public float zoomspeed;
    public float minFOV;
    public float maxFOV;

    [Header("Input Actions")]
    public InputActionProperty toggleCameraAction;
    public InputActionProperty takePhotoAction;
    public InputActionProperty zoomAction;

    void Start()
    {
        cameraOn = false;
        floatCameraRig.SetActive(false);
    }


    void OnToggleCamera(InputAction.CallbackContext ctx)
    {
        var album = UnityEngine.Object.FindFirstObjectByType<PhotoAlbum3D>();
        if (album != null && album.IsAlbumCurrentlyOpen())
        {
            UnityEngine.Object.FindFirstObjectByType<FloatingMessageManager>()?.ShowMessage("Shutdown album to turn on camera");
            return;
        }


        cameraOn = !cameraOn;
        floatCameraRig.SetActive(cameraOn);
    }

    void OnTakePhoto(InputAction.CallbackContext ctx)
    {
        if (!cameraOn) return;

        // Generate file path
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string path = Path.Combine(Application.persistentDataPath, $"photo_{timestamp}.png");
        string txtPath = Path.ChangeExtension(path, ".txt");

        // 1 Make photo RT an active RT
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = renderTexture;

        //  2 Read pixels
        Texture2D tex = new Texture2D(renderTexture.width,
                                       renderTexture.height,
                                       TextureFormat.RGBA32,
                                       false,                 // no mipmap
                                       false);

        tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        tex.Apply(false, false);

        // 3 Write PNG 
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Debug.Log($"Photo saved to: {path}");

        // 4 Restore RT & Free Up Memory
        RenderTexture.active = prev;
        Destroy(tex);

        // 5 Detect photogenic objects within the viewing cone
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(virtualCamera);
        var detectables = FindObjectsByType<DetectableObject>(FindObjectsSortMode.None);

        int sardineCount = 0;
        int sharkCount = 0;
        SharkActivity sharkState = SharkActivity.Swim;

        List<string> lines = new();

        foreach (var d in detectables)
        {
            if (!d.Renderer || !GeometryUtility.TestPlanesAABB(planes, d.Renderer.bounds))
                continue;

            if (d.objectTypes.Contains(ObjectType.Sardine))
            {
                sardineCount++;
            }
            else if (d.objectTypes.Contains(ObjectType.ThresherShark))
            {
                sharkCount++;

                // 只取第一条鲨鱼的状态
                if (sharkCount == 1 && d.TryGetComponent<SharkBehaviour>(out var sb))
                {
                    sharkState = sb.CurrentActivity;
                }
            }
            else
            {
                lines.Add($"- {d.name} ({string.Join(", ", d.objectTypes)})");
            }
        }

        // -- 规则整合 ---
        // 1. 沙丁鱼单独处理（没有鲨鱼时）
        if (sardineCount > 0 && sharkCount == 0)
        {
            lines.Add(sardineCount > 1 ? "- SardineFlock" : "- Sardine");
        }

        // 2. 鲨鱼处理
        if (sharkCount > 0)
        {
            string sharkDisplay = sharkCount > 1 ? "ThresherSharks" : "ThresherShark";

            if (sardineCount > 0)
            {
                string sardineName = sardineCount > 1 ? "SardineFlock" : "Sardine";

                switch (sharkState)
                {
                    case SharkActivity.HuntSardine:
                        lines.Add($"- {sharkDisplay} hunting {sardineName}");
                        break;
                    case SharkActivity.Play:
                        lines.Add($"- {sharkDisplay} playing with {sardineName}");
                        break;
                    default:
                        lines.Add($"- {sharkDisplay} with {sardineName}");
                        break;
                }
            }
            else
            {
                lines.Add($"- {sharkDisplay} ({sharkState})");
            }
        }

        // 写文件
        File.WriteAllText(txtPath, lines.Count > 0 ? string.Join("\n", lines)
                                                   : "(No detectable objects)");
        //List<string> lines = new();
        //foreach (var d in detectables)
        //{
        //    if (d.Renderer && GeometryUtility.TestPlanesAABB(planes, d.Renderer.bounds))
        //    {
        //        string types = string.Join(", ", d.objectTypes);
        //        lines.Add($"- {d.name} (Types: {types})");
        //    }
        //}
        //File.WriteAllText(txtPath, lines.Count > 0 ? string.Join("\n", lines)
        //                                           : "(No detectable objects)");
        //Debug.Log($"Description saved to: {txtPath}");
    }

    //void OnTakePhoto(InputAction.CallbackContext ctx)
    //{
    //    if (!cameraOn) return;

    //    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
    //    string path = Path.Combine(Application.persistentDataPath, $"photo_{timestamp}.png");
    //    string txtPath = Path.ChangeExtension(path, ".txt");


    //    RenderTexture currentRT = RenderTexture.active;
    //    RenderTexture.active = renderTexture;

    //    Texture2D image = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
    //    image.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
    //    image.Apply();

    //    byte[] bytes = image.EncodeToPNG();
    //    File.WriteAllBytes(path, bytes);
    //    RenderTexture.active = currentRT;
    //    Destroy(image);

    //    Debug.Log("Photo saved to: " + path);


    //    Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(virtualCamera);
    //    var allDetectables = FindObjectsByType<DetectableObject>(FindObjectsSortMode.None);
    //    List<string> detectedInfo = new List<string>();

    //    foreach (var d in allDetectables)
    //    {
    //        Renderer r = d.Renderer;
    //        if (r != null && GeometryUtility.TestPlanesAABB(frustumPlanes, r.bounds))
    //        {
    //            string types = string.Join(", ", d.objectTypes);
    //            detectedInfo.Add($"- {d.name}（Types: {types}）");
    //        }
    //    }

    //    if (detectedInfo.Count > 0)
    //        File.WriteAllText(txtPath, string.Join("\n", detectedInfo));
    //    else
    //        File.WriteAllText(txtPath, "(No detectable objects)");

    //    Debug.Log(" Description saved to: " + txtPath);
    //}

    void OnEnable()
    {
        toggleCameraAction.action.performed += OnToggleCamera;
        takePhotoAction.action.performed += OnTakePhoto;
        toggleCameraAction.action.Enable();
        takePhotoAction.action.Enable();
        zoomAction.action.Enable();
    }

    private void OnDisable()
    {
        toggleCameraAction.action.performed -= OnToggleCamera;
        takePhotoAction.action.performed -= OnTakePhoto;
        toggleCameraAction.action.Disable();
        takePhotoAction.action.Disable();
        zoomAction.action.Disable();
    }

    private void Update()
    {
        if (!cameraOn) return;

        float zoomInput = zoomAction.action.ReadValue<float>();
        if (Mathf.Abs(zoomInput) > 0.01f)
        {
            virtualCamera.fieldOfView = Mathf.Clamp(
                virtualCamera.fieldOfView - zoomInput * zoomspeed * Time.deltaTime,
                minFOV, maxFOV
            );
        }
    }

    public bool IsCameraActive()
    {
        return cameraOn && floatCameraRig.activeSelf;
    }

}


public enum ObjectType
{
    Sardine,
    ThresherShark,
    Manta,
    WhaleShark,
    SeaTurtle,
    Triggerfish
}
