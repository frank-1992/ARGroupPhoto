using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{

    public static CameraManager _inst;

    [SerializeField]
    private Camera AR_Camera;

    [SerializeField]
    private Camera UI_Camera;


    private void Awake()
    {
        _inst = this;
    }

    public void CloseARCamera()
    {

        AR_Camera.gameObject.SetActive(false);
        UI_Camera.gameObject.SetActive(true);
    }

    public void CloseUICamera()
    {
        UI_Camera.gameObject.SetActive(false);
        AR_Camera.gameObject.SetActive(true);
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
