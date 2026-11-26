using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    static UIManager instance;

    public TextMeshProUGUI coinEarnedText;
    public TextMeshProUGUI playerCoinsText;
    public TextMeshProUGUI level_text;

    public GameObject PedastrianUnlockedUI;
    public GameObject TwoLaneUnlockedUI;

    public Image SoundObj;

    public Sprite soundOnSprite;
    public Sprite soundOffSprite;


    public event Action OnSoundClicked;

    public static UIManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<UIManager>();

                if (instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(UIManager).Name;
                    instance = obj.AddComponent<UIManager>();
                    DontDestroyOnLoad(obj);
                }
            }

            return instance;
        }
    }

    private void Start()
    {
        GameManager.Instance.OnLevelFinished += OnLevelFinished;
        GameManager.Instance.CoinsAdded += CoinsAdded;
        GameManager.Instance.LevelLoaded += LevelLoaded;

        // 过关结算 UI 平时是隐藏的，金币数字得单独在常驻 Canvas 上显示一条
        CoinHud.Ensure(gameObject);
    }

    private void LevelLoaded()
    {
        int currentlvl = GameManager.Instance.currentLevel;

        //Show unlocked pedastrian in correct level index
        if(currentlvl == 7)
        {
            PedastrianUnlockedUI.SetActive(true);
        }

        //Show unlocked pedastrian in correct level index
        if (currentlvl == 9)
        {
            TwoLaneUnlockedUI.SetActive(true);
        }

        level_text.text = "第 " + currentlvl.ToString() + " 关";
    }

    private void CoinsAdded()
    {
        // 启动页领金币时关卡 UI 还没加载，playerCoinsText 可能是空的，这里要判空
        if (playerCoinsText != null)
        {
            playerCoinsText.text = GameManager.Instance.playerCoins.ToString();
        }
    }

    private void OnLevelFinished()
    {
        coinEarnedText.text = GameManager.Instance.coinsEarnedInCurrentLevel.ToString();
    }

    public void SoundClick()
    {
        bool soundOn = !GameManager.Instance.isSoundOn;
        GameManager.Instance.isSoundOn = soundOn;

        OnSoundClicked?.Invoke();

        SetSoundUI(soundOn);
    }

    void SetSoundUI(bool soundOn)
    {
        if (soundOn)
        {
            SoundObj.sprite = soundOnSprite;
        }
        else
        {
            SoundObj.sprite = soundOffSprite;
        }
    }
}
