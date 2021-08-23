using UnityEngine;
using Random = UnityEngine.Random;

public abstract class Tower : GameTileContent
{
    [SerializeField] [Range(1.5f, 10.5f)] protected float targetingRange = 1.5f;
    
    public abstract TowerType TowerType { get; }

    protected bool TrackTarget(ref TargetPoint  _target)
    {
        if (_target == null
            || !_target.Enemy.IsValidTarget)
        {
            return false;
        }

        Vector3 a = transform.localPosition;
        Vector3 b = _target.Position;

        float x = a.x - b.x;
        float z = a.z - b.z;

        float r = targetingRange + 0.125f * _target.Enemy.Scale;

        if (x * x + z * z > r * r)
        {
            _target = null;

            return false;
        }

        return true;
    }

    protected bool AcquireTarget(out TargetPoint _target)
    {
        if (TargetPoint.FillBuffer(transform.localPosition, targetingRange))
        {
            _target = TargetPoint.RandomBuffered;

            return true;
        }

        _target = null;

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector3 position = transform.localPosition;
        position.y += 0.01f;

        Gizmos.DrawWireSphere(position, targetingRange);
    }
}