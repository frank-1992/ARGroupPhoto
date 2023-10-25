using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ModelListGrid : MonoBehaviour
{
	public Button prefab; // This is our prefab object that will be exposed in the inspector

	private int numberToCreate; // number of objects to create. Exposed in inspector

	private PatchData patlist = null;
	private List<Button> btnlist = null;

	void Start()
	{
		btnlist = new List<Button>();
		Populate();
	}

	void Update()
	{

	}

	void TaskOnClick(int i)
	{
		Debug.Log("TaskOnClick: i=" + i);

		PatchDataElement elm = patlist.models[i];
		string downloadfile = Path.GetFileName(elm._name);
		SetPick(i);

		PlayerPrefs.SetString("selectModel", downloadfile);
		
		GameObject mvx_model = GameObject.Find("AR Session Origin");
		if (mvx_model != null)
		{
			TrackedImageInfoManager trackedImageInfoManager = mvx_model.GetComponent<TrackedImageInfoManager>();
            if (trackedImageInfoManager != null)
            {
				Debug.Log("TaskOnClick:ChangeModel=" + downloadfile);
				trackedImageInfoManager.ChangeModel(downloadfile);
			}
		}
		else
		{
			Debug.Log("TaskOnClick:MeshTextured not found");
		}
	}

	void Populate()
	{
		//clear the select model
		//PlayerPrefs.SetString("selectModel", "");
		string selectModel = PlayerPrefs.GetString("selectModel");

		string responseText = PlayerPrefs.GetString("patchlist");
		if (responseText==null)
        {

        }
		else if (responseText.Length==0)
        {

        }
		else
        {
			patlist = PatchData.CreateFromJSON(responseText);

			for (int i=0; i< patlist.models.Count; i++)
            {
				PatchDataElement elm = patlist.models[i];
				int inx = i;
				// Create new instances of our prefab until we've created as many as we specified
				Button newObj = (Button)Instantiate(prefab, transform);
				newObj.name = "modellist_btn_" + inx;
                //newObj.GetComponent<Image>().sprite = Image1;
                string fpath = Path.Combine(Application.persistentDataPath, Path.GetFileName(elm._icon));
				newObj.image.sprite = LoadSprite(fpath);
				btnlist.Add(newObj);
				btnlist[i].onClick.AddListener(delegate { TaskOnClick(inx); });

				string downloadfile = Path.GetFileName(elm._name);
				GameObject redframe = GetChildWithName(newObj.gameObject, "redframe");
				if (redframe != null)
				{
					if (selectModel.Equals(downloadfile))
                    {
						redframe.SetActive(true);
					}
                    else
                    {
						redframe.SetActive(false);
					}
				}
			}
		}
	}

	void SetPick(int index)
    {
		for(int i=0; i<btnlist.Count; i++)
        {
			GameObject redframe = GetChildWithName(btnlist[i].gameObject, "redframe");
			if (redframe != null)
			{
				redframe.SetActive(false);
				if (i == index)
				{
					redframe.SetActive(true);
				}
			}
        }
    }

	private Sprite LoadSprite(string path)
	{
		if (string.IsNullOrEmpty(path)) return null;
		if (System.IO.File.Exists(path))
		{
			byte[] bytes = System.IO.File.ReadAllBytes(path);
			Texture2D texture = new Texture2D(1, 1);
			texture.LoadImage(bytes);
			Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
			return sprite;
		}
		return null;
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