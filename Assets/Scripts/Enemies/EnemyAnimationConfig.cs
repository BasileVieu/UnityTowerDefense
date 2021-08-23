using UnityEngine;

[CreateAssetMenu]
public class EnemyAnimationConfig : ScriptableObject
{
    [SerializeField] private float moveAnimationSpeed = 1.0f;

    [SerializeField] private AnimationClip move;
    [SerializeField] private AnimationClip intro;
    [SerializeField] private AnimationClip outro;
    [SerializeField] private AnimationClip dying;
    [SerializeField] private AnimationClip appear;
    [SerializeField] private AnimationClip disappear;

    public AnimationClip Move => move;
    public AnimationClip Intro => intro;
    public AnimationClip Outro => outro;
    public AnimationClip Dying => dying;
    public AnimationClip Appear => appear;
    public AnimationClip Disappear => disappear;

    public float MoveAnimationSpeed => moveAnimationSpeed;
}