using UnityEngine;
using TMPro;

public class LeftPanelController : MonoBehaviour
{
    // 筆パネルの設定
    public GameObject BrushPanel;
    public TMP_Text BrushTitleText;

    // 消しゴムパネルの設定
    public GameObject EraserPanel;
    public TMP_Text EraserTitleText;

    // 状態の設定
    public GameObject BrushOverlay;
    public GameObject EraserOverlay;

    // 開始時
    private void Start()
    {
        // 筆パネルを表示し、消しゴムパネルを非表示にする
        BrushPanel.SetActive(true);
        EraserPanel.SetActive(false);

        // 筆を選択した状態にする
        UpdateButtonOverlay(true);
    }

    // 筆パネルを開く
    public void OpenBrush()
    {
        // 筆パネルを表示し、消しゴムパネルを非表示にする
        BrushPanel.SetActive(true);
        EraserPanel.SetActive(false);

        UpdateButtonOverlay(true);
    }

    // 消しゴムパネルを開く
    public void OpenEraser()
    {
        // 消しゴムパネルを表示、筆パネルを非表示にする
        EraserPanel.SetActive(true);
        BrushPanel.SetActive(false);

        UpdateButtonOverlay(false);
    }

    // 両方のパネルを閉じる
    public void Close()
    {
        BrushPanel.SetActive(false);
        EraserPanel.SetActive(false);
    }

    // 状態を更新する
    private void UpdateButtonOverlay(bool BrushSelected)
    {
        BrushOverlay.SetActive(!BrushSelected);

        EraserOverlay.SetActive(BrushSelected);
    }
}
