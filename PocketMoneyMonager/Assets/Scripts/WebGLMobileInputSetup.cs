using UnityEngine;

/// <summary> WebGLモバイル入力の初期設定 </summary>
public static class WebGLMobileInputSetup
{
    /// <summary> 起動時にWebGLモバイル入力を設定する </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (SystemInfo.deviceType == DeviceType.Handheld)
        {
            WebGLInput.mobileKeyboardSupport = false;
        }
#endif
    }
}
