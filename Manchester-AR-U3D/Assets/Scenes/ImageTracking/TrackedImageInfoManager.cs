using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using static ARWorldMapController;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;
using MVXUnity;

[RequireComponent(typeof(ARTrackedImageManager))]
public class TrackedImageInfoManager : MonoBehaviour
{

    ARTrackedImageManager m_TrackedImageManager;
    ARPlaneManager m_ARPlaneManager;
    ARRaycastManager m_RaycastManager;
    ARPointCloudManager m_ARPointCloudManager;
    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
    List<ARAnchor> m_Anchors = new List<ARAnchor>();
    private ARTrackedImage m_trackedImage;
    private bool isLocationLocked = false;

    [SerializeField]
    ARWorldMapController aRWorldMapController;

    [SerializeField]
    ARSession m_arSession;

    //MVX
    [SerializeField]
    private Material[] m_materialTemplates = null;
    private GameObject m_mvxObj = null;
    private string m_filename = "";

    public string fname
    {
        get { return m_filename; }
        set { m_filename = value; }
    }


    void Awake()
    {
        m_TrackedImageManager = GetComponent<ARTrackedImageManager>();
        m_ARPlaneManager = GetComponent<ARPlaneManager>();
        m_ARPointCloudManager = GetComponent<ARPointCloudManager>();
        m_RaycastManager = GetComponent<ARRaycastManager>();


        //自动加载
        string worldpath = Path.Combine(Application.persistentDataPath, "my_session.worldmap");
        if (File.Exists(worldpath))
        {
            m_TrackedImageManager.enabled = false;
            m_ARPlaneManager.enabled = true;
            m_RaycastManager.enabled = false;
            //加载world map
            Debug.Log("加载world map");
            aRWorldMapController.LoadWorldMap();
        }
    }

    void addMvxModelWithFilePath(string fullpath)
    {
        if (m_mvxObj == null)
        {
            m_mvxObj = new GameObject("MVXPlayer");
            var fileDef = ScriptableObject.CreateInstance<MvxFileDataStreamDefinition>();
            fileDef.filePath = Path.GetFullPath(fullpath);

            MvxDataStreamDeterminer mvxDataStreamDeterminer = m_mvxObj.AddComponent<MvxDataStreamDeterminer>();
            mvxDataStreamDeterminer.playbackMode = Mvx2API.RunnerPlaybackMode.RPM_PINGPONG;
            mvxDataStreamDeterminer.dataStreamDefinition = fileDef;
            mvxDataStreamDeterminer.audioStreamEnabled = true;

            mvxDataStreamDeterminer.InitializeStream();
            if (mvxDataStreamDeterminer.isOpen)
            {
                MvxMeshTexturedRenderer mvxMeshTexturedRenderer = m_mvxObj.AddComponent<MvxMeshTexturedRenderer>();
                mvxMeshTexturedRenderer.mvxStream = mvxDataStreamDeterminer;
                mvxMeshTexturedRenderer.materialTemplates = m_materialTemplates;
            }

            m_mvxObj.transform.position = m_trackedImage.transform.position;
            m_mvxObj.transform.rotation = m_trackedImage.transform.rotation;
            m_mvxObj.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
        }
        else
        {
            var fileDef = ScriptableObject.CreateInstance<MvxFileDataStreamDefinition>();
            fileDef.filePath = Path.GetFullPath(fullpath);

            MvxDataStreamDeterminer mvxDataStreamDeterminer = m_mvxObj.GetComponent<MvxDataStreamDeterminer>();
            mvxDataStreamDeterminer.dataStreamDefinition = fileDef;
        }
    }

    public void ChangeModel(string filename)
    {
        m_filename = filename;
        string fullpath = Path.Combine(Application.persistentDataPath, filename);
        addMvxModelWithFilePath(fullpath);
    }

    void OnEnable()
    {
        m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;

        if (m_mvxObj != null)
        {
            Destroy(m_mvxObj);
            m_mvxObj = null;
        }

        if (m_trackedImage != null)
        {
            Destroy(m_trackedImage);
            m_trackedImage = null;
        }
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            if (m_trackedImage == null)
            {
                m_trackedImage = trackedImage;
                string selectModel = PlayerPrefs.GetString("selectModel");
                string src_path = Path.Combine(Application.persistentDataPath, selectModel);
                if (File.Exists(src_path))
                {
                    addMvxModelWithFilePath(src_path);
                    m_TrackedImageManager.enabled = false;
                }
            }
        }
    }

    void Update()
    {
        if (isLocationLocked)
        {
            return;
        }
        
        if (Input.touchCount == 0)
            return;

        var touch = Input.GetTouch(0);
        if (IsPointerOverUIObject(touch.position))
        {
            return;
        }

        if (touch.phase != TouchPhase.Began)
            return;

        const TrackableType trackableTypes =
            TrackableType.FeaturePoint |
            TrackableType.PlaneWithinPolygon;

        if (m_RaycastManager.Raycast(touch.position, s_Hits, trackableTypes))
        {
            var hit = s_Hits[0];
            m_mvxObj.transform.position = hit.pose.position;
            m_mvxObj.transform.rotation = hit.pose.rotation;

        }
    }

    private bool IsPointerOverUIObject(Vector2 screenPosition)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(screenPosition.x, screenPosition.y);

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    public void lockLocation(Button button)
    {
        if (isLocationLocked == false)
        {
            isLocationLocked = true;
            button.image.sprite = Resources.Load<Sprite>("suo");
            //m_ARPlaneManager.enabled = false;
            //m_RaycastManager.enabled = false;
            //m_ARPointCloudManager.enabled = false;
        }
        else
        {
            isLocationLocked = false;
            button.image.sprite = Resources.Load<Sprite>("kaisuo");
            //m_ARPlaneManager.enabled = true;
            //m_RaycastManager.enabled = true;
            //m_ARPointCloudManager.enabled = true;
        }
        
    }


    public void ScaleSliderUpdate(Slider slider)
    {
        m_mvxObj.transform.localScale = new Vector3(slider.value, slider.value, slider.value);
    }

    public void RotateSliderUpdate(Slider slider)
    {
        m_mvxObj.transform.localEulerAngles = new Vector3(m_mvxObj.transform.rotation.x, slider.value, m_mvxObj.transform.rotation.z);
    }


    public void addMvxModelWithWorldMap(string fullpath, Vector3 position, Vector3 rotation, Vector3 scale)
    {
        
        m_mvxObj = new GameObject("MVXPlayer");
        var fileDef = ScriptableObject.CreateInstance<MvxFileDataStreamDefinition>();
        fileDef.filePath = Path.GetFullPath(fullpath);

        MvxDataStreamDeterminer mvxDataStreamDeterminer = m_mvxObj.AddComponent<MvxDataStreamDeterminer>();
        mvxDataStreamDeterminer.playbackMode = Mvx2API.RunnerPlaybackMode.RPM_PINGPONG;
        mvxDataStreamDeterminer.dataStreamDefinition = fileDef;
        mvxDataStreamDeterminer.audioStreamEnabled = true;

        mvxDataStreamDeterminer.InitializeStream();
        if (mvxDataStreamDeterminer.isOpen)
        {
            MvxMeshTexturedRenderer mvxMeshTexturedRenderer = m_mvxObj.AddComponent<MvxMeshTexturedRenderer>();
            mvxMeshTexturedRenderer.mvxStream = mvxDataStreamDeterminer;
            mvxMeshTexturedRenderer.materialTemplates = m_materialTemplates;
        }

        m_mvxObj.transform.position = position;
        m_mvxObj.transform.rotation = Quaternion.Euler(rotation);
        m_mvxObj.transform.localScale = scale;
    }

}
