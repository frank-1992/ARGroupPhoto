using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ARSceneClick : MonoBehaviour
{
    public void GoScene()
    {
        //Debug.Log("Hello, this is hello world");
        SceneManager.LoadScene("ImageTracking");
    }
}
