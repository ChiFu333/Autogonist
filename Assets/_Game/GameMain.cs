using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.Rendering.Universal; // Для Light2D
using DG.Tweening;

public class GameMain : MonoBehaviour
{
    public PlayerController player;
    public EnemyController enemy;
    public AnimationDataSO openMouth;
    public AnimationDataSO eated;
    public GameObject badEnd;
    
    public TextThrower textThrower;
    public InventoryController inventory;
    public SliderProgressBar progressBar;
    public List<Item> items;
    public List<Item> itemsDatabase;
    public Door endDoor;
    [SerializeField] private Light2D light;

    
    void Start()
    {
        G.AudioManager.PlayMusic(R.Audio.factorybg);
        itemsDatabase = items.ToList();
        SetUpStorages();
        SetUpBreakables();
        
        if(player != null && enemy != null) RoomChecker().Forget();
    }

    private async UniTask RoomChecker()
    {
        while (!(player.myRoom == enemy.myRoom && !player._inAnim && !enemy._inAnim))
        {
            await UniTask.Yield();
        }

        player._inAnim = true;
        player.CancelCurrentMovement();
        
        enemy.end = true;
        enemy._inAnim = true;
        enemy.CancelCurrentMovement();

        float del = player.transform.position.x - enemy.transform.position.x;
        await enemy.StartMovement(player.transform.position.x + ((del < 0) ? 1f : -1f));
        
        
        float bigPunchStrength = 0.5f;
        float bigPunchDuration = 0.7f;

        await enemy._enemyAnim.SetAnimOneShot(openMouth); 
        _ = enemy._enemyAnim.SetAnimOneShot(eated);
        player.gameObject.SetActive(false);
        G.AudioManager.PlaySound(R.Audio.Gulp);
        await enemy.transform.DOPunchScale(Vector3.one * bigPunchStrength, bigPunchDuration, 13, 1f)
            .AsyncWaitForCompletion();

        await UniTask.Delay(800);
        badEnd.SetActive(true);
    }
    
    public Item GetItem()
    {
        var it = items[0];
        items.RemoveAt(0);
        return it;
    }

    private void SetUpStorages()
    {
        for (int i = items.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            // Меняем элементы местами
            (items[i], items[randomIndex]) = (items[randomIndex], items[i]);
        }

        List<Storage> list = FindObjectsByType<Storage>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).ToList();
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            // Меняем элементы местами
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
        for (int i = 0; i < items.Count; i++)
        {
            list[i].isHereSomething = true;
        }
    }

    public void SetUpBreakables()
    {
        List<Breakable> list = FindObjectsByType<Breakable>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).ToList();
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            // Меняем элементы местами
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }

        for (int i = 0; i < list.Count; i++)
        {
            list[i].neededId = i;
        }
    }

public Item FindItemWithId(int id)
    {
        for (int i = 0; i < itemsDatabase.Count; i++)
        {
            if(itemsDatabase[i].id == id)
                return itemsDatabase[id];
        }

        return null;
    }

    public async void DoBlockout()
    {
        ConveyorManager.Instance.StopAllConveyors();
        await UniTask.Delay(200);
        G.AudioManager.StopMusic();
        G.AudioManager.PlaySound(R.Audio.electrooff);
        await DOTween.To(() => light.intensity, 
            x => light.intensity = x, 
            0.05f, 
            0.5f).AsyncWaitForCompletion();
        endDoor.isWorking = true;
        DOTween.To(() => endDoor.render.material.GetFloat("_Glow"),
            x => endDoor.render.material.SetFloat("_Glow", x),
            30f,
            3f);
        DOTween.To(() => endDoor.render.material.GetFloat("_GlowGlobal"),
            x => endDoor.render.material.SetFloat("_GlowGlobal", x),
            15f,
            3f);
    }
}
