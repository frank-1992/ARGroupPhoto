using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Text;
using Umeng;

public class AccountManager : MonoBehaviour
{
    [DllImport("__Internal")]
    static extern void openFlash();

    [DllImport("__Internal")]
    static extern void closeFlash();

    public GameObject ManualInputPanel = null;
    public GameObject LoadingPanel = null;
    public GameObject UserInfoPanel = null;
    public GameObject backbtn2 = null;
    public GameObject lightoff = null;
    public GameObject lighton = null;
    public GameObject lightoff_L = null;
    public GameObject lighton_L = null;
    public InputField user_manual_number = null;
    public Text user_name_str = null;
    public Text user_detail_str = null;
    public string m_player_login = "";
    public Text error_message = null;
    public Button error_ok = null;
    private static string loginProgress = "";
    private ScreenOrientation prevOrient = 0;
    private Boolean portrait = false;
    private Boolean isLightOn = false;

    [SerializeField]
    public string player_login
    {
        get { return m_player_login; }
        set { m_player_login = value; }
    }

    // Start is called before the first frame update
    void Start()
    {
        checkOrient();
        string login_mode = PlayerPrefs.GetString("login_mode");
        if (login_mode.Equals("1"))
        {
            ManualInputPanel.SetActive(false);
            LoadingPanel.SetActive(false);
            UserInfoPanel.SetActive(false);
            backbtn2.SetActive(false);
            error_ok.gameObject.SetActive(false);
            error_message.gameObject.SetActive(false);
            doLightOff();
        }
        else if (login_mode.Equals("2"))
        {
            ManualInputPanel.SetActive(true);
            LoadingPanel.SetActive(false);
            UserInfoPanel.SetActive(false);
            backbtn2.SetActive(true);
            error_ok.gameObject.SetActive(false);
            error_message.gameObject.SetActive(false);
            hideAllLight();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (error_message!=null)
        {
            if (error_message.isActiveAndEnabled)
            {
                error_message.text = loginProgress;
            }
        }
        checkOrient();
    }

    public void checkOrient()
    {
        if (Screen.orientation == ScreenOrientation.Portrait || Screen.orientation == ScreenOrientation.PortraitUpsideDown)
        {
            //codes for portrait
            if ( prevOrient != Screen.orientation)
            {
                prevOrient = Screen.orientation;
                portrait = true;
                showPortLight();
            }
        }
        else
        {
            //codes for Landspace;
            if (prevOrient != Screen.orientation)
            {
                prevOrient = Screen.orientation;
                portrait = false;
                showLandLight();
            }
        }
    }


    public void hideAllLight()
    {
        lightoff.SetActive(false);
        lighton.SetActive(false);
        lightoff_L.SetActive(false);
        lighton_L.SetActive(false);
    }

    public void showPortLight()
    {
        lightoff_L.SetActive(false);
        lighton_L.SetActive(false);
        if (isLightOn==false)
        {
            lightoff.SetActive(false);
            lighton.SetActive(true);
        }
        else
        {
            lightoff.SetActive(true);
            lighton.SetActive(false);
        }
    }

    public void showLandLight()
    {
        lightoff.SetActive(false);
        lighton.SetActive(false);
        if (isLightOn == false)
        {
            lightoff_L.SetActive(false);
            lighton_L.SetActive(true);
        }
        else
        {
            lightoff_L.SetActive(true);
            lighton_L.SetActive(false);
        }
    }

    public void doLightOn()
    {
        isLightOn = true;
        if (portrait)
        {
            showPortLight();
        }
        else
        {
            showLandLight();
        }
#if UNITY_IPHONE
        openFlash();
#endif
    }

    public void doLightOff()
    {
        isLightOn = false;
        if (portrait)
        {
            showPortLight();
        }
        else
        {
            showLandLight();
        }
#if UNITY_IPHONE
        closeFlash();
#endif
    }

    public void doLogin()
    {
        if (player_login.Equals("") || player_login == null)
        {
            return;
        }
        LoadingPanel.SetActive(true);
        error_ok.gameObject.SetActive(false);
        error_message.gameObject.SetActive(true);
        doLightOff();

        StartCoroutine(loignProcess(player_login));
    }

    public void doLoginByNumber()
    {
        if (user_manual_number == null)
        {
            return;
        }
        string user_number = user_manual_number.text;
        if (user_number.Equals(""))
        {
            return;
        }

        LoadingPanel.SetActive(true);
        error_ok.gameObject.SetActive(false);
        error_message.gameObject.SetActive(true);

        StartCoroutine(loignByNumberProcess(user_number));
    }

    IEnumerator loignProcess(string user_login)
    {
        long tm = NetworkManager.GetUnixTime();
        //string info_link = NetworkManager.LOGIN_BY_QRCODE_VALUE + user_login;
        string info_link = user_login;
        string info_link_uri = NetworkManager.UpperCaseUrlEncode(info_link);

        string postStr = "info_str=" + info_link_uri;
        postStr += "&timestamp=" + tm;
        postStr += NetworkManager.API_SECERT_KEY;
        Debug.Log("Post string=" + postStr);

        string md5str = NetworkManager.getMD5(postStr);
        Debug.Log("md5 string=" + md5str);


        WWWForm form = new WWWForm();
        
        form.AddField("info_str", info_link);
        //form.AddField("sign", "d59b8c25a684746c70d3b60ddea53cd5");
        form.AddField("sign", md5str.ToLower());
        form.AddField("timestamp", "" + tm);

        using (UnityWebRequest www = UnityWebRequest.Post(NetworkManager.LOGIN_BY_QRCODE_URL, form))
        {
            www.timeout = 30;

            long startT = NetworkManager.GetUnixTime();
            www.SendWebRequest();
            while (!www.isDone)
            {                
                long currT = NetworkManager.GetUnixTime();
                long diff = currT - startT;
                if (diff <= 30)
                {
                    long show = 30 - diff;
                    loginProgress = "请等待...   " + show + " 秒";
                    yield return new WaitForSeconds(0.1f);
                }
                else
                    break;
            }
            //yield return www.SendWebRequest();

            if (!www.isDone || www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
                loginProgress = www.error;
                error_ok.gameObject.SetActive(true);
                player_login = "";
            }
            else
            {
                Debug.Log("Post request complete!" + " Response Code: " + www.responseCode);
                string responseText = www.downloadHandler.text;
                Debug.Log("Response Text:" + responseText);

                AccountData userdata = AccountData.CreateFromJSON(responseText);

                if (userdata.code==0 && userdata.message.Equals("Success"))
                {
                    GA.Event("login");
                    user_name_str.text = userdata.nickname;
                    string userstr = "\n";
                    userstr += "微信号码：" + userdata.open_id + "\n\n";
                    userstr += "地区：" + userdata.city + "\n\n";
                    userstr += "票码：" + userdata.ticket;
                    user_detail_str.text = userstr;

                    PlayerPrefs.SetString("userid", userdata.user_id);
                    UserInfoPanel.SetActive(true);
                    backbtn2.SetActive(true);

                    GameObject head = GameObject.Find("HeadImgManager");
                    if (head != null)
                    {
                        HeadImgManager hMan = (HeadImgManager)head.GetComponent(typeof(HeadImgManager));
                        hMan.doGetImage(userdata.headimgurl);
                    }
                }
                else // failed
                {
                    player_login = "";
                    loginProgress = "二维码错误，请再拍一次。";
                    error_ok.gameObject.SetActive(true);
                }
            }
        }
    }

    IEnumerator loignByNumberProcess(string user_number)
    {
        long tm = NetworkManager.GetUnixTime();
        string postStr = "member_no=" + user_number;
        postStr += "&timestamp=" + tm;
        postStr += NetworkManager.API_SECERT_KEY;
        Debug.Log("Post string=" + postStr);

        string md5str = NetworkManager.getMD5(postStr);
        Debug.Log("md5 string=" + md5str);

        WWWForm form = new WWWForm();
        form.AddField("member_no", user_number);
        form.AddField("sign", md5str.ToLower());
        form.AddField("timestamp", "" + tm);

        using (UnityWebRequest www = UnityWebRequest.Post(NetworkManager.LOGIN_BY_MEMBER_NO_URL, form))
        {
            www.timeout = 30;

            long startT = NetworkManager.GetUnixTime();
            www.SendWebRequest();
            while (!www.isDone)
            {                
                long currT = NetworkManager.GetUnixTime();
                long diff = currT - startT;
                if (diff <= 30)
                {
                    long show = 30 - diff;
                    loginProgress = "请等待...   " + show + " 秒";
                    yield return new WaitForSeconds(0.1f);
                }
                else
                    break;
            }
            //yield return www.SendWebRequest();

            if (!www.isDone || www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
                player_login = "";
                loginProgress = www.error;
                error_ok.gameObject.SetActive(true);
            }
            else
            {
                Debug.Log("Post request complete!" + " Response Code: " + www.responseCode);
                string responseText = www.downloadHandler.text;
                Debug.Log("Response Text:" + responseText);

                AccountData userdata = AccountData.CreateFromJSON(responseText);

                if (userdata.code==0 && userdata.message.Equals("Success"))
                {
                    GA.Event("login");
                    user_name_str.text = userdata.nickname;
                    string userstr = "\n";
                    userstr += "微信号码：" + userdata.open_id + "\n\n";
                    userstr += "地区：" + userdata.city + "\n\n";
                    userstr += "票码：" + userdata.ticket;
                    user_detail_str.text = userstr;

                    PlayerPrefs.SetString("userid", userdata.user_id);
                    UserInfoPanel.SetActive(true);
                    backbtn2.SetActive(true);

                    GameObject head = GameObject.Find("HeadImgManager");
                    if (head != null)
                    {
                        HeadImgManager hMan = (HeadImgManager)head.GetComponent(typeof(HeadImgManager));
                        hMan.doGetImage(userdata.headimgurl);
                    }
                }
                else // failed
                {
                    player_login = "";
                    loginProgress = "用户号码错误，请重新输入。";
                    error_ok.gameObject.SetActive(true);
                }
            }
        }
    }

    public void backToPreviouPanel()
    {
        string login_mode = PlayerPrefs.GetString("login_mode");
        if (login_mode.Equals("1"))//qrcode
        {
            BarcodeCam2.ScanFlag = true;
            ManualInputPanel.SetActive(false);
            LoadingPanel.SetActive(false);
            UserInfoPanel.SetActive(false);
            backbtn2.SetActive(false);
            error_ok.gameObject.SetActive(false);
            error_message.gameObject.SetActive(false);
            doLightOff();
        }
        else if (login_mode.Equals("2"))//manual input
        {
            if (UserInfoPanel.activeSelf)
            {
                ManualInputPanel.SetActive(true);
                LoadingPanel.SetActive(false);
                UserInfoPanel.SetActive(false);
                backbtn2.SetActive(true);
                error_ok.gameObject.SetActive(false);
                error_message.gameObject.SetActive(false);
                hideAllLight();
            }
            else
            {
                GameObject gobj = GameObject.Find("Main Camera");
                if (gobj != null)
                {
                    DemoScript dscript = (DemoScript)gobj.GetComponent(typeof(DemoScript));
                    dscript.GoCoverScene();
                }
            }
        }
    }

    public void goARScene()
    {
        SceneManager.LoadScene("ImageTracking");
    }

    public void scanAgain()
    {
        BarcodeCam2.ScanFlag = true;
        LoadingPanel.SetActive(false);
    }
}