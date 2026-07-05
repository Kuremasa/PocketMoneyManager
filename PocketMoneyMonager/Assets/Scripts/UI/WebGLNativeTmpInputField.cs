using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary> WebGLモバイル向けTMP入力ブリッジ </summary>
[RequireComponent(typeof(TMP_InputField))]
public class WebGLNativeTmpInputField : MonoBehaviour
{
    const string DialogOkButtonText = "OK";
    const string DialogCancelButtonText = "キャンセル";
    const string DefaultDialogTitle = "入力";

    [SerializeField] TMP_InputField _inputField;
    [SerializeField] string _dialogTitle = DefaultDialogTitle;

    /// <summary> 初期化処理 </summary>
    void Awake()
    {
        if (_inputField == null)
        {
            _inputField = GetComponent<TMP_InputField>();
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        if (IsMobileWebGL())
        {
            _inputField.onSelect.AddListener(OnInputSelected);
        }
#endif
    }

    /// <summary> 破棄時の処理 </summary>
    void OnDestroy()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (_inputField != null)
        {
            _inputField.onSelect.RemoveListener(OnInputSelected);
        }
#endif
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    /// <summary> 入力選択時の処理 </summary>
    void OnInputSelected(string currentText)
    {
        if (IsIosWebGL())
        {
            ShowPromptDialog(currentText);
        }
        else
        {
            StartCoroutine(ShowOverlayDialogCoroutine(currentText));
        }
    }

    /// <summary> iOS向けプロンプトダイアログを表示する </summary>
    void ShowPromptDialog(string currentText)
    {
        _inputField.text = WebNativeDialog.OpenNativeStringDialog(_dialogTitle, currentText);
        DeactivateInput();
    }

    /// <summary> Android向けオーバーレイダイアログを表示する </summary>
    IEnumerator ShowOverlayDialogCoroutine(string currentText)
    {
        WebNativeDialog.SetUpOverlayDialog(_dialogTitle, currentText, DialogOkButtonText, DialogCancelButtonText);
        yield return DeactivateInputCoroutine();
        WebGLInput.captureAllKeyboardInput = false;
        while (WebNativeDialog.IsOverlayDialogActive())
        {
            yield return null;
        }

        WebGLInput.captureAllKeyboardInput = true;
        if (!WebNativeDialog.IsOverlayDialogCanceled())
        {
            _inputField.text = WebNativeDialog.GetOverlayDialogValue();
        }
    }

    /// <summary> 入力フォーカスを解除する </summary>
    void DeactivateInput()
    {
        _inputField.DeactivateInputField();
        EventSystem.current.SetSelectedGameObject(null);
    }

    /// <summary> フレーム終了後に入力フォーカスを解除する </summary>
    IEnumerator DeactivateInputCoroutine()
    {
        yield return new WaitForEndOfFrame();
        DeactivateInput();
    }

    /// <summary> モバイルWebGLかどうかを返す </summary>
    static bool IsMobileWebGL() => SystemInfo.deviceType == DeviceType.Handheld;

    /// <summary> iOS WebGLかどうかを返す </summary>
    static bool IsIosWebGL()
    {
        var operatingSystem = SystemInfo.operatingSystem;
        return operatingSystem.Contains("iPhone")
            || operatingSystem.Contains("iPad")
            || operatingSystem.Contains("iOS");
    }
#endif
}
