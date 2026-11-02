using UnityEngine;
using UnityEngine.UI;

public class BrushController : MonoBehaviour
{
    // 以下を参照する
    public PaintCanvas PaintCanvas;
    public LeftPanelController LeftPanel;
    public Slider BrushSizeSlider;

    // 筆の設定を開く
    public void OpenBrushPanel()
    {
        // 筆を選択する
        PaintCanvas.SelectBrush();

        // 筆パネルを開く
        LeftPanel.OpenBrush();

        // 保存してある筆の太さを表示
        BrushSizeSlider.SetValueWithoutNotify(PaintCanvas.BrushSize);
    }

    // 筆の太さを変更
    public void SetBrushSize(float value)
    {
        PaintCanvas.SetBrushSize(value);
    }

    // 色の設定
    public void SetRed(float value)
    {
        PaintCanvas.SetRed(value);
    }

    public void SetGreen(float value)
    {
        PaintCanvas.SetGreen(value);
    }

    public void SetBlue(float value)
    {
        PaintCanvas.SetBlue(value);
    }

    public void SetAlpha(float value)
    {
        PaintCanvas.SetAlpha(value);
    }
}
