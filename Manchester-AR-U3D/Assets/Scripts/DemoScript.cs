using UnityEngine;
using System.Collections;

public class DemoScript : MonoBehaviour {
    //name of the scene you want to load
    //public string scene;
	public Color loadToColor = Color.white;
    public float multiplier = 1.0f;
	
    /*
	public void GoFade()
    {
        //Initiate.Fade(scene, loadToColor, multiplier);
    }
    */
    public void GoCoverScene()
    {
        string scene = "cover";
        Initiate.Fade(scene, loadToColor, multiplier);
    }

    public void GoQRCodeScene()
    {
        string scene = "qrcode";
        PlayerPrefs.SetString("login_mode", "1");//qrcode input
        Initiate.Fade(scene, loadToColor, multiplier);
    }

    public void GoManualInputScene()
    {
        string scene = "qrcode";
        PlayerPrefs.SetString("login_mode", "2");//manual input
        Initiate.Fade(scene, loadToColor, multiplier);
    }

    public void BackPreviousScene()
    {
        string scene = "qrcode";
        Initiate.Fade(scene, loadToColor, multiplier);
    }
    
    public void GoPlaybackScene()
    {
        string scene = "playback";
        Initiate.Fade(scene, loadToColor, multiplier);
    }

    public void GoARScene()
    {
        string scene = "ImageTracking";
        Initiate.Fade(scene, loadToColor, multiplier);
    }

    
}
