using UnityEngine;

[CreateAssetMenu]
public class WarFactory : GameObjectFactory
{
    [SerializeField] private Explosion explosionPrefab;
    [SerializeField] private Shell shellPrefab;

    public Shell Shell => Get(shellPrefab);

    public Explosion Explosion => Get(explosionPrefab);

    T Get<T>(T _prefab) where T : WarEntity
    {
        T instance = CreateGameObjectInstance(_prefab);
        instance.OriginFactory = this;

        return instance;
    }

    public void Reclaim(WarEntity _entity)
    {
        Debug.Assert(_entity.OriginFactory == this, "Wrong factory reclaimed !");

        Destroy(_entity.gameObject);
    }
}