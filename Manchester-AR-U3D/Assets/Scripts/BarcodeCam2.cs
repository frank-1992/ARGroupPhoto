using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ZXing;
using ZXing.QrCode;
using System.IO;
using System;


public class BarcodeCam2 : MonoBehaviour
{
    private WebCamTexture mWebcamTexBack;//接收攝影機返回的圖片數據
    private WebCamTexture mWebcamTexFront;//接收攝影機返回的圖片數據
    int back = -1, front = -1, current = -1;
    public RawImage rawImg_CamTexture;

    private BarcodeReader reader = new BarcodeReader();//ZXing的解碼
    private Result res;//儲存掃描後回傳的資訊
    private int requestW = Screen.width;
    private int requestH = Screen.height;
    private int max_length = 0;
    // Create a texture the size of the screen, RGB24 format
    private Texture2D m_tex = null;

    static bool m_ScanFlag = true;
    static public bool ScanFlag
    {
        get { return m_ScanFlag; }
        set { m_ScanFlag = value; }
    }

    void Start()
    {
        BarcodeCam2.ScanFlag = true;
        string login_mode = PlayerPrefs.GetString("login_mode");
        if (login_mode.Equals("2"))
        {
            BarcodeCam2.ScanFlag = false;
        }
        // Create a texture the size of the screen, RGB24 format
        m_tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

        RuntimePlatform platform = Application.platform;
        // Fixes rotation issue
        if (platform == RuntimePlatform.IPhonePlayer)
        {
            //CamFeedObj.transform.eulerAngles = new Vector3(0, -90, 90);
        }

        reader.Options.PossibleFormats = new List<BarcodeFormat>();
        reader.Options.PossibleFormats.Add(BarcodeFormat.QR_CODE);
        reader.AutoRotate = false;
        reader.Options.TryHarder = false;

        StartCoroutine(InitAndOpenCamera());//開啟攝影機鏡頭
    }

    private void OnDestroy()
    {
        Destroy(m_tex);
        m_tex = null;
    }

    IEnumerator InitAndOpenCamera()
    {
        yield return StartCoroutine(ApplyCamera());
        OpenBackCam();
    }
    private IEnumerator ApplyCamera()
    {
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);//授权
        }
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            //設置攝影機要攝影的區域
            Debug.Log("width=" + requestW + "  Height=" + requestH);

            int length = WebCamTexture.devices.Length;
            if (length <= 0)
            {
                Debug.Log("你的设备没有摄像头！！！");
                enabled = false;
                yield break;
            }
            else if (length == 1)
            {
                mWebcamTexBack = new WebCamTexture(WebCamTexture.devices[0].name, requestW, requestH, 12);
            }
            else
            {
                for (int i = 0; i < length; i++)
                {
                    if (WebCamTexture.devices[i].isFrontFacing)
                    {
                        front = i;
                    }
                    else
                    {
                        if (back == -1)
                        {
                            back = i;
                        }
                    }
                }
                if (front == -1)
                {
                    front = back + 1;
                }
            }
        }
    }

    void OpenBackCam()
    {
        if (back == -1)
        {
            Debug.Log("你的设备没有摄像头！！！");
            return;
        }
        current = back;
        if (mWebcamTexFront != null && mWebcamTexFront.isPlaying)
        {
            mWebcamTexFront.Stop();
        }
        
        mWebcamTexBack = new WebCamTexture(WebCamTexture.devices[current].name, requestW, requestH, 60);
        rawImg_CamTexture.texture = mWebcamTexBack;
        mWebcamTexBack.Play();

        //int videoRotationAngle = mWebcamTexBack.videoRotationAngle;
        //StretchImageFullScreen(rawImg_CamTexture, -videoRotationAngle);

        int videoRotationAngle = mWebcamTexBack.videoRotationAngle;
        //Debug.Log("videoRotationAngle="+ videoRotationAngle);
        rawImg_CamTexture.transform.rotation = Quaternion.Euler(0, 0, -videoRotationAngle);
    }

    void OpenFrontCam()
    {
        if (front == -1)
        {
            OpenBackCam();
            return;
        }

        current = front;
        if (mWebcamTexBack != null && mWebcamTexBack.isPlaying)
        {
            mWebcamTexBack.Stop();
        }

        mWebcamTexFront = new WebCamTexture(WebCamTexture.devices[front].name, max_length, max_length, 60);
        mWebcamTexFront.Play();
        rawImg_CamTexture.texture = mWebcamTexFront;
    }

    public void ChangeCamera()
    {
        if (current == back)
        {
            OpenFrontCam();
        }
        else
        {
            OpenBackCam();
        }

        WebCamTexture _webCamTexture = GetCurrent_WebCamTexture();
        int videoRotationAngle = _webCamTexture.videoRotationAngle;
        rawImg_CamTexture.transform.rotation = Quaternion.Euler(0, 0, -videoRotationAngle);
    }
    public WebCamTexture GetCurrent_WebCamTexture()
    {
        WebCamTexture _webCamTexture;
        if (current == back) _webCamTexture = mWebcamTexBack;
        else _webCamTexture = mWebcamTexFront;
        return _webCamTexture;
    }

    private void _orient()
    {
        WebCamTexture _webCamTexture = GetCurrent_WebCamTexture();
        if (_webCamTexture != null)
        {
            AspectRatioFitter rawImgARF = (AspectRatioFitter)rawImg_CamTexture.GetComponent(typeof(AspectRatioFitter));
            if (rawImgARF != null)
            {
                float physical = (float)_webCamTexture.width / (float)_webCamTexture.height;
                rawImgARF.aspectRatio = physical;
            }

            float scaleY = _webCamTexture.videoVerticallyMirrored ? -1f : 1f;
            rawImg_CamTexture.rectTransform.localScale = new Vector3(1f, scaleY, 1f);

            int orient = -_webCamTexture.videoRotationAngle;
            rawImg_CamTexture.rectTransform.localEulerAngles = new Vector3(0f, 0f, orient);
        }
    }

    private void Update()
    {
        _orient();
    }

    void OnGUI()
    {
        WebCamTexture _webCamTexture = GetCurrent_WebCamTexture();

        if (_webCamTexture != null)//若有攝影機則將攝影機拍到的畫面畫出
        {
            if (_webCamTexture.isPlaying == true)//若攝影機已開啟
            {
                StartCoroutine(readScreen());
            }
        }
    }

    IEnumerator readScreen()
    {
        // We should only read the screen buffer after rendering is complete
        yield return new WaitForEndOfFrame();

        // Read screen contents into the texture
        if (m_tex == null)
        {
            yield return null;
        }
        m_tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        m_tex.Apply();

        try
        {
            if (ScanFlag == true)
            {
                IBarcodeReader barcodeReader = new BarcodeReader();
                // decode the current frame
                var result = barcodeReader.Decode(m_tex.GetPixels32(), m_tex.width, m_tex.height);
                if (result != null)
                {
                    //Rstext.text = result.Text;
                    Debug.Log("DECODED TEXT FROM QR: " + result.Text);

                    GameObject AccountMan = GameObject.Find("AccountManager");
                    AccountManager other = (AccountManager)AccountMan.GetComponent(typeof(AccountManager));
                    other.player_login = result.Text;
                    other.doLogin();
                    //SceneManager.LoadScene("ImageTracking");
                    ScanFlag = false;
                }
                else
                {
                    ScanFlag = true;
                }
            }
        }
        catch (System.Exception ex) { Debug.LogWarning(ex.Message); }

        yield return null;
    }

 
    void OnDisable()
    {
        //當程式關閉時會自動呼叫此方法關閉攝影機
        if(mWebcamTexFront != null && mWebcamTexFront.isPlaying)
        {
            mWebcamTexFront.Stop();
        }
        if (mWebcamTexBack != null && mWebcamTexBack.isPlaying)
        {
            mWebcamTexBack.Stop();
        }
    }
}
