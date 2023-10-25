using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;

public class HeadImgManager : MonoBehaviour
{
    public RawImage headImage;

    private Sprite targetSprite;
    private string url = "";

    private void Start()
    {
    }

    public void doGetImage(string link)
    {
        url = link;
        StartCoroutine(GetTextureRequest(url, (response) => {
            headImage.texture = response;
        }));
    }

    IEnumerator GetTextureRequest(string url, System.Action<Texture2D> callback)
    {
        using (var www = UnityWebRequestTexture.GetTexture(url))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                if (www.isDone)
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(www);
                    //var rect = new Rect(0, 0, 128.0f, 128.0f);
                    //var sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
                    callback(texture);
                }
            }
        }
    }
}