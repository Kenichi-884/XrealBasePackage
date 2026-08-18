using UnityEngine.Events;

namespace XrealBase.AR
{
    /// <summary>
    /// ARマーカー検出でコンテンツを表示するコンポーネントが実装するインターフェース。
    /// MarkerEventRelay はこのインターフェース経由でのみ検出元と結合する。
    /// </summary>
    public interface IMarkerContentSource
    {
        /// <summary>マーカー検出時にコンテンツが表示されたときに発火するイベント。引数: マーカー名, 表示されたインスタンス。</summary>
        UnityEvent<string, UnityEngine.GameObject> OnContentShown { get; }
    }
}
