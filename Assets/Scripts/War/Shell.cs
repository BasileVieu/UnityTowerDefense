using UnityEngine;

public class Shell : WarEntity
{
    private Vector3 launchPoint;
    private Vector3 targetPoint;
    private Vector3 launchVelocity;

    private float age;
    private float blastRadius;
    private float damage;

    public void Initialize(Vector3 _launchPoint, Vector3 _targetPoint, Vector3 _launchVelocity, float _blastRadius, float _damage)
    {
        launchPoint = _launchPoint;
        targetPoint = _targetPoint;
        launchVelocity = _launchVelocity;
        blastRadius = _blastRadius;
        damage = _damage;
    }

    public override bool GameUpdate()
    {
        age += Time.deltaTime;

        Vector3 p = launchPoint + launchVelocity * age;
        p.y -= 0.5f * 9.81f * age * age;

        if (p.y <= 0.0f)
        {
            Game.SpawnExplosion().Initialize(targetPoint, blastRadius, damage);
            
            OriginFactory.Reclaim(this);

            return false;
        }

        transform.localPosition = p;

        Vector3 d = launchVelocity;
        d.y -= 9.81f * age;

        transform.localRotation = Quaternion.LookRotation(d);

        Game.SpawnExplosion().Initialize(p, 0.1f);

        return true;
    }
}