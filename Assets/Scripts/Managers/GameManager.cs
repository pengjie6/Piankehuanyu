using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using UnityEngine.SceneManagement;
//电子邮件puhalskijsemen@gmail.com
//源码网站 开vpn全局模式打开 http://web3incubators.com/
//电报https://t.me/gamecode999
//网页客服 http://web3incubators.com/kefu.html

public class GameManager : MonoBehaviour
{
    static GameManager instance;

    public GameObject coinPrefab;

    public int playerCoins;

    public int coinsEarnedInCurrentLevel;

    public int currentLevel;

    public bool isSoundOn = true;

    public bool isActive;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();

                if (instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(GameManager).Name;
                    instance = obj.AddComponent<GameManager>();
                    DontDestroyOnLoad(obj);
                }
            }

            return instance;
        }
    }

    public event Action OnCarHit;
    public event Action OnCarEscape;
    public event Action OnLevelFinished;
    public event Action OnLose;
    public event Action CoinsAdded;
    public event Action LevelLoaded;
    public event Action OnCarClick;

    public List<Car> CarsInLevel;

    public int carsInitialCount;



    public GameObject level_complete_UI;
    public GameObject level_lose_UI;

    public CanvasGroup logoScreen;

    private void Awake()
    {
        //PlayerPrefs.SetInt("Level", 29);
    }

    void Start()
    {

        // Subscribe to the sceneLoaded event
        SceneManager.sceneLoaded += OnSceneLoaded;

        if(SceneManager.GetActiveScene().name == "StartScene")
        {
            Invoke("InitGame", 1.5f);
        }
        else
        {
            #if UNITY_EDITOR
                LevelLoaded?.Invoke();

                level_complete_UI.gameObject.SetActive(false);
                level_lose_UI.gameObject.SetActive(false);

                HideLogoScreen();

                CarsInLevel.Clear();

                Car[] cars = GameObject.FindObjectsOfType<Car>();
                foreach (Car car in cars)
                {
                    CarsInLevel.Add(car);
                }

                carsInitialCount = CarsInLevel.Count;
            #endif
        }
    }

    void InitGame()
    {
        currentLevel = PlayerPrefs.GetInt("Level");
        if(currentLevel == 0 )
        {
            currentLevel = 1;

            PlayerPrefs.SetInt("Level", currentLevel);
        }

        if (SceneExists("Level_" + currentLevel))
        {
            SceneManager.LoadScene("Level_" + currentLevel);
        }
        else
        {
            SceneManager.LoadScene("Level_" + UnityEngine.Random.Range(10, 30));
        }
    }

    public void CarClicked()
    {
        OnCarClick?.Invoke();
    }

    public void CarHit()
    {
        OnCarHit?.Invoke();
    }

    GameObject carEscaped;
    public void CarEscape(GameObject car)
    {
        carEscaped = car;

        // spawn coin
        Instantiate(coinPrefab, car.transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity);

        CarsInLevel.Remove(car.GetComponent<Car>());

        if(CarsInLevel.Count <= 0)
        {
            coinsEarnedInCurrentLevel = carsInitialCount + 3;

            LevelFinished();

            return;
        }

        Invoke("DestroyCar", 0.4f);

        OnCarEscape?.Invoke();
    }

    void DestroyCar()
    {
        Destroy(carEscaped);
    }

    public void UpdateCoins(int amout)
    {
        playerCoins = amout;

        PlayerPrefs.SetInt("playercoins", playerCoins);

        CoinsAdded?.Invoke();
    }

    public void LevelFinished()
    {
        Debug.Log("Level Finished !");

        UpdateCoins(PlayerPrefs.GetInt("playercoins"));

        level_complete_UI.SetActive(true);
        level_complete_UI.GetComponent<DOTweenAnimation>().DORestartAllById("LVL_COMPLETE");

        OnLevelFinished?.Invoke();

        // 每次通关后播放插屏广告（等 0.3 秒，别和过关动画挤在同一帧）
        Invoke("ShowLevelFinishedInterstitial", 0.3f);
    }

    void ShowLevelFinishedInterstitial()
    {
        Ads.Instance.ShowInterstitial();
    }

    /// <summary>
    /// 关卡加载完成后预加载下一个插屏广告，保证通关时立刻能播
    /// </summary>
    void PreloadInterstitialAd()
    {
        Ads.Instance.PreloadInterstitial();
    }

    public void RestartLevel()
    {
        ShowLogoScreen();
    }

    public void LoadNextLevelDelay()
    {
        Invoke("LoadNextLevel", 1.25f);
    }

    public void SkipLevel()
    {
        LoadNextLevel();
    }

    void LoadNextLevel()
    {
        currentLevel++;

        PlayerPrefs.SetInt("Level", currentLevel);

        ShowLogoScreen();
    }

    void ShowLogoScreen()
    {
        // Set the initial alpha to 0
        logoScreen.alpha = 0f;

        logoScreen.gameObject.SetActive(true);

        logoScreen.DOFade(1f, 1.35f)
            .OnComplete(() => 
            {
                if (SceneExists("Level_" + currentLevel))
                {
                    SceneManager.LoadScene("Level_" + currentLevel);
                }
                else
                {
                    SceneManager.LoadScene("Level_" + UnityEngine.Random.Range(10,30));
                }
            });
    }

    bool SceneExists(string sceneName)
    {

        // Iterate through all loaded scenes and check if the specified scene exists
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string pathToScene = SceneUtility.GetScenePathByBuildIndex(i);
            string loadedScene = System.IO.Path.GetFileNameWithoutExtension(pathToScene);

            /*Debug.Log("LgCoreReloader: Reloading to scene(0): " + sceneName);

            Scene loadedScene = SceneManager.GetSceneByBuildIndex(i);
            Debug.LogWarning(loadedScene.name);
            */

            if (loadedScene == sceneName)
            {
                return true;
            }
        }
        return false;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Check if the loaded scene is the one you are interested in
        if (scene.name.Contains("Level"))
        {
            LevelLoaded?.Invoke();

            level_complete_UI.gameObject.SetActive(false);
            level_lose_UI.gameObject.SetActive(false);

            HideLogoScreen();

            CarsInLevel.Clear();

            Car[] cars = GameObject.FindObjectsOfType<Car>();
            foreach (Car car in cars)
            {
                CarsInLevel.Add(car);
            }

            carsInitialCount = CarsInLevel.Count;

            // 关卡加载完成后预加载插屏广告（1 秒后再拉，避免和场景加载抢资源）
            CancelInvoke("PreloadInterstitialAd");
            Invoke("PreloadInterstitialAd", 1f);
        }
    }

    void HideLogoScreen()
    {
        logoScreen.DOFade(0f, 1.35f)
            .OnComplete(() => logoScreen.gameObject.SetActive(false));
    }

    public void LevelLose()
    {
        
        level_lose_UI.GetComponent<DOTweenAnimation>().DORestartAllById("LVL_FAIL");
        level_lose_UI.SetActive(true);

        Debug.Log("YOU LOST !");


        OnLose?.Invoke();

        //CarsInLevel.Clear();
    }
}
