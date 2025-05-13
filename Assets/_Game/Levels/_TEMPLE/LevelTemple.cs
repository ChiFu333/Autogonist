using UnityEngine;
using Cysharp.Threading.Tasks;

public class LevelTemple : MonoBehaviour
{
    private bool trigger = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private async UniTaskVoid Start()
    {
        await G.Main.textThrower.ThrowText(new LocString("TempleTeeeext!", "Тестовый тееееекст!"), R.normalVoice);
        await WaitTrigger();
    }

    public async UniTask WaitTrigger()
    {
        trigger = false;
        await UniTask.WaitUntil(() => trigger);
    }

    public void TriggerBool()
    {
        trigger = true;
    }
}
