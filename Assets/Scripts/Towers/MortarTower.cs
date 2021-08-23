using UnityEngine;

public class MortarTower : Tower
{
    [SerializeField] [Range(0.5f, 2.0f)] private float shotsPerSecond = 1.0f;
    [SerializeField] [Range(0.5f, 3.0f)] private float shellBlastRadius = 1.0f;
    [SerializeField] [Range(1.0f, 100.0f)] private float shellDamage = 10.0f;

    [SerializeField] private Transform mortar;

    private float launchSpeed;
    private float launchProgress;

    public override TowerType TowerType => TowerType.Mortar;

    void Awake()
    {
        OnValidate();
    }

    void OnValidate()
    {
        float x = targetingRange + 0.25001f;
        float y = -mortar.position.y;
        
        launchSpeed = Mathf.Sqrt(9.81f * (y + Mathf.Sqrt(x * x + y * y)));
    }

    public override void GameUpdate()
    {
        launchProgress += shotsPerSecond * Time.deltaTime;

        while (launchProgress >= 1.0f)
        {
            if (AcquireTarget(out TargetPoint target))
            {
                Launch(target);

                launchProgress -= 1.0f;
            }
            else
            {
                launchProgress = 0.999f;
            }
        }
    }

    public void Launch(TargetPoint _target)
    {
        Vector3 launchPoint = mortar.position;
        Vector3 targetPoint = _target.Position;
        targetPoint.y = 0.0f;

        Vector2 dir;
        dir.x = targetPoint.x - launchPoint.x;
        dir.y = targetPoint.z - launchPoint.z;

        float x = dir.magnitude;
        float y = -launchPoint.y;

        dir /= x;

        var g = 9.81f;
        float s = launchSpeed;
        float s2 = s * s;

        float r = s2 * s2 - g * (g * x * x + 2.0f * y * s2);

        Debug.Assert(r >= 0.0f, "Launch velocity insufficient for range !");
        
        float tanTheta = (s2 + Mathf.Sqrt(r)) / (g * x);
        float cosTheta = Mathf.Cos(Mathf.Atan(tanTheta));
        float sinTheta = cosTheta * tanTheta;

        mortar.localRotation = Quaternion.LookRotation(new Vector3(dir.x, tanTheta, dir.y));

        Game.SpawnShell().Initialize(launchPoint, targetPoint,
                                     new Vector3(s * cosTheta * dir.x, s * sinTheta, s * cosTheta * dir.y),
                                     shellBlastRadius, shellDamage);
    }
}