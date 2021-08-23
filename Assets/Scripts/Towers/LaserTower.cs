using UnityEngine;

public class LaserTower : Tower
{
    [SerializeField] [Range(1.0f, 100.0f)] private float damagePerSecond = 10.0f;

    [SerializeField] private Transform turret;
    [SerializeField] private Transform laserBeam;

    private TargetPoint target;

    private Vector3 laserBeamScale;

    public override TowerType TowerType => TowerType.Laser;

    void Awake()
    {
        laserBeamScale = laserBeam.localScale;
    }

    public override void GameUpdate()
    {
        if (TrackTarget(ref target)
            || AcquireTarget(out target))
        {
            Shoot();
        }
        else
        {
            laserBeam.localScale = Vector3.zero;
        }
    }

    void Shoot()
    {
        Vector3 point = target.Position;
        turret.LookAt(point);
        laserBeam.localRotation = turret.localRotation;

        float d = Vector3.Distance(turret.position, point);
        laserBeamScale.z = d;
        laserBeam.localScale = laserBeamScale;
        laserBeam.localPosition = turret.localPosition + 0.5f * d * laserBeam.forward;

        target.Enemy.ApplyDamage(damagePerSecond * Time.deltaTime);
    }
}