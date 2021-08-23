using UnityEngine;

public class GameTile : MonoBehaviour
{
    [SerializeField] private Transform arrow;

    private GameTile north;
    private GameTile east;
    private GameTile south;
    private GameTile west;
    private GameTile nextOnPath;

    private int distance;

    private GameTileContent content;

    private static Quaternion northRotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
    private static Quaternion eastRotation = Quaternion.Euler(90.0f, 90.0f, 0.0f);
    private static Quaternion southRotation = Quaternion.Euler(90.0f, 180.0f, 0.0f);
    private static Quaternion westRotation = Quaternion.Euler(90.0f, 270.0f, 0.0f);

    public bool HasPath => distance != int.MaxValue;
    
    public bool IsAlternative { get; set; }
    
    public Vector3 ExitPoint { get; private set; }
    
    public Direction PathDirection { get; private set; }

    public GameTileContent Content
    {
        get => content;
        set
        {
            Debug.Assert(value != null, "Null assigned to content !");
            
            if (content != null)
            {
                content.Recycle();
            }

            content = value;
            content.transform.localPosition = transform.localPosition;
        }
    }

    public GameTile NextTileOnPath => nextOnPath;

    public void HidePath()
    {
        arrow.gameObject.SetActive(false);
    }

    public void BecomeDestination()
    {
        distance = 0;
        nextOnPath = null;
        ExitPoint = transform.localPosition;
    }

    GameTile GrowPathTo(GameTile _neighbor, Direction _direction)
    {
        Debug.Assert(HasPath, "No path!");

        if (_neighbor == null
            || _neighbor.HasPath)
        {
            return null;
        }
        
        _neighbor.distance = distance + 1;
        _neighbor.nextOnPath = this;

        _neighbor.ExitPoint = _neighbor.transform.localPosition + _direction.GetHalfVector();

        _neighbor.PathDirection = _direction;

        return _neighbor.Content.BlocksPath ? null : _neighbor;
    }

    public GameTile GrowPathNorth() => GrowPathTo(north, Direction.South);
    public GameTile GrowPathEast() => GrowPathTo(east, Direction.West);
    public GameTile GrowPathSouth() => GrowPathTo(south, Direction.North);
    public GameTile GrowPathWest() => GrowPathTo(west, Direction.East);

    public void ShowPath()
    {
        if (distance == 0)
        {
            arrow.gameObject.SetActive(false);

            return;
        }

        arrow.gameObject.SetActive(true);

        arrow.localRotation = nextOnPath == north ? northRotation :
                              nextOnPath == east ? eastRotation :
                              nextOnPath == south ? southRotation : westRotation;
    }

    public void ClearPath()
    {
        distance = int.MaxValue;
        nextOnPath = null;
    }

    public static void MakeEastWestNeighbors(GameTile _east, GameTile _west)
    {
        Debug.Assert(_west.east == null && _east.west == null, "Redefined neighbors!");
        
        _west.east = _east;
        _east.west = _west;
    }

    public static void MakeNorthSouthNeighbors(GameTile _north, GameTile _south)
    {
        Debug.Assert(_south.north == null && _north.south == null, "Redefined neighbors!");
        
        _south.north = _north;
        _north.south = _south;
    }
}