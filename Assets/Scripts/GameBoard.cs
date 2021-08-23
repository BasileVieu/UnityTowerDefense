using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    [SerializeField] private Transform ground;

    [SerializeField] private GameTile tilePrefab;

    [SerializeField] private Texture2D gridTexture;

    private GameTile[] tiles;

    private Vector2Int size;
    
    Queue<GameTile> searchFrontier = new Queue<GameTile>();
    
    List<GameTile> spawnPoints = new List<GameTile>();
    List<GameTileContent> updatingContent = new List<GameTileContent>();

    private GameTileContentFactory contentFactory;

    private bool showPaths;
    private bool showGrid;

    public bool ShowPaths
    {
        get => showPaths;
        set
        {
            showPaths = value;

            if (showPaths)
            {
                foreach (GameTile tile in tiles)
                {
                    tile.ShowPath();
                }
            }
            else
            {
                foreach (GameTile tile in tiles)
                {
                    tile.HidePath();
                }
            }
        }
    }

    public bool ShowGrid
    {
        get => showGrid;
        set
        {
            showGrid = value;

            Material material = ground.GetComponent<MeshRenderer>().material;

            if (showGrid)
            {
                material.mainTexture = gridTexture;

                material.SetTextureScale("_MainTex", size);
            }
            else
            {
                material.mainTexture = null;
            }
        }
    }

    public int SpawnPointCount => spawnPoints.Count;

    public void Initialize(Vector2Int _size, GameTileContentFactory _contentFactory)
    {
        size = _size;
        contentFactory = _contentFactory;
        ground.localScale = new Vector3(size.x, size.y, 1.0f);
        
        tiles = new GameTile[size.x * size.y];

        var offset = new Vector2((size.x - 1) * 0.5f, (size.y - 1) * 0.5f);

        for (int i = 0, y = 0; y < size.y; y++)
        {
            for (var x = 0; x < size.x; x++, i++)
            {
                GameTile tile = tiles[i] = Instantiate(tilePrefab, transform, false);
                tile.transform.localPosition = new Vector3(x - offset.x, 0.0f, y - offset.y);

                if (x > 0)
                {
                    GameTile.MakeEastWestNeighbors(tile, tiles[i - 1]);
                }

                if (y > 0)
                {
                    GameTile.MakeNorthSouthNeighbors(tile, tiles[i - size.x]);
                }

                tile.IsAlternative = (x & 1) == 0;

                if ((y & 1) == 0)
                {
                    tile.IsAlternative = !tile.IsAlternative;
                }
            }
        }

        Clear();
    }

    public void GameUpdate()
    {
        for (var i = 0; i < updatingContent.Count; i++)
        {
            updatingContent[i].GameUpdate();
        }
    }

    bool FindPaths()
    {
        foreach (GameTile tile in tiles)
        {
            if (tile.Content.Type == GameTileContentType.Destination)
            {
                tile.BecomeDestination();
                searchFrontier.Enqueue(tile);
            }
            else
            {
                tile.ClearPath();
            }
        }

        if (searchFrontier.Count == 0)
        {
            return false;
        }

        while (searchFrontier.Count > 0)
        {
            GameTile tile = searchFrontier.Dequeue();

            if (tile != null)
            {
                if (tile.IsAlternative)
                {
                    searchFrontier.Enqueue(tile.GrowPathNorth());
                    searchFrontier.Enqueue(tile.GrowPathSouth());
                    searchFrontier.Enqueue(tile.GrowPathEast());
                    searchFrontier.Enqueue(tile.GrowPathWest());
                }
                else
                {
                    searchFrontier.Enqueue(tile.GrowPathWest());
                    searchFrontier.Enqueue(tile.GrowPathEast());
                    searchFrontier.Enqueue(tile.GrowPathSouth());
                    searchFrontier.Enqueue(tile.GrowPathNorth());
                }
            }
        }

        if (tiles.Any(_tile => !_tile.HasPath))
        {
            return false;
        }

        if (showPaths)
        {
            foreach (GameTile tile in tiles)
            {
                tile.ShowPath();
            }
        }

        return true;
    }

    public void ToggleDestination(GameTile _tile)
    {
        if (_tile.Content.Type == GameTileContentType.Destination)
        {
            _tile.Content = contentFactory.Get(GameTileContentType.Empty);

            if (!FindPaths())
            {
                _tile.Content = contentFactory.Get(GameTileContentType.Destination);

                FindPaths();
            }
        }
        else if (_tile.Content.Type == GameTileContentType.Empty)
        {
            _tile.Content = contentFactory.Get(GameTileContentType.Destination);

            FindPaths();
        }
    }

    public void ToggleWall(GameTile _tile)
    {
        if (_tile.Content.Type == GameTileContentType.Wall)
        {
            _tile.Content = contentFactory.Get(GameTileContentType.Empty);

            FindPaths();
        }
        else if (_tile.Content.Type == GameTileContentType.Empty)
        {
            _tile.Content = contentFactory.Get(GameTileContentType.Wall);

            if (!FindPaths())
            {
                _tile.Content = contentFactory.Get(GameTileContentType.Empty);

                FindPaths();
            }
        }
    }

    public void ToggleSpawnPoint(GameTile _tile)
    {
        if (_tile.Content.Type == GameTileContentType.SpawnPoint)
        {
            if (spawnPoints.Count > 1)
            {
                spawnPoints.Remove(_tile);
                _tile.Content = contentFactory.Get(GameTileContentType.Empty);
            }
        }
        else if (_tile.Content.Type == GameTileContentType.Empty)
        {
            _tile.Content = contentFactory.Get(GameTileContentType.SpawnPoint);
            spawnPoints.Add(_tile);
        }
    }

    public void ToggleTower(GameTile _tile, TowerType _towerType)
    {
        if (_tile.Content.Type == GameTileContentType.Tower)
        {
            updatingContent.Remove(_tile.Content);

            if (((Tower) _tile.Content).TowerType == _towerType)
            {
                _tile.Content = contentFactory.Get(GameTileContentType.Empty);

                FindPaths();
            }
            else
            {
                _tile.Content = contentFactory.Get(_towerType);

                updatingContent.Add(_tile.Content);
            }
        }
        else if (_tile.Content.Type == GameTileContentType.Empty)
        {
            _tile.Content = contentFactory.Get(_towerType);

            if (FindPaths())
            {
                updatingContent.Add(_tile.Content);
            }
            else
            {
                _tile.Content = contentFactory.Get(GameTileContentType.Empty);

                FindPaths();
            }
        }
        else if (_tile.Content.Type == GameTileContentType.Wall)
        {
            _tile.Content = contentFactory.Get(_towerType);

            updatingContent.Add(_tile.Content);
        }
    }

    public GameTile GetTile(Ray _ray)
    {
        if (Physics.Raycast(_ray, out RaycastHit hit, float.MaxValue, 1))
        {
            var x = (int) (hit.point.x + size.x * 0.5f);
            var y = (int) (hit.point.z + size.y * 0.5f);

            if (x >= 0
                && x < size.x
                && y >= 0
                && y < size.y)
            {
                return tiles[x + y * size.x];
            }
        }
        
        return null;
    }

    public GameTile GetSpawnPoint(int _index) => spawnPoints[_index];

    public void Clear()
    {
        foreach (GameTile tile in tiles)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Empty);
        }

        spawnPoints.Clear();

        updatingContent.Clear();

        ToggleDestination(tiles[tiles.Length / 2]);

        ToggleSpawnPoint(tiles[0]);
    }
}