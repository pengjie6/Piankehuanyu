using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 激励广告弹窗（抖音）
/// - 外部传入回调：看完广告后执行回调（例如「跳过本关」）
/// - 未传回调：默认按弹窗上的金币数发放奖励（例如启动页「看广告领奖励」）
/// </summary>
public class AdConfirm : MonoBehaviour
{
    /// <summary>
    /// 供「跳过本关」弹窗复用外观用：置 true 期间实例化出来的 AdConfirm 不做任何初始化
    /// （否则它的 Awake 会把单例指针抢走、还会把冲突的实例直接 Destroy 掉）
    /// </summary>
    public static bool SuppressAutoInit;

    private static AdConfirm instance;

    [SerializeField] private Button btnClose;
    [SerializeField] private Button btnConfirm;
    private Action onCompleted;

    public static void Show(Action callBack)
    {
        if (instance == null)
        {
            // 加载 AdConfirm 预制体
            GameObject prefab = Resources.Load<GameObject>("AdConfirm");
            if (prefab == null)
            {
                Debug.LogError("AdConfirm 预制体未找到，请确保其位于 Resources 文件夹中。");
                return;
            }

            // 实例化 AdConfirm
            GameObject adConfirmInstance = Instantiate(prefab);
            instance = adConfirmInstance.GetComponent<AdConfirm>();

            if (instance == null)
            {
                Debug.LogError("AdConfirm 脚本未附加到预制体上，请检查预制体设置。");
                Destroy(adConfirmInstance);
                return;
            }
        }

        // 设置回调并显示弹窗
        instance.onCompleted = callBack;
        instance.gameObject.SetActive(true);
    }

    private void Awake()
    {
        if (SuppressAutoInit)
        {
            // 只是被「跳过本关」弹窗借了外观，不要抢单例、不要挂监听
            return;
        }

        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        //gameObject.SetActive(false);
        btnClose?.onClick.AddListener(this.OnClose);
        btnConfirm?.onClick.AddListener(this.OnConfirm);

        // 弹窗上的金币数直接跟经济配置走，避免预制体里的数字和实际发放不一致
        Transform valueTf = transform.Find("panel/award/value");
        if (valueTf != null)
        {
            Text valueText = valueTf.GetComponent<Text>();
            if (valueText != null)
            {
                valueText.text = GameEconomy.RewardedVideoCoins.ToString();
            }
        }
    }


    private void OnClose()
    {
        this.gameObject.SetActive(false);
    }

    void OnConfirm()
    {
#if UNITY_EDITOR
        // 编辑器里抖音广告接口是 mock，直接当作看完
        this.GrantReward();
        this.OnClose();
#else
        if (AdManager.Instance == null)
        {
            Debug.LogError("[广告] 场景里没有 AdManager，无法播放广告");
            OnAdError("广告服务未初始化");
            return;
        }
        AdManager.Instance.ShowRewardedVideoAd(OnAdWatched, OnAdError);
#endif
    }

    private void OnAdWatched()
    {
        Debug.Log("广告观看完成");
        this.GrantReward();
        OnClose();
    }

    /// <summary>
    /// 发放奖励：有回调走回调，没有回调就按默认金币数加币
    /// </summary>
    private void GrantReward()
    {
        if (onCompleted != null)
        {
            onCompleted();
            return;
        }

        GameEconomy.AddCoins(GameEconomy.RewardedVideoCoins);

        Debug.Log($"[广告] 激励视频完成，发放金币 {GameEconomy.RewardedVideoCoins}");
    }

    private void OnAdError(string errorMessage)
    {
        Debug.LogError($"广告播放失败: {errorMessage}");
        // 加载 AdError 预制体
        GameObject adErrorPrefab = Resources.Load<GameObject>("AdError");
        if (adErrorPrefab != null)
        {
            // 实例化 AdError
            GameObject adErrorInstance = Instantiate(adErrorPrefab, transform.position, Quaternion.identity);
            // 1 秒后销毁实例
            Destroy(adErrorInstance, 1.5f);
        }
        else
        {
            Debug.LogError("AdError 预制体未找到，请确保其位于 Resources 文件夹中。");
        }
    }
}
