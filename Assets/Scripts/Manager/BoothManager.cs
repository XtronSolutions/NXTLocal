using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;
using System;
using Vuplex.WebView;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class ImageData
{
    public string Name;
    public string imageURL;
    public string boothLink;
}
public class BoothManager : MonoBehaviour
{
    public static BoothManager Instance;
    [Tooltip("List of image renderders on booth inside lobby")]
    [SerializeField] private Renderer[] imageRenderer;
    [Tooltip("List of image data to be loaded on booth inside lobby")]
    [SerializeField] private ImageData[] imageData;
    [Tooltip("Reference to canvas object, for parenting webview")]
    [SerializeField] private GameObject canvasParent;
    [Tooltip("Reference to back button to destroy webview and get back to game")]
    [SerializeField] private Button backButtonWebView;
    [Tooltip("Reference to interact msg to show to user")]
    [SerializeField] private GameObject interactMsg;
    [Tooltip("Reference to interact button to enable webview")]
    [SerializeField] private Button interactButton;

    private CanvasWebViewPrefab canvasWebViewPrefab=null;
    private BoothInteractor boothInteractor;
    private string selectedBoothLink;
    bool StartInteract = false;

    #region Start/MISC
    private void OnEnable()
    {
        backButtonWebView.onClick.AddListener(DestroyWebView);
        interactButton.onClick.AddListener(InteractWebView);
    }
    private void OnDisable()
    {
        backButtonWebView.onClick.RemoveListener(DestroyWebView);
        interactButton.onClick.RemoveListener(InteractWebView);
    }
    private void Start()
    {
        Instance = this;
        DownloadImages();
    }
    #endregion

    #region WebView
    private void Update()
    {
        if(StartInteract && !RPMAvatarLoader.Instance.GetIsMobile())
        {
            if (Input.GetKeyDown(KeyCode.I))
                InteractWebView();
        }
    }
    public bool GetInteractActiveStatus()
    {
        return interactButton.gameObject.activeInHierarchy;
    }
    public void ToggleInteractMessage(bool _state)
    {
        interactMsg.SetActive(_state);

        if (RPMAvatarLoader.Instance && _state)
            interactMsg.GetComponentInChildren<TextMeshProUGUI>().text = "Come closer to interact";
    }
    public void ToggleInteract(bool _state)
    {
        StartInteract = _state;
        interactButton.gameObject.SetActive(_state);

        if (RPMAvatarLoader.Instance && _state)
        {
            if(RPMAvatarLoader.Instance.GetIsMobile())
                interactButton.GetComponentInChildren<TextMeshProUGUI>().text = "Press to Interact";
            else
                interactButton.GetComponentInChildren<TextMeshProUGUI>().text = "Press 'I' to interact";
        }
    }
    public void InteractWebView()
    {
        LoadWebView(selectedBoothLink);
    }
    public void SetSelectedBoothLink(string _link)
    {
        selectedBoothLink = _link;
    }
    public async void LoadWebView(string _url)
    {
        if(canvasWebViewPrefab!=null)
            DestroyWebView();

        ToggleInteract(false);
        Constants.InteractionRunning = true;
        canvasWebViewPrefab = CanvasWebViewPrefab.Instantiate();
        canvasWebViewPrefab.transform.parent = canvasParent.transform;
        var rectTransform = canvasWebViewPrefab.transform as RectTransform;
        rectTransform.anchoredPosition3D = Vector3.zero;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        canvasWebViewPrefab.transform.localScale = new Vector3(0.85f, 0.85f, 0.85f);
        await canvasWebViewPrefab.WaitUntilInitialized();
        canvasWebViewPrefab.WebView.LoadUrl(_url);
        canvasWebViewPrefab.WebView.SetFocused(true);
        backButtonWebView.gameObject.SetActive(true);

        if (RPMAvatarLoader.Instance)
           RPMAvatarLoader.Instance.ToggleInput(false);
    }
    public void DestroyWebView()
    {
        Constants.InteractionRunning = false;
        ToggleInteract(true);
        if (canvasWebViewPrefab != null)
        {
            Destroy(canvasWebViewPrefab.gameObject);
            canvasWebViewPrefab = null;
        }

        backButtonWebView.gameObject.SetActive(false);

        if (RPMAvatarLoader.Instance)
            RPMAvatarLoader.Instance.ToggleInput(true);
    }
    #endregion

    #region Images
    public IEnumerator LoadImageFromURL(string url,string _boothURL,int index)
    {
        if (index < imageRenderer.Length)
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                imageRenderer[index].material.SetTexture("_EmissionMap", texture);

                boothInteractor = imageRenderer[index].GetComponentInChildren<BoothInteractor>(true);
                if (boothInteractor != null && !string.IsNullOrEmpty(_boothURL))
                    boothInteractor.SetBoothData(_boothURL,true);
            }
        }else
        {
            Debug.Log("ImageURL index is out of range for material array");
        }
    }
    public void DownloadImages()
    {
        for (int i = 0; i < imageData.Length; i++)
        {
            StartCoroutine(LoadImageFromURL(imageData[i].imageURL, imageData[i].boothLink, i));
        }
    }
    #endregion
}
