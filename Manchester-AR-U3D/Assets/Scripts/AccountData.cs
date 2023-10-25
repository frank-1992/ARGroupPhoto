using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SimpleJSON;

[System.Serializable]
public class AccountData
{
    public int code;
    public string message;

    public string nickname;
    public string headimgurl;
    public string open_id;
    public int sex;
    public string language;
    public string city;
    public string country;
    public string user_id;

    public string user_name;
    public string ticket;
    public string shop_id;
    public string shop_name;
    public string user_code;


    public static AccountData CreateFromJSON(string jsonString)
    {
        var N = JSON.Parse(jsonString);
        AccountData acdata = new AccountData();
        acdata.code = N["code"].AsInt;
        acdata.message = N["message"].Value;
        /////////////////////////////////////
        acdata.nickname = N["data"]["nickname"].Value;
        acdata.headimgurl = N["data"]["headimgurl"].Value;
        acdata.open_id = N["data"]["open_id"].Value;
        acdata.sex = N["data"]["sex"].AsInt;
        acdata.language = N["data"]["language"].Value;
        acdata.city = N["data"]["city"].Value;
        acdata.country = N["data"]["country"].Value;
        acdata.user_id = N["data"]["user_id"].Value;
        acdata.user_name = N["data"]["user_name"].Value;
        acdata.ticket = N["data"]["ticket"].Value;
        acdata.shop_id = N["data"]["shop_id"].Value;
        acdata.shop_name = N["data"]["shop_name"].Value;
        acdata.user_code = N["data"]["user_code"].Value;

        return acdata;
    }
    /*
     * Response Text:{"code":0,"message":"Success","data":{"nickname":"Harry","headimgurl":"http:\/\/wx.qlogo.cn\/mmopen\/g3MonUZtNHkdmzicIlibx6iaFqAc56vxLSUfpb6n5WKSYVY0ChQKkiaJSgQ1dZuTOgvLLrhJbERQQ4eMsv84eavHiaiceqxibJxCfHe\/0","open_id":"o6_bmjrPTlm6_2sgVt7hMZOPfL2M","sex":"1","language":"zh_CN","city":"\u5e7f\u5dde","country":"\u4e2d\u56fd"}}
     * 
     * 
     * 
     *     "code": 0,
    "message": "Success",
    "data": {
        "nickname": "Harry",
        "headimgurl": "http://wx.qlogo.cn/mmopen/g3MonUZtNHkdmzicIlibx6iaFqAc56vxLSUfpb6n5WKSYVY0ChQKkiaJSgQ1dZuTOgvLLrhJbERQQ4eMsv84eavHiaiceqxibJxCfHe/0",
        "open_id": "o6_bmjrPTlm6_2sgVt7hMZOPfL2M",
        "sex": "1",
        "language": "zh_CN",
        "city": "广州",
        "country": "中国",
        "user_id": "001",
        "user_name": "Harry",
        "ticket": "10001",
        "shop_id": "1",
        "shop_name": "曼联上海门店",
        "user_code": "13800138000"
    }
     */
}
