using UnityEngine;
using UnityEngine.UI;

/// <summary> メインView </summary>
public class MainView : MonoBehaviour
{
    [SerializeField] Button _button;

    /// <summary> 初期化処理 </summary>
    void Start() => _button.onClick.AddListener(OnButtonClicked);

    /// <summary> ボタンクリック時の処理 </summary>
    void OnButtonClicked() => Debug.Log("Button clicked");

    /// <summary> 破棄時の処理 </summary>
    void OnDestroy() => _button.onClick.RemoveListener(OnButtonClicked);
}
