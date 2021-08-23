using UnityEngine;

public class Explosion : WarEntity
{
    [SerializeField] [Range(0.0f, 1.0f)] private float duration = 0.5f;

    [SerializeField] private AnimationCurve opacityCurve;
    [SerializeField] private AnimationCurve scaleCurve;

    private float age;
    private float scale;

    private MeshRenderer meshRenderer;

    private static int colorPropertyId = Shader.PropertyToID("_Color");

    private static MaterialPropertyBlock propertyBlock;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        Debug.Assert(meshRenderer != null, "Explosion without renderer !");
    }

    public void Initialize(Vector3 _position, float _blastRadius, float _damage = 0.0f)
    {
        if (_damage > 0.0f)
        {
            TargetPoint.FillBuffer(_position, _blastRadius);

            for (var i = 0; i < TargetPoint.BufferedCount; i++)
            {
                TargetPoint.GetBuffered(i).Enemy.ApplyDamage(_damage);
            }
        }

        transform.localPosition = _position;

        scale = 2.0f * _blastRadius;
    }

    public override bool GameUpdate()
    {
        age += Time.deltaTime;

        if (age >= duration)
        {
            OriginFactory.Reclaim(this);

            return false;
        }

        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }

        float t = age / duration;

        Color c = Color.clear;
        c.a = opacityCurve.Evaluate(t);

        propertyBlock.SetColor(colorPropertyId, c);
        meshRenderer.SetPropertyBlock(propertyBlock);
        transform.localScale = Vector3.one * (scale * scaleCurve.Evaluate(t));

        return true;
    }
}