using UnityEngine;
using UnityEngine.UI;

public class EraserController : MonoBehaviour
{
    // 以下を参照する
    public PaintCanvas PaintCanvas;
    public LeftPanelController LeftPanel;
    public Slider EraserSizeSlider;

    // 消しゴムの設定を開く
    public void OpenEraserPanel()
    {
        // 消しゴムを選択する
        PaintCanvas.SelectEraser();

        // 消しゴムパネルを開く
        LeftPanel.OpenEraser();

        // 保存してある消しゴムの太さを表示
        EraserSizeSlider.SetValueWithoutNotify(PaintCanvas.EraserSize);
    }

    // 消しゴムの太さを変更
    public void SetEraserSize(float value)
    {
        PaintCanvas.SetEraserSize(value);
    }
}
