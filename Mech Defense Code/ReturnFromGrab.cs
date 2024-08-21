using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ML.SDK;

public class ReturnFromGrab : MonoBehaviour
{
    public Transform OriginalPosition;
    public MLGrab grabobj;

    void OnPrimaryTriggerDown_()
    {


    }

    void OnPrimaryTriggerUp_()
    {


    }


    void OnPrimaryGrabBegin_()
    {


    }

    void OnPrimaryGrabEnd_()
    {
        transform.position = OriginalPosition.position;
        transform.rotation = OriginalPosition.rotation;

    }


    // Start is called before the first frame update
    void Start()
    {

        if (grabobj != null)
        {
            Debug.Log("Grab Non null adding events");
            grabobj.OnPrimaryTriggerDown.AddListener(OnPrimaryTriggerDown_);
            grabobj.OnPrimaryTriggerUp.AddListener(OnPrimaryTriggerUp_);
            grabobj.OnPrimaryGrabBegin.AddListener(OnPrimaryGrabBegin_);
            grabobj.OnPrimaryGrabEnd.AddListener(OnPrimaryGrabEnd_);

        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
