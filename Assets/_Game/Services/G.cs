using UnityEngine;

public static class G 
{
    //Ссылки на переменные
    public static GameObject Canvas;
    //Сервисы
    public static LocSystem LocSystem;
    public static AudioManager AudioManager;
    public static SceneLoader SceneLoader;
    //Объекты в игре
    public static PausePanel PausePanel;
    public static GameMain Main;
}

public interface IService
{
    public void Init(bool b);
}