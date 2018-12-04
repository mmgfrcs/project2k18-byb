using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaderAnimationEvent : MonoBehaviour
{
    public void OnFadeIn(int mode)
    {
        if(mode == 0) EndDayManager.ShowStartDayPanel();
        else if (mode==1)
        {

        }
    }
}

