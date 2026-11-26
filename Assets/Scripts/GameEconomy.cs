using UnityEngine;

/// <summary>
/// 金币经济（唯一数值出口）
/// 所有「赚金币」「花金币」都走这里，避免各处直接读写 PlayerPrefs 导致数值打架。
///
/// 当前定价：
///   - 跳过本关：100 金币（也可以看激励视频免费跳过）
///   - 看一次激励视频（启动页「领金币」按钮）：150 金币
///   - 正常过关：车辆数 + 3（约 8~15 金币，见 GameManager.CarEscape）
/// </summary>
public static class GameEconomy
{
    public const string CoinsKey = "playercoins";

    /// <summary>跳过本关需要的金币</summary>
    public const int SkipLevelCost = 100;

    /// <summary>看一次激励视频发放的金币（略高于跳关价，鼓励从启动页看广告）</summary>
    public const int RewardedVideoCoins = 150;

    /// <summary>当前金币（以 PlayerPrefs 为准）</summary>
    public static int Coins
    {
        get { return PlayerPrefs.GetInt(CoinsKey, 0); }
    }

    /// <summary>加金币</summary>
    public static void AddCoins(int amount)
    {
        if (amount == 0)
        {
            return;
        }

        SetCoins(Coins + amount);
    }

    /// <summary>尝试花金币：够就扣掉返回 true，不够返回 false 且不扣</summary>
    public static bool TrySpend(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        int current = Coins;
        if (current < amount)
        {
            return false;
        }

        SetCoins(current - amount);
        return true;
    }

    private static void SetCoins(int value)
    {
        if (value < 0)
        {
            value = 0;
        }

        // UpdateCoins 内部会写 PlayerPrefs 并触发 CoinsAdded，让界面上的金币数立刻刷新
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateCoins(value);
        }
        else
        {
            PlayerPrefs.SetInt(CoinsKey, value);
        }

        PlayerPrefs.Save();
    }
}
