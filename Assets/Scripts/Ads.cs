using System;
using UnityEngine;

/// <summary>
/// 广告统一入口：只对接抖音小游戏（AdManager + TikTokPlatform）。
///
/// 对外保留原来被预制体绑定的方法名（ShowRewardedVideo / ShowInterstitial），
/// 这样场景和预制体上的按钮绑定不用动。
/// </summary>
public class Ads : MonoBehaviour
{
    static Ads instance;

    public static Ads Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<Ads>();

                if (instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(Ads).Name;
                    instance = obj.AddComponent<Ads>();
                    DontDestroyOnLoad(obj);
                }
            }

            return instance;
        }
    }

    void Start()
    {
        // 抖音广告平台由 AdManager 初始化（TTSDK）；场景里没挂 AdManager 时它自己会创建
        AdManager adManager = AdManager.Instance;
        if (adManager == null)
        {
            Debug.LogWarning("[广告] 抖音广告平台初始化失败，广告功能不可用");
        }
    }

    #region 插屏广告

    /// <summary>
    /// 插屏广告：每次通关后调用（GameManager.LevelFinished）
    /// </summary>
    public void ShowInterstitial()
    {
        if (AdManager.Instance == null)
        {
            Debug.LogWarning("[广告] 场景里没有 AdManager，跳过插屏广告");
            return;
        }

        AdManager.Instance.ShowInterstitialAd();
    }

    /// <summary>
    /// 预加载插屏：关卡加载完成后调用一次，这样通关时才有广告可放
    /// （抖音插屏实例是按需创建的，不预加载的话第一次通关会空放）
    /// </summary>
    public void PreloadInterstitial()
    {
        if (AdManager.Instance == null)
        {
            return;
        }

        AdManager.Instance.PreloadInterstitialAd();
    }

    #endregion

    #region 激励视频

    /// <summary>
    /// 「跳过本关」按钮绑定的就是这个方法：
    /// 弹出选择框，让玩家自己选「看广告免费」还是「花金币」。
    /// </summary>
    public void ShowRewardedVideo()
    {
        SkipLevelDialog.Show();
    }

    /// <summary>
    /// 播放激励视频（看完才算成功）
    /// </summary>
    public void PlayRewardedVideo(Action onSuccess, Action<string> onFail = null)
    {
#if UNITY_EDITOR
        // 编辑器里抖音接口是 mock，直接当作看完，方便调试流程
        Debug.Log("[广告] 编辑器环境：直接视为看完激励视频");
        if (onSuccess != null)
        {
            onSuccess();
        }
#else
        if (AdManager.Instance == null)
        {
            Debug.LogError("[广告] 场景里没有 AdManager，无法播放激励视频");
            if (onFail != null)
            {
                onFail("广告服务未初始化");
            }

            return;
        }

        AdManager.Instance.ShowRewardedVideoAd(onSuccess, onFail);
#endif
    }

    #endregion
}
