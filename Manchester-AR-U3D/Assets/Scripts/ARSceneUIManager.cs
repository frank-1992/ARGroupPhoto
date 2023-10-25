using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.SceneManagement;

public class ARSceneUIManager : MonoBehaviour
{
    [SerializeField]
    private GameObject initialPanel;

    [SerializeField]
    private GameObject recordingPanel;

    [SerializeField]
    private GameObject watermaskPanel;

    public Slider progressBar;
    public GameObject initialPanelDivider;
    public GameObject modelListView;
    public Button modelListSelector = null;
    public Button rotate_scale_btn = null;
    public Slider rotateBar = null;
    public Slider scaleBar = null;
    public Button saveWorldMapbtn;
    public Button loadWorldMapBtn;
    private float maxDuration = 15.0f;
    private float recordedTime = 0.0f;
    private bool isRecordingFinished = false;
    private bool isRecording = false;
    private bool modelFlag = false;
    private float rotateValue = 0.0f;
    private float scaleValue = 0.0f;
    private bool mShowRotateScale = false;
    private bool mShowModelListText = false;

    public float rotateVal
    {
        get { return rotateValue; }
        set { rotateValue = value; }
    }
    public float scaleVal
    {
        get { return scaleValue; }
        set { scaleValue = value; }
    }

    // private ARTrackedImageManager mgr;
    void Awake()
    {
        Debug.Log(Application.persistentDataPath);
        // mgr = ARSessionOrigin.gameObject.GetComponent<ARTrackedImageManager>();
        recordingPanel.gameObject.SetActive(false);
        watermaskPanel.gameObject.SetActive(false);
        initialPanelDivider.SetActive(false);
        modelListView.SetActive(false);
        //initialPanelDivider.SetActive(true);
        //modelListView.SetActive(true);
        //uploadPanel.SetActive(false);
        saveWorldMapbtn.gameObject.SetActive(false);
        loadWorldMapBtn.gameObject.SetActive(false);
        rotate_scale_btn.gameObject.SetActive(true);
        rotateBar.gameObject.SetActive(false);
        scaleBar.gameObject.SetActive(false);
        
    }

    // Start is called before the first frame update
    void Start()
    {
        //allow auto rotate
        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = true;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isRecording && progressBar != null && !isRecordingFinished)
        {
            recordedTime += Time.deltaTime;
            progressBar.value = Mathf.Min(1, recordedTime / maxDuration);
            if (recordedTime >= maxDuration)
            {
                progressBar.value = 1.0f;
                Debug.Log("ARSceneUIManager: Update Max duration reached");
                isRecordingFinished = true;
                isRecording = false;
            }
        }
        if (rotateBar!=null && rotateBar.value != rotateValue)
        {
            rotateValue = rotateBar.value;
            //Debug.Log("rotateValue=" + rotateValue);
        }
        if (scaleBar != null && scaleBar.value != scaleValue)
        {
            scaleValue = scaleBar.value;
            //Debug.Log("scaleValue=" + scaleValue);
        }
    }

    public void ShowRotateScaleClick()
    {
        mShowRotateScale = !mShowRotateScale;
        if (rotateBar != null) {
            rotateBar.gameObject.SetActive(mShowRotateScale);
        }
        if (scaleBar != null) {
            scaleBar.gameObject.SetActive(mShowRotateScale);
        }
        if (saveWorldMapbtn != null && loadWorldMapBtn != null)
        {
            saveWorldMapbtn.gameObject.SetActive(mShowRotateScale);
            loadWorldMapBtn.gameObject.SetActive(mShowRotateScale);
        }
        
    }

    public void ShowUploadDialogPanel()
    {
        //uploadPanel.SetActive(true);
    }

    public void ShowARGamingPanel()
    {
        initialPanel.gameObject.SetActive(true);
        recordingPanel.gameObject.SetActive(false);
        watermaskPanel.gameObject.SetActive(false);
        //reset value
        StopCapture();
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene("ImageTracking");
    }

    public void ShowARRecordingPanel()
    {
        //disable auto rotate
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        
        initialPanel.gameObject.SetActive(false);
        recordingPanel.gameObject.SetActive(true);
        watermaskPanel.gameObject.SetActive(true);

        rotate_scale_btn.gameObject.SetActive(false);
        rotateBar.gameObject.SetActive(false);
        scaleBar.gameObject.SetActive(false);

        saveWorldMapbtn.gameObject.SetActive(false);
        loadWorldMapBtn.gameObject.SetActive(false);

        isRecording = true;
    }

    public void StopCapture()
    {
        isRecording = false;
        isRecordingFinished = false;
        recordedTime = 0.0f;
        progressBar.value = 0.0f;
    }

    public void PauseCapture()
    {
        isRecording = !isRecording;
    }

    public void ChangeModelListText()
    {
        mShowModelListText = !mShowModelListText;
        if (mShowModelListText)
        {
            Text btnTxt = modelListSelector.GetComponent<Text>();
            if (btnTxt!=null)
            {
                btnTxt.text = "更多";
            }
        }
        else
        {
            Text btnTxt = modelListSelector.GetComponent<Text>();
            if (btnTxt != null)
            {
                btnTxt.text = "收起";
            }
        }
    }

    public void ShowModelList()
    {
        if (!initialPanelDivider.activeSelf)
        {
            Image bg = initialPanel.GetComponent<Image>();
            //bg.color = new Color(1f, 1f, 1f, 0.35f);
            bg.color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
            initialPanelDivider.SetActive(true);
            modelListView.SetActive(true);

            modelListSelector.GetComponentInChildren<Text>().text = "收起";
        }
        else
        {
            Image bg = initialPanel.GetComponent<Image>();
            bg.color = new Color(1f, 1f, 1f, 0f);
            initialPanelDivider.SetActive(false);
            modelListView.SetActive(false);

            modelListSelector.GetComponentInChildren<Text>().text = "更多";
        }
    }

}
