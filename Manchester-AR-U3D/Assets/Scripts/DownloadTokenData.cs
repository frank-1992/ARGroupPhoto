using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SimpleJSON;

[System.Serializable]
public class DownloadTokenData
{
    public int code;
    public string message;

    public string data_RequestId;
    public string data_AssumedRoleUser_Arn;
    public string data_AssumedRoleUser_AssumedRoleId;

    public string data_Credentials_SecurityToken;
    public string data_Credentials_AccessKeyId;
    public string data_Credentials_AccessKeySecret;
    public string data_Credentials_Expiration;

    public static DownloadTokenData CreateFromJSON(string jsonString)
    {
        var N = JSON.Parse(jsonString);
        DownloadTokenData dtdata = new DownloadTokenData();
        dtdata.code = N["code"].AsInt;
        dtdata.message = N["message"].Value;

        dtdata.data_RequestId = N["data"]["RequestId"].Value;
        dtdata.data_AssumedRoleUser_Arn = N["data"]["AssumedRoleUser"]["Arn"].Value;
        dtdata.data_AssumedRoleUser_AssumedRoleId = N["data"]["AssumedRoleUser"]["AssumedRoleId"].Value;
        dtdata.data_Credentials_SecurityToken = N["data"]["Credentials"]["SecurityToken"].Value;
        dtdata.data_Credentials_AccessKeyId = N["data"]["Credentials"]["AccessKeyId"].Value;
        dtdata.data_Credentials_AccessKeySecret = N["data"]["Credentials"]["AccessKeySecret"].Value;
        dtdata.data_Credentials_Expiration = N["data"]["Credentials"]["Expiration"].Value;
        return dtdata;
    }
    /*
    "code": 0,
    "message": "Success",
    "data": {
        "RequestId": "6E14243C-2072-44E2-A2AC-7704AA22A85A",
        "AssumedRoleUser": {
            "Arn": "acs:ram::1926367908066145:role/stsreadonly/3dapp_download",
            "AssumedRoleId": "339806873051483729:3dapp_download"
        },
        "Credentials": {
            "SecurityToken": "CAIS+AF1q6Ft5B2yfSjIr5bMJIPD270WhvWeQxX1t2gzO+hFnobn2zz2IHtFdHFuCe8dtvk2mGBT6fwTlqcoR5ZdXXvIatR26pNe/VtI6TFoAorng4YfgbiJREKxaXeiruKwDsz9SNTCAITPD3nPii50x5bjaDymRCbLGJaViJlhHL91N0vCGlggPtpNIRZ4o8I3LGbYMe3XUiTnmW3NFkFlyGEe4CFdkf3umJPHtEWP1A2ilLFM+9nLT8L6P5U2DvBWSMyo2eF6TK3F3RNL5gJCnKUM1/cVqW+Z7o7GWwQIv0XdbLCF6L50MBRja6E2HKhAveh1G2LGBQQN/BqAATCgmWR/VeuuLVuDYt6H8Pn9kogk7PhMobYCuHYFvNLx6WDN55m7LXnbY8wa/KY/3hmpHDOB10A16F880jWidJYX8xZ8U/CzmY/YO9hDCa5Pf+kLXNx0KZEcoBrEf0ew2NJMOL/SLtNSO43VBBtkO5zYNpe0jlZlCAMjmr9xSkpZ",
            "AccessKeyId": "STS.NUyo9w6b717uA3DShf7giqCL9",
            "AccessKeySecret": "82PjbLj5UetRD4ohtk1n7HhhUuhbXsGrEUxziw3CDHFv",
            "Expiration": "2020-10-11T15:08:44Z"
        }
    }
    */
}

