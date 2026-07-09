using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary> WebGL向けIndexedDBファイルシステム同期 </summary>
public static class WebGLFileSystemSync
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    static extern void JS_FileSystem_SyncPush();

    [DllImport("__Internal")]
    static extern void JS_FileSystem_SyncPull(string gameObjectName);
#endif

    /// <summary> メモリ上の変更をIndexedDBへ同期する </summary>
    public static void SyncPush()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        JS_FileSystem_SyncPush();
#endif
    }

    /// <summary> IndexedDBからメモリへ同期を開始する </summary>
    public static void SyncPull(string gameObjectName)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        JS_FileSystem_SyncPull(gameObjectName);
#endif
    }

    /// <summary> IndexedDB同期完了を待つ </summary>
    public static IEnumerator WaitForPullCompleted()
    {
        while (!WebGLFileSystemSyncRunner.IsPullCompleted)
        {
            yield return null;
        }
    }
}

/// <summary> WebGL起動時のIndexedDB読み込みを行う </summary>
public sealed class WebGLFileSystemSyncRunner : MonoBehaviour
{
    const string RunnerObjectName = "WebGLFileSystemSyncRunner";

    /// <summary> IndexedDBからの読み込みが完了したか </summary>
    public static bool IsPullCompleted { get; private set; } = true;

    /// <summary> 起動時に同期ランナーを生成する </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void CreateRunner()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        IsPullCompleted = false;
        var runnerObject = new GameObject(RunnerObjectName);
        runnerObject.AddComponent<WebGLFileSystemSyncRunner>();
        DontDestroyOnLoad(runnerObject);
#endif
    }

    /// <summary> IndexedDBからメモリへ同期を開始する </summary>
    void Start() => WebGLFileSystemSync.SyncPull(RunnerObjectName);

    /// <summary> アプリ終了時にIndexedDBへ同期する </summary>
    void OnApplicationQuit() => WebGLFileSystemSync.SyncPush();

    /// <summary> バックグラウンド移行時にIndexedDBへ同期する </summary>
    void OnApplicationPause(bool isPaused)
    {
        if (isPaused)
        {
            WebGLFileSystemSync.SyncPush();
        }
    }

    /// <summary> IndexedDBからの読み込み完了時のコールバック </summary>
    public void OnPullCompleted() => IsPullCompleted = true;
}
