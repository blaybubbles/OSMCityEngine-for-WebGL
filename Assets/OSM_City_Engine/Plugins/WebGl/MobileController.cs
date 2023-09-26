using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class MobileController : Singleton<MobileController>
{
    public bool mobileDetect;

    [DllImport("__Internal")]
    private static extern bool IsMobile();

    public bool isMobile()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
             return IsMobile();
#else
        return false;
#endif
    }

    protected override void Awake()
    {

        if (isMobile())
        {
            mobileDetect = true;
        }

    }
}
