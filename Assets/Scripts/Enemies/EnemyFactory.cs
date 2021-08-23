using UnityEngine;

[CreateAssetMenu]
public class EnemyFactory : GameObjectFactory
{
    [System.Serializable]
    class EnemyConfig
    {
        [SerializeField] public Enemy prefab;

        [SerializeField] [FloatRangeSlider(0.5f, 2.0f)]
        public FloatRange scale = new FloatRange(1.0f);

        [SerializeField] [FloatRangeSlider(0.2f, 5.0f)]
        public FloatRange speed = new FloatRange(1.0f);

        [SerializeField] [FloatRangeSlider(-0.4f, 0.4f)]
        public FloatRange pathOffset = new FloatRange(0.0f);

        [SerializeField] [FloatRangeSlider(10.0f, 1000.0f)]
        public FloatRange health = new FloatRange(100.0f);
    }

    [SerializeField] private EnemyConfig small;
    [SerializeField] private EnemyConfig medium;
    [SerializeField] private EnemyConfig large;

    public Enemy Get(EnemyType _type = EnemyType.Medium)
    {
        EnemyConfig config = GetConfig(_type);
        
        Enemy instance = CreateGameObjectInstance(config.prefab);
        instance.OriginFactory = this;
        instance.Initialize(config.scale.RandomValueInRange, config.speed.RandomValueInRange, config.pathOffset.RandomValueInRange, config.health.RandomValueInRange);

        return instance;
    }

    EnemyConfig GetConfig(EnemyType _type)
    {
        switch (_type)
        {
            case EnemyType.Small:
            {
                return small;
            }

            case EnemyType.Medium:
            {
                return medium;
            }

            case EnemyType.Large:
            {
                return large;
            }
        }
        
        Debug.Assert(false, "Unsupported enemy type !");

        return null;
    }

    public void Reclaim(Enemy _enemy)
    {
        Debug.Assert(_enemy.OriginFactory == this, "Wrong factory reclaimed !");

        Destroy(_enemy.gameObject);
    }
}