using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField] private Vector2Int boardSize = new Vector2Int(11, 11);

    [SerializeField] private GameBoard board;

    [SerializeField] private GameTileContentFactory tileContentFactory;

    [SerializeField] private WarFactory warFactory;
    
    GameBehaviorCollection enemies = new GameBehaviorCollection();
    GameBehaviorCollection nonEnemies = new GameBehaviorCollection();

    [SerializeField] private GameScenario scenario;

    private GameScenario.State activeScenario;

    private TowerType selectedTowerType;

    private static Game instance;

    [SerializeField] [Range(0, 100)] private int startingPlayerHealth = 10;

    private int playerHealth;

    private const float pausedTimeScale = 0.0f;

    [SerializeField] [Range(1.0f, 10.0f)] private float playSpeed = 1.0f;

    private Ray TouchRay => Camera.main.ScreenPointToRay(Input.mousePosition);

    void Awake()
    {
        playerHealth = startingPlayerHealth;
        
        board.Initialize(boardSize, tileContentFactory);

        board.ShowGrid = true;

        activeScenario = scenario.Begin();
    }

    void OnEnable()
    {
        instance = this;
    }

    void OnValidate()
    {
        if (boardSize.x < 2)
        {
            boardSize.x = 2;
        }
        
        if (boardSize.y < 2)
        {
            boardSize.y = 2;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleTouch();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            HandleAlternativeTouch();
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            board.ShowPaths = !board.ShowPaths;
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            board.ShowGrid = !board.ShowGrid;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedTowerType = TowerType.Laser;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selectedTowerType = TowerType.Mortar;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Time.timeScale = Time.timeScale > pausedTimeScale ? pausedTimeScale : playSpeed;
        }
        else if (Time.timeScale > pausedTimeScale)
        {
            Time.timeScale = playSpeed;
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            BeginNewGame();
        }

        if (playerHealth <= 0
            && startingPlayerHealth > 0)
        {
            Debug.Log("Defeat !");

            BeginNewGame();
        }

        if (!activeScenario.Progress()
            && enemies.IsEmpty)
        {
            Debug.Log("Victory !");

            BeginNewGame();

            activeScenario.Progress();
        }

        enemies.GameUpdate();

        Physics.SyncTransforms();

        board.GameUpdate();

        nonEnemies.GameUpdate();
    }

    public static void SpawnEnemy(EnemyFactory _factory, EnemyType _type)
    {
        GameTile spawnPoint = instance.board.GetSpawnPoint(Random.Range(0, instance.board.SpawnPointCount));

        Enemy enemy = _factory.Get(_type);

        enemy.SpawnOn(spawnPoint);

        instance.enemies.Add(enemy);
    }

    public static Shell SpawnShell()
    {
        Shell shell = instance.warFactory.Shell;
        instance.nonEnemies.Add(shell);

        return shell;
    }

    public static Explosion SpawnExplosion()
    {
        Explosion explosion = instance.warFactory.Explosion;
        instance.nonEnemies.Add(explosion);

        return explosion;
    }

    void HandleTouch()
    {
        GameTile tile = board.GetTile(TouchRay);

        if (tile != null)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                board.ToggleTower(tile, selectedTowerType);
            }
            else
            {
                board.ToggleWall(tile);
            }
        }
    }

    void HandleAlternativeTouch()
    {
        GameTile tile = board.GetTile(TouchRay);

        if (tile != null)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                board.ToggleDestination(tile);
            }
            else
            {
                board.ToggleSpawnPoint(tile);
            }
        }
    }

    void BeginNewGame()
    {
        playerHealth = startingPlayerHealth;
        
        enemies.Clear();

        nonEnemies.Clear();

        board.Clear();

        activeScenario = scenario.Begin();
    }

    public static void EnemyReachedDestination()
    {
        instance.playerHealth -= 1;
    }
}