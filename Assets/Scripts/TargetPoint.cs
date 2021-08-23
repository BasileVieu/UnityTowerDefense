using UnityEngine;

public class TargetPoint : MonoBehaviour
{
    private const int enemyLayerMask = 1 << 9;

    private static Collider[] buffer = new Collider[100];
    
    public static int BufferedCount { get; private set; }
    
    public Enemy Enemy { get; private set; }

    public Vector3 Position => transform.position;

    public static TargetPoint RandomBuffered => GetBuffered(Random.Range(0, BufferedCount));

    void Awake()
    {
        Enemy = transform.root.GetComponent<Enemy>();
        
        Debug.Assert(Enemy != null, "Target point without Enemy root !", this);

        Debug.Assert(GetComponent<SphereCollider>() != null, "Target point without sphere collider !", this);
        
        Debug.Assert(gameObject.layer == 9, "Target point on wrong layer !", this);

        Enemy.TargetPointCollider = GetComponent<Collider>();
    }

    public static bool FillBuffer(Vector3 _position, float _range)
    {
        Vector3 top = _position;
        top.y += 3.0f;

        BufferedCount = Physics.OverlapCapsuleNonAlloc(_position, top, _range, buffer, enemyLayerMask);

        return BufferedCount > 0;
    }

    public static TargetPoint GetBuffered(int _index)
    {
        var target = buffer[_index].GetComponent<TargetPoint>();
        
        Debug.Assert(target != null, "Targeted non-enemy !", buffer[0]);

        return target;
    }
}