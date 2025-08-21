using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PhotoAlbum3D : MonoBehaviour
{
    [Header("Album Components")]
    public GameObject albumRoot;
    public MeshRenderer photoQuadRenderer;
    //public TextMeshPro descriptionText3D;
    public TextMeshProUGUI descriptionTextUI;


    [Header("Input Actions")]
    public InputActionProperty toggleAlbumAction;
    public InputActionProperty toggleTutorialAction;
    public InputActionProperty joystickAction;
    public InputActionProperty deletePhotoAction;

    [Header("Tutorial Settings")]
    public Texture2D[] tutorialImages;
    public string tutorialFolderPath = "tutorial";

    public enum AlbumMode
    {
        Photos,    // Photo mode
        Tutorial   // Tutorial mode
    }

    private readonly List<string> photoPaths = new();
    private readonly List<Texture2D> loadedTutorialImages = new();
    private int currentIndex = 0;
    private bool isAlbumOpen = false;
    private AlbumMode currentMode = AlbumMode.Tutorial;
    private float lastInputTime = -999f;
    private readonly float inputCooldown = 0.5f;
    private Texture2D currentTexture;


    void Start()
    {
        albumRoot.SetActive(false);
        isAlbumOpen = false;
        LoadTutorialImagesFromFolder();
        RefreshAlbumList();
        ToggleAlbum(AlbumMode.Tutorial);
    }

    void OnEnable()
    {
        toggleAlbumAction.action.performed += ctx => ToggleAlbum(AlbumMode.Photos);
        toggleTutorialAction.action.performed += ctx => ToggleAlbum(AlbumMode.Tutorial);
        deletePhotoAction.action.performed += ctx => DeleteCurrentPhoto();

        toggleAlbumAction.action.Enable();
        toggleTutorialAction.action.Enable();
        joystickAction.action.Enable();
        deletePhotoAction.action.Enable();
    }

    void OnDisable()
    {
        toggleAlbumAction.action.performed -= ctx => ToggleAlbum(AlbumMode.Photos);
        toggleTutorialAction.action.performed -= ctx => ToggleAlbum(AlbumMode.Tutorial);
        deletePhotoAction.action.performed -= ctx => DeleteCurrentPhoto();

        toggleAlbumAction.action.Disable();
        toggleTutorialAction.action.Disable();
        joystickAction.action.Disable();
        deletePhotoAction.action.Disable();
    }

    void ToggleAlbum(AlbumMode mode)
    {
        var cameraCtrl = FindFirstObjectByType<FloatCameraController>();
        if (cameraCtrl != null && cameraCtrl.IsCameraActive())
        {
            var msgMgr = FindFirstObjectByType<FloatingMessageManager>();
            if (msgMgr) msgMgr.ShowMessage("Shutdown camera to turn on album");
            return;
        }
        if (isAlbumOpen && currentMode == mode)
        {
            isAlbumOpen = false;
            albumRoot.SetActive(false);
        }
        else if (isAlbumOpen && currentMode != mode)
        {
            currentMode = mode;
            currentIndex = 0;
            RefreshCurrentMode();
        }
        else
        {
            isAlbumOpen = true;
            currentMode = mode;
            currentIndex = 0;
            albumRoot.SetActive(true);
            RefreshCurrentMode();
        }
        if (cameraCtrl != null)
            cameraCtrl.enabled = !isAlbumOpen;
    }

    //void ToggleAlbum()
    //{

    //    var cameraCtrl = Object.FindFirstObjectByType<FloatCameraController>();
    //    if (cameraCtrl != null && cameraCtrl.IsCameraActive())
    //    {
    //        Object.FindFirstObjectByType<FloatingMessageManager>()?.ShowMessage("Shutdown camera to turn on album");
    //        return;
    //    }

    //    isAlbumOpen = !isAlbumOpen;
    //    albumRoot.SetActive(isAlbumOpen);
    //    RefreshAlbumList();

    //    if (cameraCtrl != null)
    //        cameraCtrl.enabled = !isAlbumOpen;
    //}


    void Update()
    {

        if (!isAlbumOpen || photoPaths.Count == 0) return;

        int totalCount = GetCurrentModeCount();
        if (totalCount == 0) return;

        Vector2 input = joystickAction.action.ReadValue<Vector2>();
        if (Time.time - lastInputTime < inputCooldown) return;

        if (input.x >= 0.5f)
        {
            ShowCurrentModeContent(currentIndex + 1);
            lastInputTime = Time.time;
        }
        else if (input.x <= -0.5f)
        {
            ShowCurrentModeContent(currentIndex - 1);
            lastInputTime = Time.time;
        }


        //if (descriptionText3D != null && Camera.main != null)
        //{
        //    Quaternion lookRotation = Quaternion.LookRotation(descriptionText3D.transform.position - Camera.main.transform.position);
        //    descriptionText3D.transform.rotation = lookRotation;
        //}
    }

    int GetCurrentModeCount()
    {
        return currentMode switch
        {
            AlbumMode.Photos => photoPaths.Count,
            AlbumMode.Tutorial => tutorialImages != null ? tutorialImages.Length : 0,
            _ => 0,
        };
    }

    int GetTutorialImageCount()
    {
        if (loadedTutorialImages.Count > 0)
            return loadedTutorialImages.Count;
        return tutorialImages != null ? tutorialImages.Length : 0;
    }


    void ShowCurrentModeContent(int index)
    {
        int totalCount = GetCurrentModeCount();
        if (totalCount == 0) return;

        index = (index + totalCount) % totalCount;
        currentIndex = index;

        switch (currentMode)
        {
            case AlbumMode.Photos:
                ShowPhoto(index);
                break;
            case AlbumMode.Tutorial:
                ShowTutorial(index);
                break;
        }
    }

    void ShowPhoto(int index)
    {
        if (photoPaths.Count == 0) return;

        string path = photoPaths[index];

        if (!File.Exists(path)) return;

        byte[] data = File.ReadAllBytes(path);

        if (currentTexture != null)
            Destroy(currentTexture);

        currentTexture = new Texture2D(2, 2);
        currentTexture.LoadImage(data);
        photoQuadRenderer.material.mainTexture = currentTexture;

        string txtPath = Path.ChangeExtension(path, ".txt");
        //descriptionText3D.text = File.Exists(txtPath) ? File.ReadAllText(txtPath) : "(No description)";
        string description = File.Exists(txtPath) ? File.ReadAllText(txtPath) : "(No description)";
        descriptionTextUI.text = $"Photos ({index + 1}/{photoPaths.Count})\n{description}";

        currentIndex = index;
    }

    void ShowTutorial(int index)
    {
        Texture2D imageToShow = null;
        int totalCount = GetTutorialImageCount();

        if (totalCount == 0) return;

        if (loadedTutorialImages.Count > 0 && index < loadedTutorialImages.Count)
        {
            imageToShow = loadedTutorialImages[index];
        }

        else if (tutorialImages != null && index < tutorialImages.Length)
        {
            imageToShow = tutorialImages[index];
        }

        if (imageToShow != null)
        {
            photoQuadRenderer.material.mainTexture = imageToShow;
            descriptionTextUI.text = $"({index + 1}/{totalCount})";
        }
    }

    void RefreshCurrentMode()
    {
        switch (currentMode)
        {
            case AlbumMode.Photos:
                RefreshAlbumList();
                break;
            case AlbumMode.Tutorial:
                RefreshTutorialList();
                break;
        }
    }

    public void RefreshAlbumList()
    {
        photoPaths.Clear();
        string[] files = Directory.GetFiles(Application.persistentDataPath, "photo_*.png");
        photoPaths.AddRange(files);
        photoPaths.Sort();

        if (currentMode == AlbumMode.Photos)
        {
            if (photoPaths.Count > 0)
                ShowPhoto(0);
            else
            {
                photoQuadRenderer.material.mainTexture = null;
                descriptionTextUI.text = "Photos\n(No photos)";
                //descriptionText3D.text = "(No photos)";
            }
        }
    }

    void RefreshTutorialList()
    {
        int totalCount = GetTutorialImageCount();
        if (totalCount > 0) ShowTutorial(0);
        else
        {
            photoQuadRenderer.material.mainTexture = null;
            descriptionTextUI.text = "(No tutorial images)";
        }
    }

    public void DeleteCurrentPhoto()
    {
        if (currentMode != AlbumMode.Photos || photoPaths.Count == 0) return;

        string photoPath = photoPaths[currentIndex];
        string txtPath = Path.ChangeExtension(photoPath, ".txt");

        if (File.Exists(photoPath)) File.Delete(photoPath);
        if (File.Exists(txtPath)) File.Delete(txtPath);

        Debug.Log("Deleted photo: " + Path.GetFileName(photoPath));
        RefreshAlbumList();
    }

    public void AddNewPhoto(string path)
    {
        if (!photoPaths.Contains(path))
        {
            photoPaths.Add(path);
            photoPaths.Sort();
        }
        if (currentMode == AlbumMode.Photos) ShowPhoto(photoPaths.IndexOf(path));
    }
    public bool IsAlbumCurrentlyOpen()
    {
        return isAlbumOpen;
    }

    public AlbumMode GetCurrentMode()
    {
        return currentMode;
    }

    public void SetTutorialContent(Texture2D[] images)
    {
        tutorialImages = images;

        if (currentMode == AlbumMode.Tutorial && isAlbumOpen)
        {
            RefreshTutorialList();
        }
    }

    void LoadTutorialImagesFromFolder()
    {
        loadedTutorialImages.Clear();

        Texture2D[] textures = Resources.LoadAll<Texture2D>(tutorialFolderPath);

        if (textures.Length > 0)
        {
            loadedTutorialImages.AddRange(textures);
            loadedTutorialImages.Sort((a, b) => a.name.CompareTo(b.name));
            Debug.Log($"Loaded {loadedTutorialImages.Count} tutorial images from Resources/{tutorialFolderPath}");
        }
        else
        {
            Debug.Log($"No tutorial images found in Resources/{tutorialFolderPath}");
        }
    }

    public void ReloadTutorialImages()
    {
        LoadTutorialImagesFromFolder();
        if (currentMode == AlbumMode.Tutorial && isAlbumOpen)
        {
            RefreshTutorialList();
        }
    }
}
