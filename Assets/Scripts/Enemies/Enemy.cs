using UnityEngine;

public class Enemy : GameBehavior
{
    [SerializeField] private EnemyAnimationConfig animationConfig;
    
    [SerializeField] private Transform model;

    private EnemyFactory originFactory;

    private EnemyAnimator animator;

    private GameTile tileFrom;
    private GameTile tileTo;

    private Vector3 positionFrom;
    private Vector3 positionTo;

    private Direction direction;
    private DirectionChange directionChange;

    private float directionAngleFrom;
    private float directionAngleTo;
    private float progress;
    private float progressFactor;
    private float pathOffset;
    private float speed;

    private Collider targetPointCollider;
    
    public float Scale { get; private set; }

    private float Health { get; set; }

    public bool IsValidTarget => animator.CurrentClip == EnemyAnimator.Clip.Move;

    public EnemyFactory OriginFactory
    {
        get => originFactory;
        set
        {
            Debug.Assert(originFactory == null, "Redefined origin factory !");

            originFactory = value;
        }
    }

    public Collider TargetPointCollider
    {
        set
        {
            Debug.Assert(targetPointCollider == null, "Redefined collider !");

            targetPointCollider = value;
        }
    }

    void Awake()
    {
        animator.Configure(model.GetChild(0).gameObject.AddComponent<Animator>(), animationConfig);
    }

    public void Initialize(float _scale, float _speed, float _pathOffset, float _health)
    {
        model.localScale = new Vector3(_scale, _scale, _scale);
        Scale = _scale;
        speed = _speed;
        pathOffset = _pathOffset;
        Health = _health;

        animator.PlayIntro();

        targetPointCollider.enabled = false;
    }

    void OnDestroy()
    {
        animator.Destroy();
    }

    public override void Recycle()
    {
        animator.Stop();
        
        OriginFactory.Reclaim(this);
    }

    public override bool GameUpdate()
    {
        #if UNITY_EDITOR
            if (!animator.IsValid)
            {
                animator.RestoreAfterHotReload(model.GetChild(0).GetComponent<Animator>(), animationConfig,
                                               animationConfig.MoveAnimationSpeed * speed / Scale);
            }
        #endif
        
        animator.GameUpdate();
        
        if (animator.CurrentClip == EnemyAnimator.Clip.Intro)
        {
            if (!animator.IsDone)
            {
                return true;
            }

            animator.PlayMove(animationConfig.MoveAnimationSpeed * speed / Scale);

            targetPointCollider.enabled = true;
        }
        else if (animator.CurrentClip >= EnemyAnimator.Clip.Outro)
        {
            if (animator.IsDone)
            {
                Recycle();

                return false;
            }

            return true;
        }
        
        if (Health <= 0.0f)
        {
            animator.PlayDying();

            targetPointCollider.enabled = false;

            return true;
        }
        
        progress += Time.deltaTime * progressFactor;

        while (progress >= 1.0f)
        {
            if (tileTo == null)
            {
                Game.EnemyReachedDestination();

                animator.PlayOutro();

                targetPointCollider.enabled = false;

                return true;
            }

            progress = (progress - 1.0f) / progressFactor;

            PrepareNextState();

            progress *= progressFactor;
        }

        if (directionChange == DirectionChange.None)
        {
            transform.localPosition = Vector3.LerpUnclamped(positionFrom, positionTo, progress);
        }
        else
        {
            float angle = Mathf.LerpUnclamped(directionAngleFrom, directionAngleTo, progress);

            transform.localRotation = Quaternion.Euler(0.0f, angle, 0.0f);
        }

        return true;
    }

    public void ApplyDamage(float _damage)
    {
        Debug.Assert(_damage >= 0.0f, "Negative damage applied.");

        Health -= _damage;
    }

    public void SpawnOn(GameTile _tile)
    {
        Debug.Assert(_tile.NextTileOnPath != null, "Nowhere to go !", this);

        tileFrom = _tile;
        tileTo = _tile.NextTileOnPath;

        progress = 0.0f;

        PrepareIntro();
    }

    void PrepareNextState()
    {
        tileFrom = tileTo;
        tileTo = tileTo.NextTileOnPath;
        
        positionFrom = positionTo;

        if (tileTo == null)
        {
            PrepareOutro();

            return;
        }
        
        positionTo = tileFrom.ExitPoint;

        directionChange = direction.GetDirectionChangeTo(tileFrom.PathDirection);
        direction = tileFrom.PathDirection;
        directionAngleFrom = directionAngleTo;

        switch (directionChange)
        {
            case DirectionChange.None:
            {
                PrepareForward();

                break;
            }

            case DirectionChange.TurnRight:
            {
                PrepareTurnRight();

                break;
            }

            case DirectionChange.TurnLeft:
            {
                PrepareTurnLeft();

                break;
            }

            case DirectionChange.TurnAround:
            {
                PrepareTurnAround();

                break;
            }
        }
    }

    void PrepareIntro()
    {
        positionFrom = tileFrom.transform.localPosition;
        transform.localPosition = positionFrom;
        positionTo = tileFrom.ExitPoint;

        direction = tileFrom.PathDirection;
        directionChange = DirectionChange.None;
        directionAngleFrom = directionAngleTo = direction.GetAngle();

        model.localPosition = new Vector3(pathOffset, 0.0f);

        transform.localRotation = direction.GetRotation();

        progressFactor = 2.0f * speed;
    }

    void PrepareOutro()
    {
        positionTo = tileFrom.transform.localPosition;

        directionChange = DirectionChange.None;
        directionAngleTo = direction.GetAngle();

        model.localPosition = new Vector3(pathOffset, 0.0f);

        transform.localRotation = direction.GetRotation();

        progressFactor = 2.0f * speed;
    }

    void PrepareForward()
    {
        transform.localRotation = direction.GetRotation();

        directionAngleTo = direction.GetAngle();

        model.localPosition = new Vector3(pathOffset, 0.0f);

        progressFactor = speed;
    }

    void PrepareTurnRight()
    {
        directionAngleTo = directionAngleFrom + 90.0f;

        model.localPosition = new Vector3(pathOffset - 0.5f, 0.0f);

        transform.localPosition = positionFrom + direction.GetHalfVector();

        progressFactor = speed / (Mathf.PI * 0.5f * (0.5f - pathOffset));
    }

    void PrepareTurnLeft()
    {
        directionAngleTo = directionAngleFrom - 90.0f;

        model.localPosition = new Vector3(pathOffset + 0.5f, 0.0f);

        transform.localPosition = positionFrom + direction.GetHalfVector();

        progressFactor = speed / (Mathf.PI * 0.5f * (0.5f - pathOffset));
    }

    void PrepareTurnAround()
    {
        directionAngleTo = directionAngleFrom + (pathOffset < 0.0f ? 180.0f : -180.0f);

        model.localPosition = Vector3.zero;

        transform.localPosition = new Vector3(pathOffset, 0.0f);

        progressFactor = speed / (Mathf.PI * Mathf.Max(Mathf.Abs(pathOffset), 0.2f));
    }
}