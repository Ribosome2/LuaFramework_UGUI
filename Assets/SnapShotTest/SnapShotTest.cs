using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using RemoteCodeControl;
using UnityEngine;

public class SnapShotTest : MonoBehaviour {

    [DllImport("snapshot", CallingConvention = CallingConvention.Cdecl)]
    public static extern int luaopen_snapshot(IntPtr L);
    [DllImport("snapshot", CallingConvention = CallingConvention.Cdecl)]
    public static extern int justTomeTest(IntPtr L);
	// Use this for initialization
	void Start () {
		
	}
	[ContextMenu("JustTest")]
    void JustTest()
    {
		Debug.Log("Test "+justTomeTest(IntPtr.Zero));
    }

    [ContextMenu("OpenSnapShot")]
    void OpenSnapShot()
    {
        Debug.Log("OpenSnapShot " + luaopen_snapshot(LuaHandleInterface.GetLuaPtr()));
    }

	// Update is called once per frame
	void Update () {
		
	}
}
