using System;
using System.Collections;
using System.Security.Cryptography;
using UnityEngine;
using System.Text;

public class NetworkManager : MonoBehaviour
{
    public static string API_SECERT_KEY = "3dapp2020!";
    public static string LOGIN_BY_QRCODE_VALUE = "https://artv.harves-e.com/api/get_user_info/v1/get_user_info?user_id=";
    public static string LOGIN_BY_QRCODE_URL = "https://artv.harves-e.com//api/get_user_info/v1/get_user_info";

    public static string LOGIN_BY_MEMBER_NO_URL = "https://artv.harves-e.com//api/get_user_info_by_member_no/v1/get_user_info_by_member_no";

    public static string GET_PATCH_LIST_URL = "https://artv.harves-e.com//api/get_patch_list/v1/get_patch_list";

    public static string GET_OSS_DOWNLOAD_TOKEN = "https://artv.harves-e.com//api/get_download_token/v1/get_download_token";

    public static string PUT_FILES_TO_SERVER_URL = "https://artv.harves-e.com//api/upload/v1/upload";

    public static string UpperCaseUrlEncode(string s)
    {
        string enstr = WWW.EscapeURL(s);
        char[] temp = enstr.ToCharArray();
        for (int i = 0; i < temp.Length - 2; i++)
        {
            if (temp[i] == '%')
            {
                temp[i + 1] = char.ToUpper(temp[i + 1]);
                temp[i + 2] = char.ToUpper(temp[i + 2]);
            }
        }
        return new string(temp);
    }
    
    public static string getMD5(string input)
    {
        return GetMD5Hash(MD5.Create(), input);
    }

    private static string GetMD5Hash(MD5 md5Hash, string input)
    {
        //Convert the input string to a byte array and compute the hash.
        byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
        //Create a new StringBuilder to collect the bytes and create a string.
        StringBuilder builder = new StringBuilder();
        //Loop through each byte of the hashed data and format each one as a hexadecimal strings.
        for (int cnt = 0; cnt < data.Length; cnt++)
        {
            builder.Append(data[cnt].ToString("x2"));
        }
        //Return the hexadecimal string
        return builder.ToString();
    }

    public static int GetUnixTime()
    {
        return (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
    }

    public static long CurrentTimestamp()
    {
        return (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds * 1000);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }
}