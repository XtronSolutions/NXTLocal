using Cinemachine;
using ReadyPlayerMe;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class RPMAvatarLoader : MonoBehaviour
{
    #region DataMembers
    public static RPMAvatarLoader Instance;
    [Tooltip("List of spawn points for avatar")]
    [SerializeField] private Transform[] avatarSpawnLocation;
    [Tooltip("Links of RPM avatar (.glb) models")]
    [SerializeField] private string[] avatarURL;
    [Tooltip("Reference of Animator controller for the RPM avatar")]
    [SerializeField] private RuntimeAnimatorController animatorController;
    [Tooltip("Reference of Audio clips RPM avatar controller")]
    [SerializeField] private AudioClip[] audioClips;
    [Tooltip("Reference of InputActionAsset RPM avatar controller")]
    [SerializeField] private InputActionAsset inputActionAsset;
    [Tooltip("Reference of UICanvasControllerInput RPM avatar controller")]
    [SerializeField] private UICanvasControllerInput uICanvasControllerInput;
    [Tooltip("Reference of Mobile Controller RPM avatar controller")]
    [SerializeField] private GameObject MobileController;
    public string[] AvatarURL
    {
        get
        {
            return this.avatarURL;
        }
        set
        {
            this.avatarURL = value;
        }
    }
    private string selectedAvatar;
    private GameObject avatar;
    private const string CAMERA_TARGET_OBJECT_NAME = "CameraTarget";
    private List<GameObject> loadedAvatars = new List<GameObject>();
    private StarterAssetsInputs starterAssetsInputs;

    [DllImport("__Internal")]
    private static extern bool IsMobile();
    private bool isMobile = false;
    #endregion

    #region Member Functions

    public List<GameObject> GetLoadedAvatars()
    {
        return loadedAvatars;
    }
    private void Start()
    {
        Instance = this;
        loadedAvatars.Clear();
        LoadRPMAvatar();
        isMobile = CheckMobile();
    }

    public bool CheckMobile()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
             return IsMobile();
#endif
        return false;
    }

    public bool GetIsMobile()
    {
        return isMobile;
    }
    public void LoadRPMAvatar()
    {
        var avatarLoader = new AvatarLoader();
        avatarLoader.OnCompleted += OnAvatarLoadCompleted;
        selectedAvatar = AvatarURL[Random.Range(0, AvatarURL.Length)];
        avatarLoader.LoadAvatar(selectedAvatar);
    }
    public void UnloadAvatar()
    {
        if (avatar != null) Destroy(avatar);
    }
    private void OnAvatarLoadCompleted(object sender, CompletionEventArgs args)
    {
        avatar = args.Avatar;
        Debug.Log("Avatar Loaded!");
        AvatarAnimatorHelper.SetupAnimator(args.Metadata.BodyType, avatar);
        avatar.AddComponent<EyeAnimationHandler>();
        avatar.AddComponent<VoiceHandler>();

        avatar.transform.position = avatarSpawnLocation[Random.Range(0, avatarSpawnLocation.Length)].position;
        SetupCharacter(avatar.gameObject);
        loadedAvatars.Add(avatar);
    }

    PlayerInput playerInput;
    private void SetupCharacter(GameObject _avatar)
    {
        // Cache selected object to add the components
        GameObject character = _avatar;
        MobileController.SetActive(false);

        character.tag = "Player";

        // Create camera follow target
        GameObject cameraTarget = new GameObject(CAMERA_TARGET_OBJECT_NAME);
        cameraTarget.transform.parent = character.transform;
        cameraTarget.transform.localPosition = new Vector3(0, 1.5f, 0);
        cameraTarget.tag = "CinemachineTarget";

        // Set the animator controller and disable root motion
        Animator animator = character.GetComponent<Animator>();
        animator.runtimeAnimatorController = animatorController;
        animator.applyRootMotion = false;

        // Add tp controller and set values
        ThirdPersonController tpsController = character.AddComponent<ThirdPersonController>();
        tpsController.GroundedOffset = 0.1f;
        tpsController.GroundLayers = 1;
        tpsController.JumpTimeout = 0.5f;
        tpsController.CinemachineCameraTarget = cameraTarget;
        tpsController.LandingAudioClip = audioClips[0];
        tpsController.FootstepAudioClips = new AudioClip[]
        {
           audioClips[1],
           audioClips[2]
        };

        // Add character controller and set size
        CharacterController characterController = character.GetComponent<CharacterController>();
        characterController.slopeLimit = 45f;
        characterController.stepOffset = 0.25f;
        characterController.skinWidth = 0.02f;
        characterController.minMoveDistance = 0;
        characterController.center = new Vector3(0, 0.93f, 0);
        characterController.radius = 0.28f;
        characterController.height = 1.8f;

        // Add components with default values
        character.AddComponent<BasicRigidBodyPush>();
        StarterAssetsInputs m_StarterAssetsInputs=character.AddComponent<StarterAssetsInputs>();
        m_StarterAssetsInputs.cursorLocked = false;

        // Add player input and set actions asset
        playerInput = character.GetComponent<PlayerInput>();
        playerInput.actions = inputActionAsset;
        playerInput.actions.Enable();

        uICanvasControllerInput.starterAssetsInputs = character.GetComponent<StarterAssetsInputs>();

        var camera = Object.FindObjectOfType<CinemachineVirtualCamera>();
        if (camera)
        {
            camera.Follow = cameraTarget.transform;
            camera.LookAt = cameraTarget.transform;
        }

        if (isMobile)
        {
            playerInput.enabled = false;
            MobileController.SetActive(true);
        }

        m_StarterAssetsInputs.cursorLocked = true;
        m_StarterAssetsInputs.SetCursorState(m_StarterAssetsInputs.cursorLocked);
    }

   public void ToggleInput(bool _state)
    {
        for (int i = 0; i < loadedAvatars.Count; i++)
        {
            if (!isMobile)
            {
                loadedAvatars[i].GetComponent<PlayerInput>().enabled = _state;
                starterAssetsInputs = loadedAvatars[i].GetComponent<StarterAssetsInputs>();
                starterAssetsInputs.cursorLocked = _state;
                starterAssetsInputs.SetCursorState(starterAssetsInputs.cursorLocked);

                starterAssetsInputs.move = Vector2.zero;
                starterAssetsInputs.look = Vector2.zero;
                starterAssetsInputs.jump = false;
                starterAssetsInputs.sprint = false;
            }
        }
    }

    #endregion
}
