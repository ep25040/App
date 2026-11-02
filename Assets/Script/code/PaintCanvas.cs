using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PaintCanvas : MonoBehaviour
{
    // 描画エリアの設定
    public RawImage DrawingArea;

    // 筆の設定
    public int BrushSize = 8;
    public Color BrushColor = Color.black;
    public bool isEraser = false;

    // 消しゴムの設定
    public int EraserSize = 30;

    // 選んでいる色を表示
    public Image ColorPreview;

    // RGBA を表示するテキスト
    public TMP_Text RGBA_Text;

    // 描いた絵を保存する箱
    private Texture2D texture;

    // 1 つ 1 つのピクセルの色を保存する箱
    private Color32[] pixels;

    // 描画エリアの大きさ
    private int width = 1000;
    private int height = 800;

    // マウスを押しているかどうか判定
    private bool isDrawing = false;

    // マウスの場所
    private Vector2Int lastPoint;

    // Undo, Redo の箱
    private Stack<Color32[]> UndoStack = new Stack<Color32[]>();
    private Stack<Color32[]> RedoStack = new Stack<Color32[]>();

    private int MaxHistory = 30;

    void Start()
    {
        // コード内で描画エリアのサイズを固定
        width = 1000;
        height = 800;

        // テクスチャを作成する
        texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        // 画像を表示する
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        // 配列を作る
        pixels = new Color32[width * height];

        // 透明な色を塗る
        ClearPixels();

        // 作ったテクスチャを描画エリアに表示
        DrawingArea.texture = texture;
        DrawingArea.color = Color.white;

        // テクスチャを更新
        RefreshTexture();

        // 色の見本を更新する
        UpdateColorPreview();
    }


    void Update()
    {
        // マウスを押したとき
        if (Input.GetMouseButtonDown(0))
        {
            Vector2Int point;

            // マウスが描画エリアの中にあるか確認
            if (TryGetPoint(Input.mousePosition, out point))
            {
                // 描く前の状態を保存
                SaveUndoState();

                // Redo の履歴を消す
                RedoStack.Clear();

                // 描画中にする
                isDrawing = true;

                // 最初の点を保存
                lastPoint = point;

                // 点を描く
                DrawLine(point, point);

                // 画面を更新
                RefreshTexture();
            }
        }

        // マウスを押している間
        if (isDrawing && Input.GetMouseButton(0))
        {
            Vector2Int point;

            // マウスが描画エリア内にあるか確認
            if (TryGetPoint(Input.mousePosition, out point))
            {
                // 前の点と今の点を線でつなぐ
                DrawLine(lastPoint, point);

                // 今の点を次の処理のために保存
                lastPoint = point;

                // 画面を更新する
                RefreshTexture();
            }
        }

        // マウスを離したとき
        if (Input.GetMouseButtonUp(0))
            isDrawing = false;
    }


    // マウスの位置をピクセルの位置に変換
    bool TryGetPoint(Vector2 mousePosition, out Vector2Int point)
    {
        // 座標の初期化
        point = Vector2Int.zero;
        Vector2 localPoint;

        // マウスの位置を描画エリア内の座標に変換
        bool result = RectTransformUtility.ScreenPointToLocalPointInRectangle( DrawingArea.rectTransform, mousePosition, null, out localPoint);

        // 描画エリア内かどうか
        if (result == false)
            return false;

        Rect rect = DrawingArea.rectTransform.rect;

        // 描画エリアの外なら不可
        if (rect.Contains(localPoint) == false)
            return false;

        // マウスの位置を変換
        float x = (localPoint.x - rect.xMin) / rect.width;
        float y = (localPoint.y - rect.yMin) / rect.height;

        // ピクセルの位置に変換
        int pixelX = (int)(x * width);
        int pixelY = (int)(y * height);

        // 描画エリアの範囲内に収める
        pixelX = Mathf.Clamp(pixelX, 0, width - 1);
        pixelY = Mathf.Clamp(pixelY, 0, height - 1);

        point = new Vector2Int(pixelX, pixelY);

        return true;
    }


    // 2 つの点を線でつなぐ
    void DrawLine(Vector2Int startPoint, Vector2Int endPoint)
    {
        int distanceX = Mathf.Abs(endPoint.x - startPoint.x);
        int distanceY = Mathf.Abs(endPoint.y - startPoint.y);

        // 横と縦のうち、大きい方を使って線を描く
        int steps = Mathf.Max(distanceX, distanceY, 1);

        // 点を描く
        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;

            int x = Mathf.RoundToInt(Mathf.Lerp(startPoint.x, endPoint.x, t));

            int y = Mathf.RoundToInt(Mathf.Lerp(startPoint.y, endPoint.y, t));

            // 線の上に筆を置く
            DrawBrush(x, y);
        }
    }


    // 指定した場所に筆を置く
    void DrawBrush(int centerX, int centerY)
        {
        int currentSize;

        // 状態が消しゴムか筆かを確認する
        if (isEraser)
        {
            // 消しゴムの太さを使う
            currentSize = EraserSize;
        }
        else
        {
            // 筆の太さを使う
            currentSize = BrushSize;
        }

        // 半径を計算する
        int radius = Mathf.Max(1, currentSize) / 2;

        Color32 color;

        // 消しゴムなら透明色にする
        if (isEraser)
        {
            color = new Color32(0, 0, 0, 0);
        }
        else
        {
            color = (Color32)BrushColor;
        }

        // 筆の範囲を調べる
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                // 円の外側には描かない
                if (x * x + y * y > radius * radius)
                {
                    continue;
                }

                int pixelX = centerX + x;
                int pixelY = centerY + y;

                // 描画エリアの外には描かない
                if (pixelX < 0 || pixelX >= width || pixelY < 0 || pixelY >= height)
                    continue;

                // ピクセルの色を変更する
                pixels[pixelY * width + pixelX] = color;
            }
        }
    }


    // Undo用に現在の絵を保存する
    void SaveUndoState()
    {
        // 配列の複製
        Color32[] savePixels = (Color32[])pixels.Clone();
        UndoStack.Push(savePixels);

        // 履歴が 30 個を超えないようにする
        while (UndoStack.Count > MaxHistory)
        {
            Color32[][] history = UndoStack.ToArray();

            UndoStack.Clear();

            for (int i = history.Length - 2; i >= 0; i--)
            {
                UndoStack.Push(history[i]);
            }
        }
    }


    // 1 つ前の状態に戻す
    public void Undo()
    {
        // 空なら何もしない
        if (UndoStack.Count == 0)
            return;

        // 現在の絵を Redo 用に保存する
        RedoStack.Push((Color32[])pixels.Clone());

        // 1 つ前の絵を取り出す
        pixels = UndoStack.Pop();

        // 画面を更新する
        RefreshTexture();
    }


    // Undo した操作をやり直す
    public void Redo()
    {
        if (RedoStack.Count == 0)
            return;

        // 現在の絵を Undo 用に保存する
        UndoStack.Push((Color32[])pixels.Clone());

        // やり直す絵を取り出す
        pixels = RedoStack.Pop();

        // 画面を更新する
        RefreshTexture();
    }

    // 筆を選択
    public void SelectBrush()
    {
        isEraser = false;
    }

    // 消しゴムを選択
    public void SelectEraser()
    {
        isEraser = true;
    }

    // 筆の太さを変更
    public void SetBrushSize(float value)
    {
        BrushSize = Mathf.Clamp(Mathf.RoundToInt(value), 2, 100);
    }

    // 消しゴムの太さを変更
    public void SetEraserSize(float value)
    {
        EraserSize = Mathf.Clamp(Mathf.RoundToInt(value), 2, 100);
    }

    // 赤色の値を変更
    public void SetRed(float value)
    {
        BrushColor.r = Mathf.RoundToInt(value) / 255f;
        UpdateColorPreview();
    }


    // 緑色の値を変更
    public void SetGreen(float value)
    {
        BrushColor.g = Mathf.RoundToInt(value) / 255f;
        UpdateColorPreview();
    }


    // 青色の値を変更
    public void SetBlue(float value)
    {
        BrushColor.b = Mathf.RoundToInt(value) / 255f;
        UpdateColorPreview();
    }

    // アルファ値を変更
    public void SetAlpha(float value)
    {
        BrushColor.a = Mathf.RoundToInt(value) / 255f;
        UpdateColorPreview();
    }

    // 選んでいる色を表示する
    void UpdateColorPreview()
    {
        ColorPreview.color = BrushColor;

        if (RGBA_Text != null)
        {
            Color32 color = BrushColor;

            RGBA_Text.text = $"R : {color.r}\nG : {color.g}\nB : {color.b}\n";
        }
    }


    // すべてのピクセルを透明にする
    void ClearPixels()
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = new Color32(0, 0, 0, 0);
        }
    }


    // ピクセルの情報をテクスチャに反映する
    void RefreshTexture()
    {
        texture.SetPixels32(pixels);
        texture.Apply(false);
    }


    // スクリプトが破棄されたときに実行する
    void OnDestroy()
    {
        if (texture != null)
        {
            Destroy(texture);
        }
    }
}
