//using BehaviorDesigner.Runtime;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;

public enum AnimalDisplayState
{

}
public class AnimalDisplay : MonoBehaviour
{
    public AnimalDisplayState State;
    public float BaseSize;
    //public BehaviorTree bt;
    //public Animator animator;
    public SpriteRenderer spriteRenderer;
    public Dictionary<AnimalAgeStatus, Sprite> sprites =new Dictionary<AnimalAgeStatus, Sprite>();
    public void Init(AnimalData animaldata /*,AnimalDisplayState AnimationName*/, AnimalAgeStatus SpriteRendererName)
    {
        //TODO:SetAnimalDisplay; 增加资源
        //animator ??= GetComponent<Animator>();
        sprites.Clear();
        spriteRenderer ??= GetComponent<SpriteRenderer>();
        sprites.Add(AnimalAgeStatus.Adult, animaldata.GetAdultSprite());
        sprites.Add(AnimalAgeStatus.Juvenile, animaldata.GetJuvenileSprite());
        sprites.TryGetValue(SpriteRendererName, out var s);
        spriteRenderer.sprite = s;
        BaseSize = Random.Range(0.5f, 1.0f);

        // 基于原始大小设置倍率
        // 获取精灵
        Sprite sprite = spriteRenderer.sprite;

        // 计算精灵的世界单位大小
        Vector2 originalWorldSize = new Vector2(
            sprite.rect.width / sprite.pixelsPerUnit,
            sprite.rect.height / sprite.pixelsPerUnit
        );

        // 计算新的大小
        Vector2 newSize = new Vector2(
            originalWorldSize.x * BaseSize,
            originalWorldSize.y * BaseSize
        );
        //bt??= transform.AddComponent<BehaviorTree>(); 
        //var extBt = Resources.Load<ExternalBehaviorTree>(SysDefine.BehaviorTreeDefaultTree);
        //bt.StartWhenEnabled = true;
        //bt.ExternalBehavior = extBt;
        //bt.RestartWhenComplete = true;
        // 设置到所有 SpriteRenderer
        spriteRenderer.size = newSize;
        //SetAnimalDisPlay(AnimationName);
       // SetAnimalDisplay(SpriteRendererName);
    }
    //public void SetAnimalDisPlay(AnimalDisplayState s)
    //{
    //    animator.SetTrigger(s.ToString());
    //}
    public void SetAnimalDisplay(AnimalAgeStatus animalAgeStatus)
    {
        sprites.TryGetValue(animalAgeStatus, out var s);
        spriteRenderer.sprite = s;
    }
}
