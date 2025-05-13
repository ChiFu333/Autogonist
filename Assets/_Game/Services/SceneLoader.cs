using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class SceneLoader : MonoBehaviour, IService
{
    public string currentSceneName = null;
    public string beforeSceneName = null;

    private SceneList _sceneList = new SceneList();
    private GameObject _fadeCanvas;
    private bool showFade;
    private float _speed = 0.35f;
    
    public void Init(bool showWhenInit)
    {
        showFade = showWhenInit;
        currentSceneName = SceneManager.GetActiveScene().name;
        CreateFadeCanvas();
        _sceneList.Init();
        _ = Unfade();
    }
    public void JustLoad(string n)
    {
        _ = Load(n);
    }
    public async UniTask Load(string n)
    {
        await Fade();
        if(showFade) await UniTask.Yield();
        LoadScene(n);
        if(showFade) await UniTask.Yield();
        
        await Unfade();
        //G.PausePanel.Init();
        //if(n == "MainMenu") G.AudioManager.PlayMusic(R.Audio.mainMenuMusic);
    }
    private void LoadScene(string n)
    {
        if(currentSceneName == null) return;
        beforeSceneName = currentSceneName;
        
        SceneManager.LoadScene(n);
        
        currentSceneName = n;
    }
    private void CreateFadeCanvas()
    {
        if(!showFade) return;
        _fadeCanvas = new GameObject("FadeCanvas");
        DontDestroyOnLoad(_fadeCanvas);
        _fadeCanvas.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        _fadeCanvas.GetComponent<Canvas>().sortingOrder = 1000; 

        _fadeCanvas.AddComponent<GraphicRaycaster>();

        GameObject fadeImage = new GameObject("FadeImage");
        fadeImage.transform.parent = _fadeCanvas.transform;
        
        fadeImage.AddComponent<Image>().color = Color.black;
        
        fadeImage.GetComponent<RectTransform>().anchorMin = Vector2.zero; // Якоря в нижний левый угол
        fadeImage.GetComponent<RectTransform>().anchorMax = Vector2.one;  // Якоря в верхний правый угол
        fadeImage.GetComponent<RectTransform>().offsetMin = Vector2.zero; // Нулевые отступы
        fadeImage.GetComponent<RectTransform>().offsetMax = Vector2.zero; 
    }
    private async UniTask Fade()
    {
        if (_fadeCanvas == null) return;
        bool trigger = false;
        _fadeCanvas.GetComponentInChildren<Image>().raycastTarget = true;
        _fadeCanvas.transform.GetChild(0).GetComponent<Image>().DOColor(new Color(0,0,0,1), _speed).onComplete = () => trigger = true;
        await UniTask.WaitUntil(() => trigger);
    }
    private async UniTask Unfade()
    {
        if (_fadeCanvas == null) return;
        bool trigger = false;
        _fadeCanvas.transform.GetChild(0).GetComponent<Image>().DOColor(new Color(0,0,0,0), _speed).onComplete = () => trigger = true;
        await UniTask.WaitUntil(() => trigger);
        _fadeCanvas.GetComponentInChildren<Image>().raycastTarget = false;
    }
}
public class SceneList
{
    public Dictionary<string, Scene> scenesByName;
    public void Init()
    {
        scenesByName = new Dictionary<string, Scene>()
        {
            { "MainMenu", SceneManager.GetSceneByBuildIndex(0) },
            { "MainMenuAboard", SceneManager.GetSceneByBuildIndex(1) },
            { "Level0", SceneManager.GetSceneByBuildIndex(2) },
            { "Level1", SceneManager.GetSceneByBuildIndex(3) },
            { "End", SceneManager.GetSceneByBuildIndex(4) }
        };
    }
    
}

