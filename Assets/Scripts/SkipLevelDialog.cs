using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 「跳过本关」选择弹窗：看广告免费跳过 / 花金币直接跳过。
///
/// 做法：直接复用 Resources/AdConfirm 预制体的外观（面板、两个按钮、金币图标都是现成的），
/// 运行时把 AdConfirm 自己的「看广告领金币」脚本摘掉，只改文案和按钮行为。
/// 好处：不需要新增美术资源，也不会影响启动页那个「看广告领金币」弹窗。
/// </summary>
public class SkipLevelDialog : MonoBehaviour
{
    private static SkipLevelDialog instance;

    private Button btnAd;
    private Button btnCoin;
    private Button btnBack;

    private Text titleText;    // panel/award/tips (1)
    private Text valueText;    // panel/award/value
    private Text descText;     // panel/award/tips

    private string defaultDesc;
    private Color defaultDescColor = Color.white;
    private Coroutine hintCoroutine;

    public static bool IsOpen
    {
        get { return instance != null && instance.gameObject.activeSelf; }
    }

    /// <summary>弹出选择框</summary>
    public static void Show()
    {
        if (instance == null)
        {
            Build();
        }

        if (instance == null)
        {
            return;
        }

        instance.gameObject.SetActive(true);
        instance.transform.SetAsLastSibling();
        instance.Refresh();
    }

    private static void Build()
    {
        GameObject prefab = Resources.Load<GameObject>("AdConfirm");
        if (prefab == null)
        {
            Debug.LogError("[跳过本关] 找不到 Resources/AdConfirm 预制体，无法弹出选择框");
            return;
        }

        GameObject go = null;
        AdConfirm.SuppressAutoInit = true;
        try
        {
            go = Instantiate(prefab);
        }
        finally
        {
            AdConfirm.SuppressAutoInit = false;
        }

        if (go == null)
        {
            return;
        }

        go.name = "SkipLevelDialog";

        // AdConfirm 自带「看广告发金币」逻辑，这里只借它的皮，把脚本摘掉
        AdConfirm confirm = go.GetComponent<AdConfirm>();
        if (confirm != null)
        {
            Destroy(confirm);
        }

        instance = go.AddComponent<SkipLevelDialog>();
        instance.Init();
    }

    private void Init()
    {
        btnAd = FindButton("panel/btn_confirm");
        btnBack = FindButton("panel/btn_close");
        titleText = FindText("panel/award/tips (1)");
        valueText = FindText("panel/award/value");
        descText = FindText("panel/award/tips");

        // 清掉 AdConfirm.Awake 里注册的监听
        ClearListeners(btnAd);
        ClearListeners(btnBack);

        // 复制「确认」按钮，当作金币按钮
        if (btnAd != null)
        {
            GameObject coinGo = Instantiate(btnAd.gameObject, btnAd.transform.parent);
            coinGo.name = "btn_coin";
            btnCoin = coinGo.GetComponent<Button>();
            ClearListeners(btnCoin);
        }

        LayoutButton(btnBack, -210f, "返回", 38);
        LayoutButton(btnCoin, 0f, GameEconomy.SkipLevelCost + "金币", 32);
        LayoutButton(btnAd, 210f, "看广告", 36);

        if (titleText != null)
        {
            titleText.text = "跳过本关";
        }

        if (descText != null)
        {
            descText.fontSize = 34;
            defaultDescColor = descText.color;
        }

        if (btnAd != null)
        {
            btnAd.onClick.AddListener(OnClickAd);
        }

        if (btnCoin != null)
        {
            btnCoin.onClick.AddListener(OnClickCoin);
        }

        if (btnBack != null)
        {
            btnBack.onClick.AddListener(Close);
        }
    }

    private void Refresh()
    {
        if (valueText != null)
        {
            valueText.text = GameEconomy.SkipLevelCost.ToString();
        }

        if (descText != null)
        {
            descText.color = defaultDescColor;
            descText.text = "花 " + GameEconomy.SkipLevelCost + " 金币跳过本关";
            defaultDesc = descText.text;
        }
    }

    private void OnClickCoin()
    {
        if (GameEconomy.TrySpend(GameEconomy.SkipLevelCost))
        {
            Close();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SkipLevel();
            }

            return;
        }

        ShowHint("金币不足，看广告可以免费跳过");
    }

    private void OnClickAd()
    {
        Close();

        Ads.Instance.PlayRewardedVideo(
            () =>
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SkipLevel();
                }
            },
            error =>
            {
                Debug.LogWarning("[跳过本关] 广告没有正常播放完成：" + error);
                Show();
                ShowHint("广告没看完或加载失败，再试一次吧");
            });
    }

    private void Close()
    {
        gameObject.SetActive(false);
    }

    private void ShowHint(string message)
    {
        if (descText == null)
        {
            return;
        }

        if (hintCoroutine != null)
        {
            StopCoroutine(hintCoroutine);
        }

        hintCoroutine = StartCoroutine(HintRoutine(message));
    }

    private IEnumerator HintRoutine(string message)
    {
        descText.text = message;
        descText.color = new Color(1f, 0.36f, 0.36f, 1f);

        yield return new WaitForSecondsRealtime(1.8f);

        descText.text = defaultDesc;
        descText.color = defaultDescColor;
        hintCoroutine = null;
    }

    #region 工具

    private Button FindButton(string path)
    {
        Transform t = transform.Find(path);
        if (t == null)
        {
            Debug.LogWarning("[跳过本关] 没找到按钮：" + path);
            return null;
        }

        return t.GetComponent<Button>();
    }

    private Text FindText(string path)
    {
        Transform t = transform.Find(path);
        if (t == null)
        {
            Debug.LogWarning("[跳过本关] 没找到文本：" + path);
            return null;
        }

        return t.GetComponent<Text>();
    }

    private static void ClearListeners(Button button)
    {
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
        }
    }

    private static void LayoutButton(Button button, float x, string label, int fontSize)
    {
        if (button == null)
        {
            return;
        }

        RectTransform rect = button.transform as RectTransform;
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x, 120f);
            rect.sizeDelta = new Vector2(190f, 100f);
        }

        // AdConfirm 原按钮左侧挂着一个 267x200 的装饰图标（视频 icon），
        // 按钮宽度收窄到 190 后它会盖在文字上，这里直接隐藏掉，文字才能完整居中。
        Transform icon = button.transform.Find("icon");
        if (icon != null)
        {
            icon.gameObject.SetActive(false);
        }

        Text text = button.GetComponentInChildren<Text>(true);
        if (text != null)
        {
            text.text = label;
            text.fontSize = fontSize;

            // 文字原本为了避开左侧图标而向右偏了 17.8，图标隐藏后归零，重新铺满居中
            RectTransform textRect = text.transform as RectTransform;
            if (textRect != null)
            {
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
                textRect.anchoredPosition = Vector2.zero;
                textRect.sizeDelta = Vector2.zero;
            }
        }
    }

    #endregion
}
