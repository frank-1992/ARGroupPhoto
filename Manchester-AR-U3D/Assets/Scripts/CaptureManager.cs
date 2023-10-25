using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using RenderHeads.Media.AVProVideo.Demos;
using RenderHeads.Media.AVProMovieCapture;
using UnityEngine;

public class CaptureManager : MonoBehaviour
{
    public CaptureBase movieCapture;
    //public GameObject videoPlaybackManager;
    public Canvas captureCanvas;
    //public Canvas playbackCanvas;
    private bool isHumanStart;
    private bool isPause;
    private float delayTime=-1.0f;
    private bool openSnd = false;
    private Vector3 _pos;
    private Quaternion _rotate;
    private string _filename;

    private void Awake()
    {
        isPause = false;
        //videoPlaybackManager.SetActive(false);//manager object
        //playbackCanvas.gameObject.SetActive(false);//UI object
    }
    // Start is called before the first frame update
    void Start()
    {
        //举例一些录制⏺的简单设置-----可以用代码设置也可以在Editor设置
        movieCapture.ResolutionDownScale = CaptureBase.DownScale.Original;//原画画质
        movieCapture.FrameRate = 30.0f;//帧数
        movieCapture.StopAfterSecondsElapsed = 15;//自动录制15s停止
        movieCapture.AppendFilenameTimestamp = false;//文件名是否加时间
                                                 // movieCapture.OutputFolderPath = Path.Combine(Application.streamingAssetsPath);//文件路径(正式项目建议新建文件夹,此处只是举例)
        movieCapture.OutputFolderPath = Path.Combine(Application.persistentDataPath);//文件路径(正式项目建议新建文件夹,此处只是举例)
        movieCapture.FilenamePrefix = "MVX";//视频名字前缀
    //.........等等,详见内部文档
}

    // Update is called once per frame
    void Update()
    {
        //如果拍攝完畢，而又是用戶自已按拍攝鍵，那就跳到playback UI
        bool isCapture = movieCapture.IsCapturing();
        //Debug.Log("CaptureManager Update: isCapture="+isCapture+" isHumanStart="+isHumanStart);
        if (isCapture == false && isHumanStart==true)
        {
            Debug.Log("CaptureManager Update: jump to playback");
            isHumanStart = false;
            JumpToPlayback();
            delayTime = 0.0f;
        }
    }

    public void JumpToPlayback()
    {
        GameObject go = GameObject.Find("UICamera");
        if (go!=null)
        {
            DemoScript other = (DemoScript)go.GetComponent(typeof(DemoScript));
            if (other!=null)
            {
                other.GoPlaybackScene();
            }
        }
    }

    public void JumpToAR()
    {
        //AR mode=====>
        CameraManager._inst.CloseUICamera();
        captureCanvas.gameObject.SetActive(true);
        //playbackCanvas.gameObject.SetActive(false);
        //videoPlaybackManager.SetActive(false);
        openSnd = false;
    }

    public void doStartCapture()
    {
        StartCoroutine(StartCapture());
    }

    IEnumerator StartCapture()
    {
        delayTime = -1.0f;
        movieCapture.StartCapture();
        isHumanStart = true;
        Debug.Log("CaptureManager StartCapture: isPause="+isPause+" isHumanStart = "+isHumanStart);
        yield return null;
    }

    public void StopCapture()
    {
        if (delayTime >= 0.0f)
        {
            return;
        }
        delayTime = -1.0f;
        isHumanStart = false;
        movieCapture.StopCapture();
        JumpToPlayback();
        Debug.Log("CaptureManager StopCapture: isPause="+ isPause+" isHumanStart = "+isHumanStart);
    }

    public void PauseCapture()
    {
        if (delayTime>-1.5f && delayTime<0.0f)
        {
            isPause = !isPause;
            if (isPause)
            {
                Debug.Log("CaptureManager PauseCapture: isPause=" + isPause);
                movieCapture.PauseCapture();
                isHumanStart = false;
            }
            else
            {
                Debug.Log("CaptureManager PauseCapture: isPause=" + isPause);
                movieCapture.ResumeCapture();
                isHumanStart = true;
            }
        }
    }
}
