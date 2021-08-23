using UnityEngine;

[CreateAssetMenu]
public class GameTileContentFactory : GameObjectFactory
{
    [SerializeField] private GameTileContent destinationPrefab;
    [SerializeField] private GameTileContent emptyPrefab;
    [SerializeField] private GameTileContent wallPrefab;
    [SerializeField] private GameTileContent spawnPointPrefab;
    [SerializeField] private Tower[] towerPrefabs;
    
    public void Reclaim(GameTileContent _content)
    {
        Debug.Assert(_content.OriginFactory == this, "Wrong factory reclaimed !");
        
        Destroy(_content.gameObject);
    }
    
    T Get<T>(T _prefab) where T : GameTileContent
    {
        T instance = CreateGameObjectInstance(_prefab);
        instance.OriginFactory = this;

        return instance;
    }

    public GameTileContent Get(GameTileContentType _type)
    {
        switch (_type)
        {
            case GameTileContentType.Destination:
            {
                return Get(destinationPrefab);
            }

            case GameTileContentType.Empty:
            {
                return Get(emptyPrefab);
            }

            case GameTileContentType.Wall:
            {
                return Get(wallPrefab);
            }

            case GameTileContentType.SpawnPoint:
            {
                return Get(spawnPointPrefab);
            }
        }

        Debug.Assert(false, "Unsupported non-tower type : " + _type);

        return null;
    }

    public Tower Get(TowerType _type)
    {
        Debug.Assert((int)_type < towerPrefabs.Length, "Unsupported tower type !");

        Tower prefab = towerPrefabs[(int) _type];

        Debug.Assert(_type == prefab.TowerType, "Tower prefab at wrong index !");

        return Get(prefab);
    }
}