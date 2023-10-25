using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SimpleJSON;

[System.Serializable]
public class PatchData
{
    public int code;
    public string message;
    public List<PatchDataElement> models = null;

    public static PatchData CreateFromJSON(string jsonString)
    {
        var N = JSON.Parse(jsonString);
        PatchData ptdata = new PatchData();
        ptdata.code = N["code"].AsInt;
        ptdata.message = N["message"].Value;
        /////////////////////////////////////
        ///
        ptdata.models = new List<PatchDataElement>();
        int count = N["data"].Count;
        for(int i=0; i<count; i++)
        {
            PatchDataElement elm = new PatchDataElement(
                N["data"][i]["name"].Value,
                N["data"][i]["size"].AsInt,
                N["data"][i]["last_modified"].Value,
                N["data"][i]["expire_time"].Value,
                N["data"][i]["icon"].Value);
            ptdata.models.Add(elm);
        }
        return ptdata;
    }
    /*
    "code": 0,
    "message": "Success",
    "data": [
        {
            "name": "patch/BrandyUW.mvx",
            "size": 502493752,
            "last_modified": "2020-10-10T07:53:28.000Z",
            "expire_time": "2022-10-10 23:59:59"
        },
        {
    "name": "patch/Greg_Welcome_AR_20191010.mvx",
            "size": 153650149,
            "last_modified": "2020-10-10T07:51:33.000Z",
            "expire_time": "2022-10-10 23:59:59"
        },
        {
    "name": "patch/erica_phone_etc-1.mvx",
            "size": 501691484,
            "last_modified": "2020-10-10T07:52:28.000Z",
            "expire_time": "2022-10-10 23:59:59"
        }
    ]
    */
}

public class PatchDataElement
{
    public string _name;
    public int _size;
    public string _last_modified;
    public string _expire_time;
    public string _icon;

    public PatchDataElement(string name, int size, string last_modified, string expire_time, string icon)
    {
        _name = name;
        _size = size;
        _last_modified = last_modified;
        _expire_time = expire_time;
        _icon = icon;
    }
}
