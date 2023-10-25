using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using DataBank;
using SimpleJSON;


public class UploadFileManager : MonoBehaviour
{
    private string current_upload_file = "";
    private string current_userid = "";
    private int current_record_id = 0;
    private int current_scr_w = 0;
    private int current_scr_h = 0;
    private int current_scr_orient = 0;
    private string current_upload_img = "";

    public static string upProgressByte = "";
    public static string upProgressPercent = "";
    public static bool bUploading = false;
    public static bool bFinish = false;

    private static GameObject _Instance = null;


    void Awake()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        if (_Instance == null)
            _Instance = gameObject;
        else
            Destroy(gameObject);

    }

    // Update is called once per frame
    void Update()
    {
    }

    public void doUpload(String fullpathwithname, String userid, int record_id, int scr_width, int scr_height, int orient)
    {
        current_upload_file = fullpathwithname;
        current_upload_img = fullpathwithname;
        current_upload_img = current_upload_img.Replace(".mp4", ".jpg");
        current_userid = userid;
        current_record_id = record_id;
        current_scr_w = scr_width;
        current_scr_h = scr_height;
        current_scr_orient = orient;

        bFinish = false;

        FileInfo info1 = new FileInfo(current_upload_file);
        FileInfo info2 = new FileInfo(current_upload_img);
        if (info1.Exists == true && info2.Exists == true)
        {
            upProgressByte = "";
            upProgressPercent = "";
            StartCoroutine(UploadProcess());
        }
        else
        {
            upProgressPercent = "100%";
            UploadVideoDB m_UpVideoDB = new UploadVideoDB();
            m_UpVideoDB.deleteDataByString("" + current_record_id);
            m_UpVideoDB.close();
            bFinish = true;
            //File.Delete(current_upload_file);
            Debug.Log("File not found, Delete record:" + current_upload_file);
        }
    }

    IEnumerator UploadProcess()
    {
        long tm = NetworkManager.GetUnixTime();
        string postStr = "blob_num=1";
        postStr += "&file_extension=mp4";
        postStr += "&height=" + current_scr_h;
        postStr += "&screen=" + current_scr_orient;
        postStr += "&timestamp=" + tm;
        postStr += "&total_blob_num=1";
        postStr += "&user_id=" + current_userid;
        postStr += "&width=" + current_scr_w;
        postStr += NetworkManager.API_SECERT_KEY;
        Debug.Log("Post string=" + postStr);

        string md5str = NetworkManager.getMD5(postStr);
        Debug.Log("md5 string=" + md5str);


        string path = current_upload_file;
        byte[] videoByte = File.ReadAllBytes(path);

        string imgpath = current_upload_img;
        byte[] imgByte = File.ReadAllBytes(imgpath);

        
        WWWForm form = new WWWForm();
        form.AddField("blob_num", "1");
        form.AddField("file_extension", "mp4");
        form.AddField("height", ""+ current_scr_h);
        form.AddField("screen", ""+ current_scr_orient);// 横屏：0，竖屏：1
        form.AddField("sign", md5str.ToLower());
        form.AddField("timestamp", "" + tm);
        form.AddField("total_blob_num", "1");
        form.AddField("user_id", ""+ current_userid);
        form.AddField("width", ""+ current_scr_w);
        form.AddBinaryData("file", videoByte, Path.GetFileName(path), "video/mp4");
        form.AddBinaryData("file_img", imgByte, Path.GetFileName(imgpath), "image/jpg");
        
        using (UnityWebRequest www = UnityWebRequest.Post(NetworkManager.PUT_FILES_TO_SERVER_URL, form))
        {
            //yield return www.SendWebRequest();
            www.SendWebRequest();
            while (!www.isDone)
            {
                Debug.Log("Uploading file. Progress " + (int)(www.uploadProgress * 100.0f) + "%  Bytes=" + www.uploadedBytes);
                upProgressByte = "" + www.uploadedBytes + " B";
                upProgressPercent = "" + (int)(www.uploadProgress * 100.0f) + "%";
                yield return new WaitForSeconds(0.1f);
            }

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
                bFinish = true;
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
                    
                    if (code==0 && message.Equals("Success"))
                    {
                        //delete record
                        UploadVideoDB m_UpVideoDB = new UploadVideoDB();
                        m_UpVideoDB.deleteDataByString("" + current_record_id);
                        m_UpVideoDB.close();
                        File.Delete(current_upload_file);
                        Debug.Log("Delete record:" + current_upload_file);
                        File.Delete(current_upload_img);
                        Debug.Log("Delete preview image:" + current_upload_img);
                    }
                }
                bFinish = true;
            }
        }
    }
}
