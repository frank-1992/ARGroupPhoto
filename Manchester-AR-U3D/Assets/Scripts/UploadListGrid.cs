using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using DataBank;
using System.IO;
using System;

public class UploadListGrid : MonoBehaviour
{
	[DllImport("__Internal")]
	static extern void createThumbNail(string pVideoPath, string pVideoName);

	public GameObject prefab; // This is our prefab object that will be exposed in the inspector

	//public RawImage preview;
	private List<UploadVideoEntity> upList = null;
	private List<GameObject> objlist = null;
	private float delta = 0.0f;

	void Start()
	{
		readDBList();
	}

	void Update()
	{
		UpBtn();
		updateStats();
	}

	public void createPreview(string videoPath, string videoName)
	{
#if UNITY_IPHONE
		createThumbNail(videoPath, videoName);
#endif
	}

	public void updateStats()
    {
		string b = UploadFileManager.upProgressByte;
		string bp =UploadFileManager.upProgressPercent;

		if (UploadFileManager.bUploading && objlist != null && objlist.Count>0)
        {
			GameObject percent = GetChildWithName(objlist[0], "percent");
			if (percent != null)
			{
				Text txt2 = percent.GetComponent<Text>();
				txt2.text = bp;
			}

			if (bp.Equals("100%") && UploadFileManager.bFinish==true)
            {
				delta += Time.deltaTime;
				if (delta>1.0f)
                {
					delta = 0.0f;
					upList.RemoveAt(0);
					GameObject newObj = objlist[0];
					Destroy(newObj);
					objlist.RemoveAt(0);
					UploadFileManager.bUploading = false;
				}
			}
		}
	}

	public void UpBtn()
    {
		if (UploadFileManager.bUploading == false && upList != null && upList.Count > 0)
        {
			UploadVideoEntity entity = upList[0];
			if (entity != null)//have record
			{
				//create preview picture
				string nameonly = Path.GetFileNameWithoutExtension(entity._filename);
				createPreview(entity._filename, nameonly);

				GameObject gobj = GameObject.Find("UploadManager");
				if (gobj != null)
				{
					UploadFileManager dscript = (UploadFileManager)gobj.GetComponent(typeof(UploadFileManager));
					if (dscript != null)
					{
						delta = 0.0f;
						UploadFileManager.bUploading = true;
						dscript.doUpload(entity._filename, entity._userid, entity._id,
							int.Parse(entity._scrwidth), int.Parse(entity._scrheight), int.Parse(entity._orient));
					}
				}
			}
		}
	}

	void readDBList()
	{
		//Fetch All Data
		UploadVideoDB m_UpVideoDB = new UploadVideoDB();
		System.Data.IDataReader reader = m_UpVideoDB.getAllData();
		//int fieldCount = reader.FieldCount;
		upList = new List<UploadVideoEntity>();
        List<int> delList = new List<int>();
		while (reader.Read())
		{
			FileInfo info = new FileInfo(reader[1].ToString());
			if (info.Exists == true)
			{
				UploadVideoEntity entity = new UploadVideoEntity(int.Parse(reader[0].ToString()),
									reader[1].ToString(),
									reader[2].ToString(),
									reader[3].ToString(),
									reader[4].ToString(),
									reader[5].ToString(),
									reader[6].ToString(),
									reader[7].ToString(),
									reader[8].ToString());
				Debug.Log("readDBList id: " + entity._id + " filename=" + entity._filename);
				upList.Add(entity);
			}
			else
            {
				delList.Add(int.Parse(reader[0].ToString()));
			}
		}
		for(int i=0; i< delList.Count; i++)
        {
			int delid = delList[i];
			m_UpVideoDB.deleteDataByString(""+delid);
			Debug.Log("Delete crash file=" + delid);
		}
		delList.Clear();
		m_UpVideoDB.close();

		objlist = new List<GameObject>();
		for (int i=0; i<upList.Count; i++)
		{
			UploadVideoEntity elm = upList[i];
			int inx = i;
			// Create new instances of our prefab until we've created as many as we specified
			GameObject newObj = (GameObject)Instantiate(prefab, transform);
			newObj.name = "uplist_obj_" + inx;
			objlist.Add(newObj);
			GameObject fname = GetChildWithName(objlist[inx], "filename");
			if (fname!=null)
			{
				Text txt = fname.GetComponent<Text>();
				//txt.text = Path.GetFileName(elm._filename);
				DateTime oDate = DateTime.Parse(elm._dateCreated);
				DateTime nDate = oDate.ToLocalTime();
				txt.text = "["+elm._userid + "] " + nDate.ToString("yyyy'-'MM'-'dd HH':'mm':'ss");
			}
			GameObject percent = GetChildWithName(objlist[inx], "percent");
			if (percent!=null)
            {
				Text txt2 = percent.GetComponent<Text>();
				txt2.text = "";
			}
		}
	}

	GameObject GetChildWithName(GameObject obj, string name)
	{
		Transform trans = obj.transform;
		Transform childTrans = trans.Find(name);
		if (childTrans != null)
		{
			return childTrans.gameObject;
		}
		else
		{
			return null;
		}
	}
}