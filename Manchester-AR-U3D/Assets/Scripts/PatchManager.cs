using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Aliyun.OSS;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using SimpleJSON;
using Aliyun.OSS.Common;
using System.Threading;

public class PatchManager : MonoBehaviour
{
    //public Text fileNameTitle = null;
    public Text fileNameText = null;
    public Text fileSizeText = null;
    public Text filePercent = null;
    public GameObject downloadPatchPanel = null;
    private PatchData userdata = null;
    private DownloadTokenData tokenData = null;
    private Boolean bStartUpdate = false;
    private int currentDownload = 0;
    private Thread thread = null;
    static string progress_fname = "";
    static string progress_percent = "";
    static string progress_byte = "";
    bool isStopped = true;
    private int pressCount = 0;
    private float delta = 0.0f;

    void Awake()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        //auto rotate
        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = true;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;

        //auto start if no model
        if (downloadPatchPanel != null)
        {
            string patchlst = PlayerPrefs.GetString("patchlist");
            if (patchlst==null || patchlst.Length==0)
            {
                pressCount = 5;
                doPatch();
            }
            else
            {
                downloadPatchPanel.SetActive(false);
            }
        }
    }

    


    // Update is called once per frame
    void Update()
    {
        if (pressCount>0)
        {
            delta += Time.deltaTime;
            if (delta > 1.0f)
            {
                delta = 0.0f;
                pressCount = 0;
            }
        }

        //fileNameTitle.text = "" + Time.deltaTime;
        fileNameText.text = progress_fname;
        fileSizeText.text = progress_byte;
        filePercent.text = progress_percent;
        if (tokenData != null && userdata != null && bStartUpdate==true)
        {
            bStartUpdate = false;
            Debug.Log("Update Start:");

            if (currentDownload < (userdata.models.Count * 2))
            {
                isStopped = false;
                thread = new Thread(downloadPatches);
                thread.Start();
                Debug.Log("thread Start:");
                //set the 1st obj
                if (currentDownload == 0)
                {
                    string model_name = Path.GetFileName(userdata.models[0]._name);
                    PlayerPrefs.SetString("selectModel", model_name);
                    Debug.Log("set the 1st model = "+ model_name);
                }
            }
            else
            {
                Debug.Log("thread Abort:");
                if (thread!=null)
                {
                    thread.Abort();
                }
                isStopped = true;
                if (downloadPatchPanel != null)
                {
                    downloadPatchPanel.SetActive(false);
                    Screen.sleepTimeout = (int)SleepTimeout.SystemSetting;

                    //string selectModel = PlayerPrefs.GetString("selectModel");
                }
            }
        }
    }

    public void doPatch()
    {
        pressCount++;
        if (pressCount >= 5)
        {
            delta = 0.0f;
            pressCount = 0;
            if (downloadPatchPanel != null)
            {
                downloadPatchPanel.SetActive(true);
                //don't sleep
                Screen.sleepTimeout = (int)SleepTimeout.NeverSleep;
            }
            currentDownload = 0;
            isStopped = true;
            bStartUpdate = false;
            StartCoroutine(getDownloadToken());
        }
    }

    IEnumerator getPatchList()
    {
        long tm = NetworkManager.GetUnixTime();
        string postStr = "timestamp=" + tm;
        postStr += NetworkManager.API_SECERT_KEY;
        Debug.Log("Post string=" + postStr);

        string md5str = NetworkManager.getMD5(postStr);
        Debug.Log("md5 string=" + md5str);

        WWWForm form = new WWWForm();
        form.AddField("sign", md5str.ToLower());
        form.AddField("timestamp", "" + tm);

        using (UnityWebRequest www = UnityWebRequest.Post(NetworkManager.GET_PATCH_LIST_URL, form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Post request complete!" + " Response Code: " + www.responseCode);
                string responseText = www.downloadHandler.text;
                Debug.Log("Response Text:" + responseText);

                if (www.responseCode == 200)
                {
                    var N = JSON.Parse(responseText);
                    int code = N["code"].AsInt;
                    string message = N["message"].Value;

                    if (code == 0 && message.Equals("Success"))
                    {
                        userdata = PatchData.CreateFromJSON(responseText);
                        PlayerPrefs.SetString("patchlist", responseText);
                        bStartUpdate = true;
                    }
                }
            }
        }
    }

    IEnumerator getDownloadToken()
    {
        Debug.Log("getDownloadToken Start:");
        bStartUpdate = false;

        long tm = NetworkManager.GetUnixTime();
        string postStr = "timestamp=" + tm;
        postStr += NetworkManager.API_SECERT_KEY;
        Debug.Log("Post string=" + postStr);

        string md5str = NetworkManager.getMD5(postStr);
        Debug.Log("md5 string=" + md5str);

        WWWForm form = new WWWForm();
        form.AddField("sign", md5str.ToLower());
        form.AddField("timestamp", "" + tm);

        using (UnityWebRequest www = UnityWebRequest.Post(NetworkManager.GET_OSS_DOWNLOAD_TOKEN, form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Post request complete!" + " Response Code: " + www.responseCode);
                string responseText = www.downloadHandler.text;
                Debug.Log("Response Text:" + responseText);

                if (www.responseCode==200)
                {
                    var N = JSON.Parse(responseText);
                    int code = N["code"].AsInt;
                    string message = N["message"].Value;

                    if (code == 0 && message.Equals("Success"))
                    {
                        tokenData = DownloadTokenData.CreateFromJSON(responseText);
                        if (tokenData != null)
                        {
                            StartCoroutine(getPatchList());
                        }
                    }
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (thread != null)
        {
            thread.Abort();
        }
        isStopped = true;
    }

    private void OnApplicationQuit()
    {
        if (thread!=null)
        {
            thread.Abort();
        }
        isStopped = true;
    }

    private void downloadPatches()
    {
        Debug.Log("downloadPatches Start:");
        while (!isStopped)
        {
            if (tokenData != null && userdata != null)
            {
                string endpoint = "oss-cn-beijing.aliyuncs.com";
                string bucketName = "3dapp";
                string objectName = "";
                string downloadFilename = "";
                string checkpointDir = "" + Path.Combine(Application.persistentDataPath, "checkpointdir");
                if (!Directory.Exists(checkpointDir))
                {
                    Directory.CreateDirectory(checkpointDir);
                }

                if (currentDownload < (userdata.models.Count*2))
                {
                    int realinx = (int)(currentDownload / 2);
                    int subinx = currentDownload % 2;
                    PatchDataElement elm = userdata.models[realinx];
                    objectName = elm._name;
                    downloadFilename = Path.Combine(Application.persistentDataPath, Path.GetFileName(elm._name));
                    if (subinx == 1)
                    {
                        int status = 0;
                        string realpath = "";
                        //"https://harrytest1.oss-cn-shanghai.aliyuncs.com/video/01/icon01.png"
                        string[] split = elm._icon.Split(new Char[] { '/' });
                        for (int i=0; i< split.Length; i++)
                        {
                            if (status == 2){
                                realpath += "/";
                            }
                            if (status==0 && split[i].Contains(endpoint)){
                                status = 1;
                                continue;
                            }
                            if (status >= 1){
                                status = 2;
                                realpath += split[i];
                            }
                        }
                        objectName = realpath;
                        downloadFilename = Path.Combine(Application.persistentDataPath, Path.GetFileName(elm._icon));
                    }
                    
                    progress_fname = objectName;

                    //if found at local, delete it
                    FileInfo info = new FileInfo(downloadFilename);
                    if (info.Exists == true)
                    {
                        File.Delete(downloadFilename);
                    }
                }
                else
                {
                    //quit thread
                    isStopped = true;
                    bStartUpdate = true;
                    return;
                }


                Debug.Log("getDownloadToken Start OssClient:");
                OssClient client = new OssClient(endpoint,
                    tokenData.data_Credentials_AccessKeyId,
                    tokenData.data_Credentials_AccessKeySecret,
                    tokenData.data_Credentials_SecurityToken);
                Debug.Log("endpt:" + endpoint +
                    "  aid:" + tokenData.data_Credentials_AccessKeyId +
                    "  asecret:" + tokenData.data_Credentials_AccessKeySecret +
                    "  token:" + tokenData.data_Credentials_SecurityToken +
                    "  objectName:" + objectName +
                    "  downloadFilename:" + downloadFilename +
                    "  checkpointDir:" + checkpointDir);
                try
                {
                    // 通过DownloadObjectRequest设置多个参数。
                    DownloadObjectRequest request = new DownloadObjectRequest(bucketName, objectName, downloadFilename)
                    {
                        // 指定下载的分片大小。
                        PartSize = 8 * 1024 * 1024,
                        // 指定并发线程数。
                        ParallelThreadCount = 3,
                        // checkpointDir用于保存断点续传进度信息。如果某一分片下载失败，再次下载时会根据文件中记录的点继续下载。如果checkpointDir为null，断点续传功能不会生效，每次失败后都会重新下载。
                        CheckpointDir = checkpointDir,
                    };
                    request.StreamTransferProgress += streamProgressCallback;
                    progress_percent = "";
                    progress_byte = "";
                    // 断点续传下载。
                    client.ResumableDownloadObject(request);
                    Debug.LogFormat("Resumable download object:{0} succeeded", objectName);
                }
                catch (OssException ex)
                {
                    Debug.LogFormat("Failed with error code: {0}; Error info: {1}. \nRequestID:{2}\tHostID:{3}",
                        ex.ErrorCode, ex.Message, ex.RequestId, ex.HostId);
                }
                catch (Exception ex)
                {
                    Debug.LogFormat("Failed with error info: {0}", ex.Message);
                }

                currentDownload++;
            }
        }
        Debug.Log("downloadPatches Stop:");
    }

    private static void streamProgressCallback(object sender, StreamTransferProgressArgs args)
    {
        long percent = args.TransferredBytes * 100 / args.TotalBytes;

        System.Console.WriteLine("ProgressCallback - Progress: {0}%, TotalBytes:{1}, TransferredBytes:{2} ",
            percent, args.TotalBytes, args.TransferredBytes);

        progress_percent = "" + percent + "%";
        progress_byte = "" + HumanReadableFilesize(args.TransferredBytes) + " / " + HumanReadableFilesize(args.TotalBytes); //+ " B";
    }


    private static String HumanReadableFilesize(double size)
    {
        String[] units = new String[] { "B", "KB", "MB", "GB", "TB", "PB" };
        double mod = 1024.0;
        int i = 0;
        while (size >= mod)
        {
            size /= mod;
            i++;
        }
        return Math.Round(size) + units[i];
    }
}
