using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 游戏内常显的金币数量。
///
/// 背景：原来唯一的金币数字挂在「过关结算 UI」(LevelCompletedUI) 里，
/// 而 GameManager 平时会把这个 UI SetActive(false)，所以游戏过程中玩家根本看不到金币。
/// 这里在常驻 Canvas 上单独建一条金币 HUD，美术直接复用工程里已有的两张图
/// （CoinHolderCorner 白胶囊、Coin 金币），不新增任何资源。
/// </summary>
public class CoinHud : MonoBehaviour
{
    private static CoinHud instance;

    // 尺寸与位置：和「看广告领奖励」「跳过本关」排在同一水平线上，居中不遮挡
    private const float PanelWidth = 206f;
    private const float PanelHeight = 64f;
    private const float OffsetX = 100f;    // 相对屏幕顶边中心向右
    private const float OffsetY = -124f;   // 相对屏幕顶边向下

    private Text valueText;
    private GameManager manager;
    private int lastCoins = int.MinValue;

    public static void Ensure(GameObject context)
    {
        if (instance != null || context == null)
        {
            return;
        }

        Canvas canvas = FindHudCanvas(context);
        if (canvas == null)
        {
            Debug.LogWarning("[金币HUD] 找不到 Canvas，金币显示未创建");
            return;
        }

        GameObject go = new GameObject("CoinHud", typeof(RectTransform));
        go.layer = canvas.gameObject.layer;
        go.transform.SetParent(canvas.transform, false);

        instance = go.AddComponent<CoinHud>();
        instance.Build();
    }

    /// <summary>
    /// UIManager 自己是挂在 MANAGERS_AND_UI 根节点下的，根节点不在任何 Canvas 里，
    /// 所以不能直接往上找。改用「跳过本关」按钮所在的 Canvas（就是游戏 HUD 那一层），
    /// 再退化为场景里第一个可见的 Overlay Canvas。
    /// </summary>
    private static Canvas FindHudCanvas(GameObject context)
    {
        Canvas canvas = context.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            return canvas;
        }

        GameObject skipBtn = GameObject.Find("SkipLevelBtn");
        if (skipBtn != null)
        {
            canvas = skipBtn.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                return canvas;
            }
        }

        foreach (Canvas c in Object.FindObjectsOfType<Canvas>())
        {
            if (c.isActiveAndEnabled && c.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return c;
            }
        }

        return null;
    }

    private void Build()
    {
        Sprite plateSprite = null;
        Sprite coinSprite = null;
        Sprite fallback = null;

        // 从工程里现成的 UI 图片上取 sprite，避免新增美术资源
        foreach (Image img in Resources.FindObjectsOfTypeAll<Image>())
        {
            if (img == null || img.sprite == null)
            {
                continue;
            }

            string n = img.gameObject.name;

            if (plateSprite == null && n == "CoinHolderCorner")
            {
                plateSprite = img.sprite;
            }
            else if (coinSprite == null && n == "Coin")
            {
                coinSprite = img.sprite;
            }

            if (fallback == null)
            {
                fallback = img.sprite;
            }
        }

        if (plateSprite == null)
        {
            plateSprite = fallback;
        }

        RectTransform self = transform as RectTransform;
        self.anchorMin = new Vector2(0.5f, 1f);
        self.anchorMax = new Vector2(0.5f, 1f);
        self.pivot = new Vector2(0.5f, 0.5f);
        self.anchoredPosition = new Vector2(OffsetX, OffsetY);
        self.sizeDelta = new Vector2(PanelWidth, PanelHeight);

        Image bg = gameObject.AddComponent<Image>();
        bg.sprite = plateSprite;
        bg.type = Image.Type.Simple;
        bg.color = Color.white;
        bg.raycastTarget = false;

        if (coinSprite != null)
        {
            GameObject iconGo = new GameObject("CoinIcon", typeof(RectTransform));
            iconGo.layer = gameObject.layer;

            RectTransform iconRect = iconGo.transform as RectTransform;
            iconRect.SetParent(self, false);
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = new Vector2(44f, 2f);
            iconRect.sizeDelta = new Vector2(58f, 58f);

            Image icon = iconGo.AddComponent<Image>();
            icon.sprite = coinSprite;
            icon.color = Color.white;
            icon.raycastTarget = false;
        }

        GameObject textGo = new GameObject("CoinValue", typeof(RectTransform));
        textGo.layer = gameObject.layer;

        RectTransform textRect = textGo.transform as RectTransform;
        textRect.SetParent(self, false);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(80f, 0f);
        textRect.offsetMax = new Vector2(-10f, 0f);

        // 这里只显示阿拉伯数字，用内置字体即可（不依赖中文 TMP 字库）
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        valueText = textGo.AddComponent<Text>();
        valueText.font = font;
        valueText.fontSize = 42;
        valueText.fontStyle = FontStyle.Bold;
        valueText.alignment = TextAnchor.MiddleLeft;
        valueText.color = new Color(0.36f, 0.23f, 0.11f, 1f);
        valueText.horizontalOverflow = HorizontalWrapMode.Overflow;
        valueText.verticalOverflow = VerticalWrapMode.Overflow;
        valueText.raycastTarget = false;
        valueText.text = "0";
    }

    private void Update()
    {
        if (valueText == null)
        {
            return;
        }

        if (manager == null)
        {
            manager = GameManager.Instance;
        }

        int coins = manager != null ? manager.playerCoins : PlayerPrefs.GetInt("playercoins");

        if (coins != lastCoins)
        {
            lastCoins = coins;
            valueText.text = coins.ToString();
        }
    }
}
