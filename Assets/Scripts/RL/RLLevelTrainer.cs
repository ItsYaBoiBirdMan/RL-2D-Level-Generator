using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class RLLevelTrainer : MonoBehaviour
{
    private RLGeneratorAgent _sideScrollingterrainAgent = new RLGeneratorAgent();
    private RLGeneratorAgent _topDownAgent = new RLGeneratorAgent();
    private RLGeneratorAgent _platformAgent = new RLGeneratorAgent();
    private RLGeneratorAgent _enemyAgent = new RLGeneratorAgent();
    private RLGeneratorAgent _pickUpAgent = new RLGeneratorAgent();
    private RLGeneratorAgent _collectiblesAgent = new RLGeneratorAgent();
    private RLGeneratorAgent _hazardAgent = new RLGeneratorAgent();
    
    public enum GenerationMode
    {
        SideScroller,
        TopDown
    }
    
    public enum PlatformDifficultyRelation
    {
        MoreOnHigherDifficulty,
        MoreOnLowerDifficulty
    }

    [Header("Player Performance Tracker")] 
    [SerializeField] private PlayerPerformanceTracker PlayerPerformanceTracker;
    
    [Header("Basic Settings")]
    
    [SerializeField] private int LevelWidth = 150;
    [SerializeField] private int LevelHeight = 50;
    [SerializeField] private int TrainingEpisodes = 500;
    [SerializeField] private int BlockSize = 1;

    [SerializeField] private float Difficulty;
    
    [Header("Prefabs")] 
    [SerializeField] private GameObject GroundBlock;
    [SerializeField] private GameObject PlatformBlock;
    [SerializeField] private GameObject LeftEdgeTrigger;
    [SerializeField] private GameObject RightEdgeTrigger;
    [SerializeField] private GameObject SideScrollingEnemyPerfab;
    [SerializeField] private GameObject TopDownEnemyPerfab;
    [SerializeField] private GameObject CoinPrefab;
    [SerializeField] private GameObject PickupPrefab;
    [SerializeField] private GameObject HazardPrefab;
    [SerializeField] private GameObject DeathPlane;
    
    [Header("Object Parents")]
    [SerializeField] private Transform BlockParent;
    [SerializeField] private Transform EdgeParent;
    [SerializeField] private Transform EnemyParent;
    [SerializeField] private Transform CoinParent;
    [SerializeField] private Transform HazardParent;
    
    [Header("Player Object")]
    [SerializeField] private GameObject SideScrollerPlayer;
    [SerializeField] private GameObject TopDownPlayer;

    [Header("Generation Settings")] 
    [SerializeField] private GenerationMode generationMode;
    [SerializeField] private bool haveEnemies;
    [SerializeField] private bool havePlatforms;
    [SerializeField] private PlatformDifficultyRelation platformDifficultyRelation;
    [SerializeField] private int MinPlatformHeightFromGround = 4;
    [SerializeField] private int MaxPlatformHeightFromGround = 6;
    [SerializeField] private bool havePickups;
    [SerializeField] private bool haveHazards;
    [SerializeField] private bool haveCoins;
    [SerializeField] private bool haveEnemyScaling;

    [Header("Debug")] 
    [SerializeField] private bool DoNotInstantiate;
    
    private TileType[,] _tileGrid;
    private TileType[,] _decorationGrid;
    private int[] _groundHeight;
    private GameObject _actualPlayer;
    
 
    private int _enemyCount;
    private int _coinCount;
    private int _baseGroundHeight = 5;
    
    private void Start()
    {
        FlexibleGeneratorFunction();
    }

    public int GetEnemyCount()
    {
        return _enemyCount;
    }
    
    public int GetCoinCount()
    {
        return _coinCount;
    }
    
    public void FlexibleGeneratorFunction()
    {
        Difficulty = PlayerPerformanceTracker.Instance.Difficulty;
        _tileGrid = new TileType[LevelWidth, LevelHeight];
        _decorationGrid = new TileType[LevelWidth, LevelHeight];
        
        _sideScrollingterrainAgent = new RLGeneratorAgent();
        _topDownAgent = new RLGeneratorAgent();
        _platformAgent = new RLGeneratorAgent();
        _enemyAgent = new RLGeneratorAgent();
        _pickUpAgent = new RLGeneratorAgent();
        _collectiblesAgent = new RLGeneratorAgent();
        _hazardAgent = new RLGeneratorAgent();
        
        switch (generationMode)
        {
            case GenerationMode.SideScroller:
                
                if (SideScrollerPlayer == null)
                {
                    Debug.Log("No Side Scroller Player Object found. Stopping Generation");
                    return;
                }

                if (TopDownPlayer == null)
                {
                    Debug.Log("Attention! No Top Down Player Object found. Generation for Side Scroller level can still continue.");
                }
                
                _actualPlayer = SideScrollerPlayer;
                if (TopDownPlayer) TopDownPlayer.SetActive(false);
                
                for (int i = 0; i < TrainingEpisodes; i++)
                {
                    Generate2DSideScrollingTerrainWithRL();

                    if (havePlatforms)
                    {
                        switch (platformDifficultyRelation)
                        {
                            case PlatformDifficultyRelation.MoreOnLowerDifficulty:
                                GeneratePlatformsWithRLLessOnHigherDifficult();
                                break;
                            
                            case PlatformDifficultyRelation.MoreOnHigherDifficulty:
                                GeneratePlatformsWithRLMoreOnHigherDifficult();
                                break;
                        }
                    }
                    
                    
                    if(haveEnemies) Generate2DSideScrollingEnemiesWithRL();
                    if(havePickups) GenerateHealthPickupsWithRL();
                    if(haveHazards) GenerateStageHazardsWithRL();
                    if(haveCoins) Generate2DSideScrollingCoinsWithRL();
                }
                
                Generate2DSideScrollingTerrainWithRL();

                if (havePlatforms)
                {
                    switch (platformDifficultyRelation)
                    {
                        case PlatformDifficultyRelation.MoreOnLowerDifficulty:
                            GeneratePlatformsWithRLLessOnHigherDifficult();
                            break;
                            
                        case PlatformDifficultyRelation.MoreOnHigherDifficulty:
                            GeneratePlatformsWithRLMoreOnHigherDifficult();
                            break;
                    }
                }
                    
                AddEdges();
                
                if(haveEnemies) Generate2DSideScrollingEnemiesWithRL();
                if(havePickups) GenerateHealthPickupsWithRL();
                if(haveHazards) GenerateStageHazardsWithRL();
                if(haveCoins) Generate2DSideScrollingCoinsWithRL();
                
                Debug.Log("Training complete");
        
                for (int x = 0; x < LevelWidth; x++)
                {
                    for (int y = 0; y < LevelHeight; y++)
                    {   
                        if (_tileGrid[x, y] == TileType.Enemy) _enemyCount++;
                        if (_tileGrid[x, y] == TileType.Coin) _coinCount++;
                    }
                }
        
                Debug.Log("Enemy count: " + _enemyCount);
                Debug.Log("Coin count: " + _coinCount);
        
                InstantiateBlocks();
                InstantiateEnemies();
                InstantiateHazards();
                PlaceDeathPlane();
                InstantiatePickups();

                var gridCoordPlayerSpawn = SetPlayerSpawnPoint();

                Vector3 worldSpawnPoint = GridCoordinatesToWorldCoordinates(gridCoordPlayerSpawn.x, gridCoordPlayerSpawn.y);
        
                _actualPlayer.transform.position = worldSpawnPoint;
                break;
            
            case GenerationMode.TopDown:
                
                if (TopDownPlayer == null)
                {
                    Debug.Log("No Top Down Player Object found. Stopping Generation");
                    return;
                }

                if (TopDownPlayer == null)
                {
                    Debug.Log("Attention! No Side Scroller Player Object found. Generation for Top Down level can still continue.");
                }

                _actualPlayer = TopDownPlayer;
                if (SideScrollerPlayer) SideScrollerPlayer.SetActive(false);
                
                
                for (int i = 0; i < TrainingEpisodes; i++)
                {
                    Generate2DTopDownTerrainWithRL();
                    if(haveEnemies) Generate2DTopDownEnemiesWithRL();
                    if(havePickups) GenerateTopDownHealthPickupsWithRL();
                    if(haveHazards) GenerateTopDownStageHazardsWithRL();
                    if(haveCoins) GenerateTopDownCoinsWithRL();
                }
        
                Generate2DTopDownTerrainWithRL();
                if(haveEnemies) Generate2DTopDownEnemiesWithRL();
                if(havePickups) GenerateTopDownHealthPickupsWithRL();
                if(haveHazards) GenerateTopDownStageHazardsWithRL();
                if(haveCoins) GenerateTopDownCoinsWithRL();
        
                Debug.Log("Training complete");
                
                for (int x = 0; x < LevelWidth; x++)
                {
                    for (int y = 0; y < LevelHeight; y++)
                    {   
                        if (_tileGrid[x, y] == TileType.Enemy) _enemyCount++;
                        if (_tileGrid[x, y] == TileType.Coin) _coinCount++;
                    }
                }
        
                Debug.Log("Enemy count: " + _enemyCount);
                Debug.Log("Coin count: " + _coinCount);
        
                InstantiateBlocks();
                InstantiateEnemies();
                InstantiateHazards();
                InstantiatePickups();
        
                gridCoordPlayerSpawn = SetTopDownPlayerSpawnPoint();

                worldSpawnPoint = GridCoordinatesToWorldCoordinates(gridCoordPlayerSpawn.x, gridCoordPlayerSpawn.y);
        
                _actualPlayer.transform.position = worldSpawnPoint;
                
                break;
        }
    }
    private void Generate2DSideScrollingTerrainWithRL()
    {
        List<(GeneratorState, GeneratorAction)> episode = new();

        int groundHeight = _baseGroundHeight;
        bool inGap = false;

        _groundHeight = new int[LevelWidth];
        _tileGrid = new TileType[LevelWidth, LevelHeight];
        _decorationGrid = new TileType[LevelWidth, LevelHeight];

        int x = 0;

        while (x < LevelWidth)
        {
            GeneratorState state = new GeneratorState
            {
                X = x,
                GroundHeight = groundHeight,
                InGap = inGap
            };

            bool[] allowedAction =
            {
                true,
                true,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                false,
                false
            };
            
            GeneratorAction action = _sideScrollingterrainAgent.Decide(state, allowedAction);
            episode.Add((state, action));

            int segmentLength = Random.Range(5, 10);

            switch (action)
            {
                case GeneratorAction.Flat:
                {
                    inGap = false;

                    for (int i = 0; i < segmentLength && x < LevelWidth; i++, x++)
                        PlaceGround(x, groundHeight);

                    break;
                }

                case GeneratorAction.SlopeUp:
                {
                    inGap = false;

                    for (int i = 0; i < segmentLength && x < LevelWidth; i++, x++)
                    {
                        groundHeight = Mathf.Clamp(groundHeight + 1, 1, LevelHeight - 5);
                        PlaceGround(x, groundHeight);
                    }

                    break;
                }

                case GeneratorAction.SlopeDown:
                {
                    inGap = false;

                    for (int i = 0; i < segmentLength && x < LevelWidth; i++, x++)
                    {
                        groundHeight = Mathf.Clamp(groundHeight - 1, 1, LevelHeight - 5);
                        PlaceGround(x, groundHeight);
                    }

                    break;
                }

                case GeneratorAction.Gap:
                {
                    inGap = true;
                    int gapSize = Random.Range(2, 6);

                    for (int i = 0; i < gapSize && x < LevelWidth; i++, x++)
                    {
                        for (int y = 0; y < LevelHeight; y++)
                            _tileGrid[x, y] = TileType.Empty;

                        _groundHeight[x] = -1;
                    }

                    break;
                }
            }
        }

        float reward = Evaluate2DSideScrollingTerrain();
        _sideScrollingterrainAgent.Learn(episode, reward);
    }
    
    private void GeneratePlatformsWithRLLessOnHigherDifficult()
    {
        List<(GeneratorState, GeneratorAction)> episode = new();

        int cooldown = 0;

        for (int x = 0; x < LevelWidth; x++)
        {
            cooldown = Mathf.Max(0, cooldown - 1);

            int ground = _groundHeight[x];
            if (ground < 0) continue;

            GeneratorState state = new GeneratorState
            {
                X = x,
                GroundHeight = ground
            };
            
            bool[] allowedAction =
            {
                false,
                false,
                false,
                false,
                true,
                false,
                false,
                false,
                false,
                false,
                false
                
            };

            GeneratorAction action = _platformAgent.Decide(state, allowedAction);
            episode.Add((state, action));

            if (action == GeneratorAction.Platform && cooldown == 0)
            {
                int length = Random.Range(4, 7);

                if (TryPlacePlatform(x, ground, length))
                {
                    cooldown = length + Random.Range(3, 8);
                }
            }
        }

        float reward = EvaluatePlatforms();
        _platformAgent.Learn(episode, reward);
    }

    private void Generate2DSideScrollingEnemiesWithRL()
    {
        List<(GeneratorState, GeneratorAction)> episode = new();

        int cooldown = 0;
        int lastEnemyX = -999;

        // PASS 1 — Platforms first
        bool platformHasEnemy = false;

        for (int x = 0; x < LevelWidth; x++)
        {
            cooldown = Mathf.Max(0, cooldown - 1);

            bool foundPlatformTile = false;

            for (int y = 0; y < LevelHeight - 1; y++)
            {
                if (_tileGrid[x, y] != TileType.Platform)
                    continue;

                foundPlatformTile = true;

                if (_tileGrid[x, y + 1] != TileType.Empty)
                    break;

                GeneratorState state = new GeneratorState
                {
                    X = x,
                    GroundHeight = _groundHeight[x],
                    InGap = false,
                    DistanceFromLastEnemy = Mathf.Min(10, x - lastEnemyX),
                    DifficultyBucket = Mathf.RoundToInt(Difficulty * 3f)
                };

                bool[] allowedAction =
                {
                    false,
                    false,
                    false,
                    false,
                    false,
                    true,
                    true,
                    false,
                    false,
                    false,
                    false
                };

                GeneratorAction action = _enemyAgent.Decide(state, allowedAction);

                if (!platformHasEnemy && action == GeneratorAction.Enemy && cooldown == 0 && (_decorationGrid[x, y + 1] != TileType.EdgeLeft || _decorationGrid[x, y + 1] != TileType.EdgeRight))
                {
                    _tileGrid[x, y + 1] = TileType.Enemy;
                    cooldown = Mathf.RoundToInt(Mathf.Lerp(8f, 0f, Difficulty));
                    lastEnemyX = x;
                    platformHasEnemy = true;
                }

                episode.Add((state, action));
                break;
            }
            
            if (!foundPlatformTile)
                platformHasEnemy = false;
        }
        
        for (int x = 0; x < LevelWidth; x++)
        {
            cooldown = Mathf.Max(0, cooldown - 1);

            for (int y = 0; y < LevelHeight - 1; y++)
            {
                if (_tileGrid[x, y] != TileType.Ground)
                    continue;

                if (_tileGrid[x, y + 1] != TileType.Empty)
                    continue;

                GeneratorState state = new GeneratorState
                {
                    X = x,
                    GroundHeight = _groundHeight[x],
                    InGap = false,
                    DistanceFromLastEnemy = Mathf.Min(10, x - lastEnemyX),
                    DifficultyBucket = Mathf.RoundToInt(Difficulty * 3f)
                };

                bool[] allowedAction =
                {
                    false,
                    false,
                    false,
                    false,
                    false,
                    true,
                    true,
                    false,
                    false,
                    false,
                    false
                };

                GeneratorAction action = _enemyAgent.Decide(state, allowedAction);

                if (action == GeneratorAction.Enemy && cooldown == 0 && (_decorationGrid[x, y + 1] != TileType.EdgeLeft || _decorationGrid[x, y + 1] != TileType.EdgeRight))
                {
                    _tileGrid[x, y + 1] = TileType.Enemy;
                    cooldown = Mathf.RoundToInt(Mathf.Lerp(8f, 2f, Difficulty));
                    lastEnemyX = x;
                }
                else if (action == GeneratorAction.None)
                {
                    _tileGrid[x, y + 1] = TileType.Empty;
                }

                episode.Add((state, action));
                break;
            }
        }

        float reward = Evaluate2DSideScrollingEnemies();
        _enemyAgent.Learn(episode, reward);
    }

    private void Generate2DTopDownTerrainWithRL()
    {
        List<(GeneratorState, GeneratorAction)> episode = new();

        _tileGrid = new TileType[LevelWidth, LevelHeight];
        _decorationGrid = new TileType[LevelWidth, LevelHeight];

        // Border walls
        for (int x = 0; x < LevelWidth; x++)
        {
            for (int y = 0; y < LevelHeight; y++)
            {
                bool isEdge =
                    x == 0 ||
                    y == 0 ||
                    x == LevelWidth - 1 ||
                    y == LevelHeight - 1;

                _tileGrid[x, y] =
                    isEdge
                        ? TileType.Ground
                        : TileType.Empty;
            }
        }
        
        for (int x = 1; x < LevelWidth - 1; x++)
        {
            for (int y = 1; y < LevelHeight - 1; y++)
            {
                GeneratorState state = new GeneratorState
                {
                    X = x,
                    GroundHeight = y
                };

                bool[] allowedActions =
                {
                    false, // Flat
                    false, // SlopeUp
                    false, // SlopeDown
                    false, // Gap
                    false, // Platform
                    false, // Enemy
                    true,  // None
                    true,  // Wall
                    false, // Coin
                    false, //Pickup
                    false  //Hazard
                };

                GeneratorAction action =
                    _topDownAgent.Decide(state, allowedActions);

                episode.Add((state, action));

                if (action != GeneratorAction.Wall)
                    continue;

                bool horizontal = Random.value < 0.5f;

                int minLength = Mathf.RoundToInt(
                    Mathf.Lerp(2f, 4f, Difficulty)
                );

                int maxLength = Mathf.RoundToInt(
                    Mathf.Lerp(4f, 12f, Difficulty)
                );

                int length = Random.Range(minLength, maxLength);

                bool canPlace = true;
                
                for (int i = 0; i < length; i++)
                {
                    int wx = x + (horizontal ? i : 0);
                    int wy = y + (horizontal ? 0 : i);
                    
                    if (wx <= 0 || wx >= LevelWidth - 1 ||
                        wy <= 0 || wy >= LevelHeight - 1)
                    {
                        canPlace = false;
                        break;
                    }
                    
                    if (_tileGrid[wx, wy] != TileType.Empty)
                    {
                        canPlace = false;
                        break;
                    }
                    
                    int nearbyWalls = 0;

                    for (int ox = -2; ox <= 2; ox++)
                    {
                        for (int oy = -2; oy <= 2; oy++)
                        {
                            int nx = wx + ox;
                            int ny = wy + oy;

                            if (nx < 0 || nx >= LevelWidth ||
                                ny < 0 || ny >= LevelHeight)
                                continue;

                            if (_tileGrid[nx, ny] ==
                                TileType.Ground)
                            {
                                nearbyWalls++;
                            }
                        }
                    }

                    int maxNearbyWalls =
                        Mathf.RoundToInt(
                            Mathf.Lerp(2f, 10f, Difficulty)
                        );

                    if (nearbyWalls > maxNearbyWalls)
                    {
                        canPlace = false;
                        break;
                    }
                }

                if (!canPlace)
                    continue;
                
                List<Vector2Int> placedTiles = new();

                for (int i = 0; i < length; i++)
                {
                    int wx = x + (horizontal ? i : 0);
                    int wy = y + (horizontal ? 0 : i);

                    _tileGrid[wx, wy] = TileType.Ground;

                    placedTiles.Add(new Vector2Int(wx, wy));
                }
                
                if (!IsTopDownMapConnected())
                {
                    foreach (Vector2Int tile in placedTiles)
                    {
                        _tileGrid[tile.x, tile.y] =
                            TileType.Empty;
                    }
                }
            }
        }

        float reward = EvaluateTopDownTerrain();

        if (!IsTopDownMapConnected())
            reward -= 500f;

        _topDownAgent.Learn(episode, reward);
    }
    
    private void Generate2DTopDownEnemiesWithRL()
{
    List<(GeneratorState, GeneratorAction)> episode = new();

    int cooldown = 0;
    int lastEnemyX = -999;
    int enemyCount = 0;

    for (int x = 1; x < LevelWidth - 1; x++)
    {
        for (int y = 1; y < LevelHeight - 1; y++)
        {
            cooldown = Mathf.Max(0, cooldown - 1);
            
            if (_tileGrid[x, y] != TileType.Empty)
                continue;
            
            if (x <= 1 || x >= LevelWidth - 2 ||
                y <= 1 || y >= LevelHeight - 2)
                continue;
            
            int nearbyWalls = 0;

            if (_tileGrid[x + 1, y] == TileType.Ground) nearbyWalls++;
            if (_tileGrid[x - 1, y] == TileType.Ground) nearbyWalls++;
            if (_tileGrid[x, y + 1] == TileType.Ground) nearbyWalls++;
            if (_tileGrid[x, y - 1] == TileType.Ground) nearbyWalls++;

            if (nearbyWalls >= 3)
                continue;

            GeneratorState state = new GeneratorState
            {
                X = x,
                GroundHeight = y,
                DistanceFromLastEnemy =
                    Mathf.Min(15, Mathf.Abs(x - lastEnemyX)),
                DifficultyBucket =
                    Mathf.RoundToInt(Difficulty * 3f)
            };

            bool[] allowedActions =
            {
                false, // Flat
                false, // SlopeUp
                false, // SlopeDown
                false, // Gap
                false, // Platform
                true,  // Enemy
                true,  // None
                false, // Wall
                false, // Coin
                false, //Pickup
                false  //Hazard
            };

            GeneratorAction action =
                _enemyAgent.Decide(state, allowedActions);

            if (action == GeneratorAction.Enemy &&
                cooldown == 0)
            {
                bool tooClose = false;

                for (int ox = -2; ox <= 2; ox++)
                {
                    for (int oy = -2; oy <= 2; oy++)
                    {
                        int nx = x + ox;
                        int ny = y + oy;

                        if (nx < 0 || nx >= LevelWidth ||
                            ny < 0 || ny >= LevelHeight)
                            continue;

                        if (_tileGrid[nx, ny] ==
                            TileType.Enemy)
                        {
                            tooClose = true;
                            break;
                        }
                    }

                    if (tooClose)
                        break;
                }

                if (!tooClose)
                {
                    _tileGrid[x, y] =
                        TileType.Enemy;

                    enemyCount++;

                    cooldown =
                        Mathf.RoundToInt(
                            Mathf.Lerp(10f, 2f, Difficulty)
                        );

                    lastEnemyX = x;
                }
            }

            episode.Add((state, action));
        }
    }

    float reward = EvaluateTopDownEnemies(enemyCount);

    _enemyAgent.Learn(episode, reward);
}
    
    private void Generate2DSideScrollingCoinsWithRL()
    {
        List<(GeneratorState, GeneratorAction)> episode = new();

        int cooldown = 0;
        int lastPickupX = -999;
        
        for (int x = 0; x < LevelWidth; x++)
        {
            cooldown = Mathf.Max(0, cooldown - 1);

            for (int y = 0; y < LevelHeight - 1; y++)
            {
                if (_tileGrid[x, y] != TileType.Platform)
                    continue;

                if (_tileGrid[x, y + 1] != TileType.Empty)
                    break;

                GeneratorState state = new GeneratorState
                {
                    X = x,
                    GroundHeight = _groundHeight[x],
                    InGap = false,
                    DistanceFromLastEnemy = Mathf.Min(10, x - lastPickupX),
                    DifficultyBucket = Mathf.RoundToInt(Difficulty * 3f)
                };

                bool[] allowedAction =
                {
                    false, // Flat
                    false, // SlopeUp
                    false, // SlopeDown
                    false, // Gap
                    false, // Platform
                    false, // Enemy
                    true,  // None
                    false, // Wall
                    true,  // Coin
                    false, // Pickup
                    false  // Hazard
                };

                GeneratorAction action = _collectiblesAgent.Decide(state, allowedAction);

                if (action == GeneratorAction.Coin &&
                    cooldown == 0 &&
                    _decorationGrid[x, y + 1] != TileType.EdgeLeft &&
                    _decorationGrid[x, y + 1] != TileType.EdgeRight)
                {
                    int nearbyPickups = 0;
                    
                    for (int px = Mathf.Max(0, x - 2);
                         px <= Mathf.Min(LevelWidth - 1, x + 2);
                         px++)
                    {
                        if (_tileGrid[px, y + 1] == TileType.Coin)
                            nearbyPickups++;
                    }
                    
                    if (nearbyPickups < 2)
                    {
                        _tileGrid[x, y + 1] = TileType.Coin;

                        cooldown = Mathf.RoundToInt(
                            Mathf.Lerp(3f, 1f, Difficulty)
                        );

                        lastPickupX = x;
                    }
                }

                episode.Add((state, action));
                break;
            }
        }
        
        for (int x = 0; x < LevelWidth; x++)
        {
            cooldown = Mathf.Max(0, cooldown - 1);

            for (int y = 0; y < LevelHeight - 1; y++)
            {
                if (_tileGrid[x, y] != TileType.Ground)
                    continue;

                if (_tileGrid[x, y + 1] != TileType.Empty)
                    continue;

                GeneratorState state = new GeneratorState
                {
                    X = x,
                    GroundHeight = _groundHeight[x],
                    InGap = false,
                    DistanceFromLastEnemy = Mathf.Min(10, x - lastPickupX),
                    DifficultyBucket = Mathf.RoundToInt(Difficulty * 3f)
                };

                bool[] allowedAction =
                {
                    false, // Flat
                    false, // SlopeUp
                    false, // SlopeDown
                    false, // Gap
                    false, // Platform
                    false, // Enemy
                    true,  // None
                    false, // Wall
                    true,  // Coin
                    false, // Pickup
                    false  // Hazard
                };

                GeneratorAction action = _collectiblesAgent.Decide(state, allowedAction);

                if (action == GeneratorAction.Coin &&
                    cooldown == 0 &&
                    _decorationGrid[x, y + 1] != TileType.EdgeLeft &&
                    _decorationGrid[x, y + 1] != TileType.EdgeRight)
                {
                    int nearbyPickups = 0;

                    for (int px = Mathf.Max(0, x - 2);
                         px <= Mathf.Min(LevelWidth - 1, x + 2);
                         px++)
                    {
                        if (_tileGrid[px, y + 1] == TileType.Coin)
                            nearbyPickups++;
                    }

                    if (nearbyPickups < 2)
                    {
                        _tileGrid[x, y + 1] = TileType.Coin;

                        cooldown = Mathf.RoundToInt(
                            Mathf.Lerp(4f, 1f, Difficulty)
                        );

                        lastPickupX = x;
                    }
                }
                else if (action == GeneratorAction.None)
                {
                    _tileGrid[x, y + 1] = TileType.Empty;
                }

                episode.Add((state, action));
                break;
            }
        }

        float reward = Evaluate2DSideScrollingPickups();
        _collectiblesAgent.Learn(episode, reward);
    }
    
    private void GenerateTopDownCoinsWithRL()
    {
        List<(GeneratorState, GeneratorAction)> episode = new();

        int cooldown = 0;
        int lastCoinX = -999;

        for (int x = 1; x < LevelWidth - 1; x++)
        {
            for (int y = 1; y < LevelHeight - 1; y++)
            {
                cooldown = Mathf.Max(0, cooldown - 1);

                if (_tileGrid[x, y] != TileType.Empty)
                    continue;

                int nearbyWalls = 0;

                if (_tileGrid[x + 1, y] == TileType.Ground) nearbyWalls++;
                if (_tileGrid[x - 1, y] == TileType.Ground) nearbyWalls++;
                if (_tileGrid[x, y + 1] == TileType.Ground) nearbyWalls++;
                if (_tileGrid[x, y - 1] == TileType.Ground) nearbyWalls++;

                if (nearbyWalls >= 3)
                    continue;

                GeneratorState state = new GeneratorState
                {
                    X = x,
                    GroundHeight = y,
                    InGap = false,
                    DistanceFromLastEnemy = Mathf.Min(15, x - lastCoinX),
                    DifficultyBucket = Mathf.RoundToInt(Difficulty * 3f)
                };

                bool[] allowedActions =
                {
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    true, 
                    false,
                    true, 
                    false,
                    false
                };

                GeneratorAction action =
                    _collectiblesAgent.Decide(state, allowedActions);

                if (action == GeneratorAction.Coin && cooldown == 0)
                {
                    int nearbyCoins = 0;

                    for (int ox = -2; ox <= 2; ox++)
                    {
                        for (int oy = -2; oy <= 2; oy++)
                        {
                            int nx = x + ox;
                            int ny = y + oy;

                            if (nx < 0 || nx >= LevelWidth ||
                                ny < 0 || ny >= LevelHeight)
                                continue;

                            if (_tileGrid[nx, ny] == TileType.Coin)
                                nearbyCoins++;
                        }
                    }

                    if (nearbyCoins < 3)
                    {
                        _tileGrid[x, y] = TileType.Coin;

                        lastCoinX = x;

                        cooldown = Mathf.RoundToInt(
                            Mathf.Lerp(5f, 1f, Difficulty)
                        );
                    }
                }

                episode.Add((state, action));
            }
        }

        float reward = EvaluateTopDownCoins();
        _collectiblesAgent.Learn(episode, reward);
    }
    
    private void AddEdges()
    {
        for (int x = 0; x < LevelWidth; x++)
        {
            for (int y = 0; y < LevelHeight; y++)
            {
                if (_tileGrid[x, y] == TileType.Ground)
                {
                    bool leftGap = (x == 0) || _groundHeight[x - 1] < 0;
                    bool rightGap = (x == LevelWidth - 1) || _groundHeight[x + 1] < 0;

                    if (leftGap && y == _groundHeight[x])
                    {
                        _decorationGrid[x, y + 1] = TileType.EdgeLeft;
                    }

                    if (rightGap && y == _groundHeight[x])
                    {
                        _decorationGrid[x, y + 1] = TileType.EdgeRight;
                    }
                }
                
                if (_tileGrid[x, y] == TileType.Platform)
                {
                    bool leftEmpty = (x == 0) || _tileGrid[x - 1, y] != TileType.Platform;
                    bool rightEmpty = (x == LevelWidth - 1) || _tileGrid[x + 1, y] != TileType.Platform;

                    if (leftEmpty)
                    {
                        if (y + 1 < LevelHeight) _decorationGrid[x, y + 1] = TileType.EdgeLeft;
                    }

                    if (rightEmpty)
                    {
                        if (y + 1 < LevelHeight) _decorationGrid[x, y + 1] = TileType.EdgeRight;
                    }
                }
            }
        }
    }
    
    private void PlaceGround(int x, int height)
    {
        _groundHeight[x] = height;

        for (int y = 0; y <= height; y++)
        {
            _tileGrid[x, y] = TileType.Ground;
        }
    }
    
    private bool TryPlacePlatform(int startX, int groundHeight, int length)
    {
        if (startX + length >= LevelWidth)
            return false;

        int y = groundHeight + Random.Range(MinPlatformHeightFromGround, MaxPlatformHeightFromGround + 1);

        if (!IsAreaClear(startX, y, length))
            return false;
        if (!IsPlatformStructurallyValid(startX, y, length))
            return false;

        for (int i = 0; i < length; i++)
        {
            _tileGrid[startX + i, y] = TileType.Platform;
        }

        return true;
    }
    
    private bool IsAreaClear(int startX, int y, int length)
    {
        if (y < 0 || y >= LevelHeight)
            return false;

        for (int i = 0; i < length; i++)
        {
            int x = startX + i;

            if (x < 0 || x >= LevelWidth)
                return false;
            
            if (_tileGrid[x, y] != TileType.Empty)
                return false;
        }
        return true;
    }
    
    private bool IsPlatformStructurallyValid(int startX, int y, int length)
    {
        int minClearance = 3;

        for (int i = 0; i < length; i++)
        {
            int x = startX + i;

            if (x < 0 || x >= LevelWidth)
                return false;
            
            for (int checkY = y - 1; checkY >= y - minClearance; checkY--)
            {
                if (checkY < 0)
                    return false;

                if (_tileGrid[x, checkY] != TileType.Empty)
                    return false;
            }
        }

        for (int x = startX; x < startX + length; x++)
        {
            if (x <= 0 || x >= LevelWidth)
                continue;

            int diff = Mathf.Abs(_groundHeight[x] - _groundHeight[x - 1]);
            if (diff > 1)
                return false;
        }

        return true;
    }
    
    private bool IsValidTopDownHazardPosition(int x, int y)
    {
        bool left  = IsBlocked(x - 1, y);
        bool right = IsBlocked(x + 1, y);
        bool up    = IsBlocked(x, y + 1);
        bool down  = IsBlocked(x, y - 1);

        bool upLeft    = IsBlocked(x - 1, y + 1);
        bool upRight   = IsBlocked(x + 1, y + 1);
        bool downLeft  = IsBlocked(x - 1, y - 1);
        bool downRight = IsBlocked(x + 1, y - 1);

        if (left && right)
            return false;

        if (up && down)
            return false;

        if (left && up)
            return false;

        if (left && down)
            return false;

        if (right && up)
            return false;

        if (right && down)
            return false;

        if (upLeft && !left && !up)
            return false;

        if (upRight && !right && !up)
            return false;

        if (downLeft && !left && !down)
            return false;

        if (downRight && !right && !down)
            return false;

        return true;
    }
    
    private bool IsBlocked(int checkX, int checkY)
    {
        if (checkX < 0 || checkX >= LevelWidth ||
            checkY < 0 || checkY >= LevelHeight)
            return true;

        return _tileGrid[checkX, checkY] == TileType.Ground;
    }

    private void AdjustEnemyDifficulty(EnemyDifficultyAdjuster eda)
    {
        float targetHp = Mathf.Lerp(30, 90, Difficulty);
        float targetSpeed = Mathf.Lerp(1, 2, Difficulty);

        eda.MaxHealthPoints = targetHp;
        eda.MovementSpeed = targetSpeed;
    }
    
    private void GeneratePlatformsWithRLMoreOnHigherDifficult()
    {
        List<(GeneratorState, GeneratorAction)> episode = new();

        int cooldown = 0;

        for (int x = 0; x < LevelWidth; x++)
        {
            cooldown = Mathf.Max(0, cooldown - 1);

            int ground = _groundHeight[x];

            if (ground < 0)
                continue;

            GeneratorState state = new GeneratorState
            {
                X = x,
                GroundHeight = ground,
                DifficultyBucket = Mathf.RoundToInt(Difficulty * 3f)
            };

            bool[] allowedAction =
            {
                false,
                false,
                false,
                false,
                true,
                false,
                false,
                false,
                false,
                false, 
                false  
            };

            GeneratorAction action =
                _platformAgent.Decide(state, allowedAction);

            episode.Add((state, action));

            if (action == GeneratorAction.Platform &&
                cooldown == 0)
            {
                int length = Random.Range(3, 8);

                if (TryPlaceCollectathonPlatform(x, ground, length))
                {
                    cooldown = Mathf.RoundToInt(
                        Mathf.Lerp(8f, 2f, Difficulty)
                    );
                }
            }
        }

        float reward = EvaluateCollectathonPlatforms();

        _platformAgent.Learn(episode, reward);
    }
    
    private bool TryPlaceCollectathonPlatform(int startX, int groundHeight, int length)
    {
        if (startX + length >= LevelWidth)
            return false;
        
        int y = groundHeight + Random.Range(MinPlatformHeightFromGround, MaxPlatformHeightFromGround + 1);

        if (!IsAreaClear(startX, y, length))
            return false;

        if (!IsCollectathonPlatformValid(startX, y, length))
            return false;

        for (int i = 0; i < length; i++)
        {
            _tileGrid[startX + i, y] =
                TileType.Platform;
        }

        return true;
    }
    
    private bool IsCollectathonPlatformValid(int startX, int y, int length)
    {
        int minClearanceBelow = 2;
        int minClearanceAbove = 3;

        for (int i = 0; i < length; i++)
        {
            int x = startX + i;

            if (x < 0 || x >= LevelWidth)
                return false;

            for (int checkY = y - 1;
                 checkY >= y - minClearanceBelow;
                 checkY--)
            {
                if (checkY < 0)
                    return false;

                if (_tileGrid[x, checkY] != TileType.Empty)
                    return false;
            }

            for (int checkY = y + 1;
                 checkY <= y + minClearanceAbove;
                 checkY++)
            {
                if (checkY >= LevelHeight)
                    break;

                if (_tileGrid[x, checkY] == TileType.Platform)
                    return false;
            }

            for (int offsetX = -1; offsetX <= 1; offsetX++)
            {
                int nx = x + offsetX;

                if (nx < 0 || nx >= LevelWidth)
                    continue;

                for (int offsetY = -2; offsetY <= 2; offsetY++)
                {
                    int ny = y + offsetY;

                    if (ny < 0 || ny >= LevelHeight)
                        continue;
                    
                    if (nx == x && ny == y)
                        continue;

                    if (_tileGrid[nx, ny] == TileType.Platform)
                    {
                        if (Mathf.Abs(offsetY) <= 2) return false;
                    }
                }
            }
        }
        
        for (int x = startX;
             x < startX + length;
             x++)
        {
            if (x <= 0 || x >= LevelWidth)
                continue;

            int diff = Mathf.Abs(_groundHeight[x] - _groundHeight[x - 1]);

            int allowedSlope =
                Mathf.RoundToInt(
                    Mathf.Lerp(2f, 4f, Difficulty)
                );

            if (diff > allowedSlope)
                return false;
        }

        int requiredPlayerSpace = 3;

        for (int x = startX; x < startX + length; x++)
        {
            int left = Mathf.Max(0, x - 1);
            int right = Mathf.Min(LevelWidth - 1, x + 1);

            for (int checkX = left; checkX <= right; checkX++)
            {
                int terrainHeight = _groundHeight[checkX];

                if (terrainHeight < 0)
                    continue;

                int clearance = y - terrainHeight;

                if (clearance <= requiredPlayerSpace)
                    return false;
            }
        }
        
        return true;
    }
    
    private bool IsTopDownMapConnected()
    {
        bool[,] visited = new bool[LevelWidth, LevelHeight];

        Queue<Vector2Int> queue = new();
        
        for (int x = 1; x < LevelWidth - 1; x++)
        {
            for (int y = 1; y < LevelHeight - 1; y++)
            {
                if (_tileGrid[x, y] == TileType.Empty)
                {
                    queue.Enqueue(new Vector2Int(x, y));
                    visited[x, y] = true;
                    goto START_FILL;
                }
            }
        }

        START_FILL:

        int reachable = 0;
        int totalFloor = 0;

        for (int x = 1; x < LevelWidth - 1; x++)
        {
            for (int y = 1; y < LevelHeight - 1; y++)
            {
                if (_tileGrid[x, y] == TileType.Empty)
                    totalFloor++;
            }
        }

        Vector2Int[] dirs =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        while (queue.Count > 0)
        {
            Vector2Int p = queue.Dequeue();

            reachable++;

            foreach (var d in dirs)
            {
                int nx = p.x + d.x;
                int ny = p.y + d.y;

                if (nx <= 0 || nx >= LevelWidth - 1 ||
                    ny <= 0 || ny >= LevelHeight - 1)
                    continue;

                if (visited[nx, ny])
                    continue;

                if (_tileGrid[nx, ny] != TileType.Empty)
                    continue;

                visited[nx, ny] = true;
                queue.Enqueue(new Vector2Int(nx, ny));
            }
        }

        return reachable == totalFloor;
    }
    
    private void GenerateHealthPickupsWithRL()
    {
        List<(GeneratorState, GeneratorAction)> episode = new();

        int cooldown = 0;
        int lastPickupX = -999;

        bool platformHasPickup = false;
        
        for (int x = 0; x < LevelWidth; x++)
        {
            cooldown = Mathf.Max(0, cooldown - 1);

            bool foundPlatformTile = false;

            for (int y = 0; y < LevelHeight - 2; y++)
            {
                if (_tileGrid[x, y] != TileType.Platform)
                    continue;

                foundPlatformTile = true;
                
                if (_tileGrid[x, y + 1] != TileType.Empty)
                    break;

                if (_tileGrid[x, y + 2] != TileType.Empty)
                    break;

                GeneratorState state = new GeneratorState
                {
                    X = x,
                    GroundHeight = _groundHeight[x],
                    InGap = false,
                    DistanceFromLastEnemy =
                        Mathf.Min(10, x - lastPickupX),
                    DifficultyBucket =
                        Mathf.RoundToInt(Difficulty * 3f)
                };

                bool[] allowedActions =
                {
                    false, // Flat
                    false, // SlopeUp
                    false, // SlopeDown
                    false, // Gap
                    false, // Platform
                    false, // Enemy
                    true,  // None
                    false, // Wall
                    false, // Coin
                    true,  // Pickup
                    false  // Hazard
                };

                GeneratorAction action =
                    _pickUpAgent.Decide(state, allowedActions);

                if (!platformHasPickup &&
                    action == GeneratorAction.Pickup &&
                    cooldown == 0)
                {
                    if (_decorationGrid[x, y] != TileType.EdgeLeft &&
                        _decorationGrid[x, y] != TileType.EdgeRight)
                    {
                        _tileGrid[x, y + 2] =
                            TileType.Pickup;

                        platformHasPickup = true;

                        lastPickupX = x;
                        
                        cooldown = Mathf.RoundToInt(
                            Mathf.Lerp(2f, 10f, Difficulty)
                        );
                    }
                }

                episode.Add((state, action));

                break;
            }
            
            if (!foundPlatformTile) platformHasPickup = false;
        }
        
        for (int x = 0; x < LevelWidth; x++)
        {
            cooldown = Mathf.Max(0, cooldown - 1);

            for (int y = 0; y < LevelHeight - 2; y++)
            {
                if (_tileGrid[x, y] != TileType.Ground)
                    continue;
                
                if (_tileGrid[x, y + 1] != TileType.Empty)
                    continue;

                if (_tileGrid[x, y + 2] != TileType.Empty)
                    continue;

                GeneratorState state = new GeneratorState
                {
                    X = x,
                    GroundHeight = _groundHeight[x],
                    InGap = false,
                    DistanceFromLastEnemy =
                        Mathf.Min(10, x - lastPickupX),
                    DifficultyBucket =
                        Mathf.RoundToInt(Difficulty * 3f)
                };

                bool[] allowedActions =
                {
                    false, // Flat
                    false, // SlopeUp
                    false, // SlopeDown
                    false, // Gap
                    false, // Platform
                    false, // Enemy
                    true,  // None
                    false, // Wall
                    false, // Coin
                    true,  // Pickup
                    false  // Hazard
                };

                GeneratorAction action =
                    _pickUpAgent.Decide(state, allowedActions);

                if (action == GeneratorAction.Pickup &&
                    cooldown == 0)
                {
                    if (_decorationGrid[x, y] != TileType.EdgeLeft &&
                        _decorationGrid[x, y] != TileType.EdgeRight)
                    {
                        
                        _tileGrid[x, y + 2] = TileType.Pickup;

                        lastPickupX = x;
                        
                        cooldown = Mathf.RoundToInt(
                            Mathf.Lerp(2f, 12f, Difficulty)
                        );
                    }
                }

                episode.Add((state, action));

                break;
            }
        }

        float reward = EvaluateHealthPickups();

        _pickUpAgent.Learn(episode, reward);
    }
    
    private void GenerateTopDownHealthPickupsWithRL()
    {
        List<(GeneratorState, GeneratorAction)> episode = new();

        int cooldown = 0;
        int lastPickupX = -999;

        for (int x = 1; x < LevelWidth - 1; x++)
        {
            for (int y = 1; y < LevelHeight - 1; y++)
            {
                cooldown = Mathf.Max(0, cooldown - 1);
                
                if (_tileGrid[x, y] != TileType.Empty)
                    continue;
                
                int nearbyWalls = 0;

                if (_tileGrid[x + 1, y] == TileType.Ground)
                    nearbyWalls++;

                if (_tileGrid[x - 1, y] == TileType.Ground)
                    nearbyWalls++;

                if (_tileGrid[x, y + 1] == TileType.Ground)
                    nearbyWalls++;

                if (_tileGrid[x, y - 1] == TileType.Ground)
                    nearbyWalls++;
                
                if (nearbyWalls >= 3)
                    continue;

                GeneratorState state = new GeneratorState
                {
                    X = x,
                    GroundHeight = y,
                    InGap = false,
                    DistanceFromLastEnemy = Mathf.Min(15, x - lastPickupX),
                    DifficultyBucket = Mathf.RoundToInt(Difficulty * 3f)
                };

                bool[] allowedActions =
                {
                    false, // Flat
                    false, // SlopeUp
                    false, // SlopeDown
                    false, // Gap
                    false, // Platform
                    false, // Enemy
                    true,  // None
                    false, // Wall
                    false, // Coin
                    true,  // Pickup
                    false  // Hazard
                };

                GeneratorAction action =
                    _pickUpAgent.Decide(state, allowedActions);

                if (action == GeneratorAction.Pickup &&
                    cooldown == 0)
                {
                    bool tooClose = false;
                    
                    for (int ox = -3; ox <= 3; ox++)
                    {
                        for (int oy = -3; oy <= 3; oy++)
                        {
                            int nx = x + ox;
                            int ny = y + oy;

                            if (nx < 0 || nx >= LevelWidth ||
                                ny < 0 || ny >= LevelHeight)
                                continue;

                            if (_tileGrid[nx, ny] ==
                                TileType.Pickup)
                            {
                                tooClose = true;
                                break;
                            }
                        }

                        if (tooClose)
                            break;
                    }

                    if (!tooClose)
                    {
                        _tileGrid[x, y] = TileType.Pickup;

                        lastPickupX = x;
                        
                        cooldown = Mathf.RoundToInt(Mathf.Lerp(2f, 12f, Difficulty));
                    }
                }

                episode.Add((state, action));
            }
        }

        float reward = EvaluateTopDownHealthPickups();

        _pickUpAgent.Learn(episode, reward);
    }
    
    private void GenerateStageHazardsWithRL()
    {
        List<(GeneratorState, GeneratorAction)> episode = new();

        int cooldown = 0;
        int lastHazardX = -999;

        bool platformHasHazard = false;

        
        for (int x = 0; x < LevelWidth; x++)
        {
            cooldown = Mathf.Max(0, cooldown - 1);

            bool foundPlatformTile = false;

            for (int y = 0; y < LevelHeight - 1; y++)
            {
                if (_tileGrid[x, y] != TileType.Platform)
                    continue;

                foundPlatformTile = true;
                
                if (_tileGrid[x, y + 1] != TileType.Empty)
                    break;

                GeneratorState state = new GeneratorState
                {
                    X = x,
                    GroundHeight = _groundHeight[x],
                    InGap = false,
                    DistanceFromLastEnemy =
                        Mathf.Min(10, x - lastHazardX),
                    DifficultyBucket =
                        Mathf.RoundToInt(Difficulty * 3f)
                };

                bool[] allowedActions =
                {
                    false, // Flat
                    false, // SlopeUp
                    false, // SlopeDown
                    false, // Gap
                    false, // Platform
                    false, // Enemy
                    true,  // None
                    false, // Wall
                    false, // Coin
                    false, // Pickup
                    true   // Hazard
                };

                GeneratorAction action =
                    _hazardAgent.Decide(state, allowedActions);

                if (!platformHasHazard &&
                    action == GeneratorAction.Hazard &&
                    cooldown == 0)
                {
                    if (_decorationGrid[x, y] != TileType.EdgeLeft &&
                        _decorationGrid[x, y] != TileType.EdgeRight)
                    {
                        _tileGrid[x, y + 1] =
                            TileType.Hazard;

                        platformHasHazard = true;

                        lastHazardX = x;
                        
                        cooldown = Mathf.RoundToInt(Mathf.Lerp(10f, 2f, Difficulty));
                    }
                }

                episode.Add((state, action));

                break;
            }
            
            if (!foundPlatformTile) platformHasHazard = false;
        }
        
        for (int x = 0; x < LevelWidth; x++)
        {
            cooldown = Mathf.Max(0, cooldown - 1);

            for (int y = 0; y < LevelHeight - 1; y++)
            {
                if (_tileGrid[x, y] != TileType.Ground)
                    continue;
                
                if (_tileGrid[x, y + 1] != TileType.Empty)
                    continue;

                GeneratorState state = new GeneratorState
                {
                    X = x,
                    GroundHeight = _groundHeight[x],
                    InGap = false,
                    DistanceFromLastEnemy = Mathf.Min(10, x - lastHazardX),
                    DifficultyBucket = Mathf.RoundToInt(Difficulty * 3f)
                };

                bool[] allowedActions =
                {
                    false, // Flat
                    false, // SlopeUp
                    false, // SlopeDown
                    false, // Gap
                    false, // Platform
                    false, // Enemy
                    true,  // None
                    false, // Wall
                    false, // Coin
                    false, // Pickup
                    true   // Hazard
                };

                GeneratorAction action =
                    _hazardAgent.Decide(state, allowedActions);

                if (action == GeneratorAction.Hazard &&
                    cooldown == 0)
                {
                    if (_decorationGrid[x, y] != TileType.EdgeLeft &&
                        _decorationGrid[x, y] != TileType.EdgeRight)
                    {
                        _tileGrid[x, y + 1] = TileType.Hazard;

                        lastHazardX = x;
                        
                        cooldown = Mathf.RoundToInt(Mathf.Lerp(12f, 3f, Difficulty));
                    }
                }

                episode.Add((state, action));

                break;
            }
        }

        float reward = EvaluateStageHazards();

        _hazardAgent.Learn(episode, reward);
    }
    
    private void GenerateTopDownStageHazardsWithRL()
    {
        List<(GeneratorState, GeneratorAction)> episode = new();

        int cooldown = 0;
        int lastHazardX = -999;

        for (int x = 1; x < LevelWidth - 1; x++)
        {
            for (int y = 1; y < LevelHeight - 1; y++)
            {
                cooldown = Mathf.Max(0, cooldown - 1);
                
                if (_tileGrid[x, y] != TileType.Empty)
                    continue;
                
                int nearbyWalls = 0;

                if (_tileGrid[x + 1, y] == TileType.Ground)
                    nearbyWalls++;

                if (_tileGrid[x - 1, y] == TileType.Ground)
                    nearbyWalls++;

                if (_tileGrid[x, y + 1] == TileType.Ground)
                    nearbyWalls++;

                if (_tileGrid[x, y - 1] == TileType.Ground)
                    nearbyWalls++;

                // Too cramped
                if (nearbyWalls >= 3)
                    continue;

                GeneratorState state = new GeneratorState
                {
                    X = x,
                    GroundHeight = y,
                    InGap = false,
                    DistanceFromLastEnemy =
                        Mathf.Min(15, x - lastHazardX),
                    DifficultyBucket =
                        Mathf.RoundToInt(Difficulty * 3f)
                };

                bool[] allowedActions =
                {
                    false, // Flat
                    false, // SlopeUp
                    false, // SlopeDown
                    false, // Gap
                    false, // Platform
                    false, // Enemy
                    true,  // None
                    false, // Wall
                    false, // Coin
                    false, // Pickup
                    true   // Hazard
                };

                GeneratorAction action = _hazardAgent.Decide(state, allowedActions);

                if (action == GeneratorAction.Hazard && cooldown == 0 && IsValidTopDownHazardPosition(x, y))
                {
                    bool tooClose = false;
                    
                    for (int ox = -3; ox <= 3; ox++)
                    {
                        for (int oy = -3; oy <= 3; oy++)
                        {
                            int nx = x + ox;
                            int ny = y + oy;

                            if (nx < 0 || nx >= LevelWidth ||
                                ny < 0 || ny >= LevelHeight)
                                continue;

                            if (_tileGrid[nx, ny] ==
                                TileType.Hazard)
                            {
                                tooClose = true;
                                break;
                            }
                        }

                        if (tooClose)
                            break;
                    }

                    if (!tooClose)
                    {
                        _tileGrid[x, y] =
                            TileType.Hazard;

                        lastHazardX = x;
                        
                        cooldown = Mathf.RoundToInt(
                            Mathf.Lerp(14f, 2f, Difficulty)
                        );
                    }
                }

                episode.Add((state, action));
            }
        }

        float reward = EvaluateTopDownStageHazards();

        _hazardAgent.Learn(episode, reward);
    }
    
    //--------- EVALUATION ------------//
    
    private float Evaluate2DSideScrollingTerrain()
    { 
        float reward = 0f;
        
        float gapTarget = Mathf.Lerp(0.08f, 0.22f, Difficulty);
        float heightVariationWeight = Mathf.Lerp(0.6f, 1.4f, Difficulty);
        float smoothnessWeight = Mathf.Lerp(1.4f, -0.4f, Difficulty);
        float plateauWeight = Mathf.Lerp(1.3f, -0.7f, Difficulty);

        int plateauLength = 1;
        int gapLength = 0;

        int minHeight = int.MaxValue;
        int maxHeight = int.MinValue;

        int gapCount = 0;

        for (int x = 0; x < LevelWidth; x++)
        {
            int h = _groundHeight[x];
            
            if (h >= 0)
            {
                minHeight = Mathf.Min(minHeight, h);
                maxHeight = Mathf.Max(maxHeight, h);
            }

            bool isGap = h < 0;

            if (isGap)
            {
                gapCount++;

                if (gapLength == 0)
                {
                    float gapStartReward = Mathf.Lerp(0.0f, 1.2f, Difficulty);
                    reward += gapStartReward;
                }

                gapLength++;
            }
            else
            {
                if (gapLength > 6)
                    reward -= (gapLength - 3) * 1.2f;

                gapLength = 0;
            }

            if (x > 0)
            {
                int prev = _groundHeight[x - 1];

                if (prev >= 0 && h >= 0)
                {
                    int diff = Mathf.Abs(h - prev);
                    
                    if (diff == 0)
                        reward += 0.15f * smoothnessWeight;
                    else if (diff == 1)
                        reward += 0.08f * smoothnessWeight;
                    
                    if (h == prev)
                    {
                        plateauLength++;
                    }
                    else
                    {
                        if (plateauLength >= 4)
                            reward += Mathf.Log(plateauLength + 1) * 0.4f * plateauWeight;;

                        plateauLength = 1;
                    }
                }
            }
        }
        
        if (plateauLength >= 4)
            reward += Mathf.Log(plateauLength + 1) * 0.4f;
        
        if (minHeight < int.MaxValue && maxHeight > int.MinValue)
        {
            int range = maxHeight - minHeight;
            float idealRange = (int)Mathf.Lerp(LevelHeight * 0.2f, LevelHeight * 0.75f, Difficulty);
            reward += Mathf.Clamp(
                -Mathf.Abs(range - idealRange) * 0.5f * heightVariationWeight,
                -2f,
                5f
            );
        }
        int maxAllowedHeight = (int)Mathf.Lerp(LevelHeight * 0.2f, LevelHeight * 0.85f, Difficulty);

        if (maxHeight > maxAllowedHeight)
        {
            int overflow = maxHeight - maxAllowedHeight;

            float heightPenalty = Mathf.Lerp(3.5f, 1.2f, Difficulty);
            reward -= overflow * heightPenalty;
        }
        
        float targetPeak = Mathf.Lerp(LevelHeight * 0.3f, maxAllowedHeight, Difficulty);
        
        reward += -Mathf.Abs(maxHeight - targetPeak) * 0.3f;
        
        float gapRatio = (float)gapCount / LevelWidth;
        float gapWeight = Mathf.Lerp(3f, 7f, Difficulty);
        reward += -Mathf.Abs(gapRatio - gapTarget) * gapWeight;

        return reward;
    }
    
    private float EvaluatePlatforms()
    {
        float reward = 0;
        int platformTiles = 0;
        int usefulPlatforms = 0;

        for (int x = 0; x < LevelWidth; x++)
        {
            for (int y = 0; y < LevelHeight; y++)
            {
                if (_tileGrid[x, y] == TileType.Platform)
                {
                    platformTiles++;

                    int ground = _groundHeight[x];
                    if (ground >= 0)
                    {
                        int height = y - ground;
                        
                        if (height >= 2 && height <= 5)
                            usefulPlatforms++;
                    }
                }
            }
        }

        reward += usefulPlatforms * 0.6f;
        
        reward -= Mathf.Max(0, platformTiles - 25) * 0.3f;

        return reward;
    }
    
    private float EvaluateCollectathonPlatforms()
    {
        float reward = 0f;

        int platformTiles = 0;
        int usefulPlatforms = 0;
        int highPlatforms = 0;

        for (int x = 0; x < LevelWidth; x++)
        {
            for (int y = 0; y < LevelHeight; y++)
            {
                if (_tileGrid[x, y] != TileType.Platform)
                    continue;

                platformTiles++;

                int ground = _groundHeight[x];

                if (ground >= 0)
                {
                    int height = y - ground;
                    
                    if (height >= 2 && height <= 6)
                    {
                        usefulPlatforms++;
                        reward += 0.4f;
                    }
                    
                    if (height >= 5)
                    {
                        highPlatforms++;

                        float highPlatformReward =
                            Mathf.Lerp(0.1f, 0.8f, Difficulty);

                        reward += highPlatformReward;
                    }
                }
                
                if (x > 0 && _tileGrid[x - 1, y] == TileType.Platform)
                {
                    reward += 0.15f;
                }
            }
        }
        
        float targetPlatformRatio =
            Mathf.Lerp(0.08f, 0.22f, Difficulty);

        float idealPlatformTiles =
            LevelWidth * targetPlatformRatio;

        float diff = platformTiles - idealPlatformTiles;
        
        reward -= diff * diff * 0.08f;
        
        reward += usefulPlatforms * Mathf.Lerp(0.3f, 0.8f, Difficulty);
        
        reward += highPlatforms * Mathf.Lerp(0.1f, 0.5f, Difficulty);

        return reward;
    }
    
    private float Evaluate2DSideScrollingEnemies()
    {
        float reward = 0f;
        int enemyCount = 0;
        int validEnemies = 0;

        for (int x = 0; x < LevelWidth; x++)
        {
            for (int y = 0; y < LevelHeight; y++)
            {
                if (_tileGrid[x, y] != TileType.Enemy)
                    continue;

                enemyCount++;

                if (y > 0 &&
                    (_tileGrid[x, y - 1] == TileType.Ground ||
                     _tileGrid[x, y - 1] == TileType.Platform))
                {
                    validEnemies++;
                }
            }
        }

        float enemyRatio = Mathf.Lerp(0.02f, 0.2f, Difficulty);
        float idealEnemies = LevelWidth * enemyRatio;

        float diff = enemyCount - idealEnemies;

        float densityWeight = Mathf.Lerp(0.4f, 0.8f, Difficulty);
        reward -= diff * diff * densityWeight;
    
        float placementReward = Mathf.Lerp(0.08f, 0.18f, Difficulty);
        reward += validEnemies * placementReward;

        return reward;
    }
    
    private float Evaluate2DSideScrollingPickups()
    {
        float reward = 0f;

        int pickupCount = 0;

        for (int x = 0; x < LevelWidth; x++)
        {
            for (int y = 0; y < LevelHeight; y++)
            {
                if (_tileGrid[x, y] != TileType.Coin)
                    continue;

                pickupCount++;
                
                if (y > 0 &&
                    (_tileGrid[x, y - 1] == TileType.Ground ||
                     _tileGrid[x, y - 1] == TileType.Platform))
                {
                    reward += 0.2f;
                }
                
                int nearby = 0;

                for (int px = Mathf.Max(0, x - 2); px <= Mathf.Min(LevelWidth - 1, x + 2); px++)
                {
                    if (px == x)
                        continue;

                    if (_tileGrid[px, y] == TileType.Coin)
                        nearby++;
                }

                reward -= nearby * 0.15f;
            }
        }
        
        float pickupRatio = Mathf.Lerp(0.08f, 0.22f, Difficulty);

        float idealPickups = LevelWidth * pickupRatio;

        float diff = pickupCount - idealPickups;

        reward -= diff * diff * 0.25f;

        return reward;
    }

    private float EvaluateTopDownTerrain()
    {
        float reward = 0f;

        int wallCount = 0;
        int openTiles = 0;

        for (int x = 1; x < LevelWidth - 1; x++)
        {
            for (int y = 1; y < LevelHeight - 1; y++)
            {
                if (_tileGrid[x, y] == TileType.Empty)
                {
                    openTiles++;
                }

                if (_tileGrid[x, y] == TileType.Ground)
                {
                    wallCount++;

                    int horizontalNeighbors = 0;
                    int verticalNeighbors = 0;

                    if (_tileGrid[x + 1, y] == TileType.Ground)
                        horizontalNeighbors++;

                    if (_tileGrid[x - 1, y] == TileType.Ground)
                        horizontalNeighbors++;

                    if (_tileGrid[x, y + 1] == TileType.Ground)
                        verticalNeighbors++;

                    if (_tileGrid[x, y - 1] == TileType.Ground)
                        verticalNeighbors++;
                    
                    if (horizontalNeighbors >= 1)
                        reward += 0.08f;

                    if (verticalNeighbors >= 1)
                        reward += 0.08f;
                    
                    if (horizontalNeighbors + verticalNeighbors >= 3)
                        reward -= 0.15f;
                }
            }
        }
        
        float wallRatio = Mathf.Lerp(0.02f, 0.35f, Difficulty);

        float idealWalls =
            (LevelWidth - 2) *
            (LevelHeight - 2) *
            wallRatio;

        float diff = wallCount - idealWalls;

        float densityWeight =
            Mathf.Lerp(0.15f, 0.35f, Difficulty);

        reward -= diff * diff * densityWeight;
        
        float openRatio =
            (float)openTiles /
            ((LevelWidth - 2) * (LevelHeight - 2));

        float idealOpenRatio =
            Mathf.Lerp(0.94f, 0.65f, Difficulty);

        reward -=
            Mathf.Abs(openRatio - idealOpenRatio) * 25f;
        
        if (!IsTopDownMapConnected())
        {
            reward -= 500f;
        }

        return reward;
    }
    
    private float EvaluateTopDownEnemies(int enemyCount)
    {
        float reward = 0f;

        int validEnemies = 0;

        for (int x = 1; x < LevelWidth - 1; x++)
        {
            for (int y = 1; y < LevelHeight - 1; y++)
            {
                if (_tileGrid[x, y] != TileType.Enemy)
                    continue;

                int nearbyWalls = 0;

                if (_tileGrid[x + 1, y] == TileType.Ground)
                    nearbyWalls++;

                if (_tileGrid[x - 1, y] == TileType.Ground)
                    nearbyWalls++;

                if (_tileGrid[x, y + 1] == TileType.Ground)
                    nearbyWalls++;

                if (_tileGrid[x, y - 1] == TileType.Ground)
                    nearbyWalls++;
                
                if (nearbyWalls <= 2)
                {
                    reward += 0.25f;
                    validEnemies++;
                }
                
                if (nearbyWalls >= 3)
                {
                    reward -= 1.5f;
                }
            }
        }
        
        float enemyRatio =
            Mathf.Lerp(0.01f, 0.08f, Difficulty);

        float idealEnemies =
            LevelWidth *
            LevelHeight *
            enemyRatio;

        float diff = enemyCount - idealEnemies;

        reward -= diff * diff * 0.08f;
        
        reward += validEnemies * 0.15f;

        return reward;
    }
    
    private float EvaluateHealthPickups()
    {
        float reward = 0f;

        int pickupCount = 0;
        int validPickups = 0;

        for (int x = 0; x < LevelWidth; x++)
        {
            for (int y = 1; y < LevelHeight; y++)
            {
                if (_tileGrid[x, y] != TileType.Pickup)
                    continue;

                pickupCount++;

                TileType below =
                    _tileGrid[x, y - 1];
                
                if (below == TileType.Ground ||
                    below == TileType.Platform)
                {
                    validPickups++;
                    reward += 0.3f;
                }
                
                if (_decorationGrid[x, y - 1] ==
                    TileType.EdgeLeft ||
                    _decorationGrid[x, y - 1] ==
                    TileType.EdgeRight)
                {
                    reward -= 1.5f;
                }
            }
        }
        
        float pickupRatio =
            Mathf.Lerp(0.10f, 0.02f, Difficulty);

        float idealPickups =
            LevelWidth * pickupRatio;

        float diff =
            pickupCount - idealPickups;

        reward -= diff * diff * 0.4f;

        reward += validPickups * 0.1f;

        return reward;
    }
    
    private float EvaluateTopDownHealthPickups()
    {
        float reward = 0f;

        int pickupCount = 0;
        int validPickups = 0;

        for (int x = 1; x < LevelWidth - 1; x++)
        {
            for (int y = 1; y < LevelHeight - 1; y++)
            {
                if (_tileGrid[x, y] != TileType.Pickup)
                    continue;

                pickupCount++;

                int nearbyWalls = 0;

                if (_tileGrid[x + 1, y] == TileType.Ground)
                    nearbyWalls++;

                if (_tileGrid[x - 1, y] == TileType.Ground)
                    nearbyWalls++;

                if (_tileGrid[x, y + 1] == TileType.Ground)
                    nearbyWalls++;

                if (_tileGrid[x, y - 1] == TileType.Ground)
                    nearbyWalls++;
                
                if (nearbyWalls <= 2)
                {
                    reward += 0.4f;
                    validPickups++;
                }
                
                if (nearbyWalls >= 3)
                {
                    reward -= 1.2f;
                }
            }
        }
        
        float pickupRatio =
            Mathf.Lerp(0.08f, 0.015f, Difficulty);

        float idealPickups =
            LevelWidth *
            LevelHeight *
            pickupRatio;

        float diff =
            pickupCount - idealPickups;

        reward -= diff * diff * 0.05f;

        reward += validPickups * 0.15f;

        return reward;
    }
    
    private void InstantiateBlocks()
    {
        if(DoNotInstantiate) return;
        
        if (_tileGrid == null) return;
        
        for (int x = 0; x < LevelWidth; x++)
        {
            for (int y = 0; y < LevelHeight; y++)
            {
                Vector3 position = new Vector3(x * BlockSize, y * BlockSize, 0);

                switch (_tileGrid[x, y])
                {
                    case TileType.Ground:
                        Instantiate(GroundBlock, position, Quaternion.identity, BlockParent);
                        break;

                    case TileType.Platform:
                        Instantiate(PlatformBlock, position, Quaternion.identity, BlockParent);
                        break;
                }

                switch (_decorationGrid[x, y])
                {
                    case TileType.EdgeLeft:
                        Instantiate(LeftEdgeTrigger, position, Quaternion.identity, EdgeParent);
                        break;
                    case TileType.EdgeRight:
                        Instantiate(RightEdgeTrigger, position, Quaternion.identity, EdgeParent);
                        break;
                }
            }
        }
    }
    
    private float EvaluateStageHazards()
    {
        float reward = 0f;

        int hazardCount = 0;
        int validHazards = 0;

        for (int x = 0; x < LevelWidth; x++)
        {
            for (int y = 1; y < LevelHeight; y++)
            {
                if (_tileGrid[x, y] != TileType.Hazard)
                    continue;

                hazardCount++;

                TileType below =
                    _tileGrid[x, y - 1];
                
                if (below == TileType.Ground ||
                    below == TileType.Platform)
                {
                    validHazards++;
                    reward += 0.25f;
                }
                
                if (_decorationGrid[x, y - 1] ==
                    TileType.EdgeLeft ||
                    _decorationGrid[x, y - 1] ==
                    TileType.EdgeRight)
                {
                    reward -= 1.5f;
                }
                
                int nearbyHazards = 0;

                for (int ox = -3; ox <= 3; ox++)
                {
                    int nx = x + ox;

                    if (nx < 0 || nx >= LevelWidth)
                        continue;

                    if (nx == x)
                        continue;

                    if (_tileGrid[nx, y] ==
                        TileType.Hazard)
                    {
                        nearbyHazards++;
                    }
                }

                reward -= nearbyHazards * 0.4f;
            }
        }
        
        float hazardRatio =
            Mathf.Lerp(0.01f, 0.10f, Difficulty);

        float idealHazards =
            LevelWidth * hazardRatio;

        float diff =
            hazardCount - idealHazards;

        reward -= diff * diff * 0.45f;

        reward += validHazards * 0.1f;

        return reward;
    }
    
    private float EvaluateTopDownStageHazards()
    {
        float reward = 0f;

        int hazardCount = 0;
        int validHazards = 0;

        for (int x = 1; x < LevelWidth - 1; x++)
        {
            for (int y = 1; y < LevelHeight - 1; y++)
            {
                if (_tileGrid[x, y] != TileType.Hazard)
                    continue;

                hazardCount++;

                int nearbyWalls = 0;

                if (_tileGrid[x + 1, y] == TileType.Ground)
                    nearbyWalls++;

                if (_tileGrid[x - 1, y] == TileType.Ground)
                    nearbyWalls++;

                if (_tileGrid[x, y + 1] == TileType.Ground)
                    nearbyWalls++;

                if (_tileGrid[x, y - 1] == TileType.Ground)
                    nearbyWalls++;
                
                if (nearbyWalls >= 1 &&
                    nearbyWalls <= 2)
                {
                    reward += 0.45f;
                    validHazards++;
                }
                
                if (nearbyWalls >= 3)
                {
                    reward -= 1.5f;
                }
                
                if (nearbyWalls == 0)
                {
                    reward -= 0.2f;
                }
                
                int nearbyHazards = 0;

                for (int ox = -3; ox <= 3; ox++)
                {
                    for (int oy = -3; oy <= 3; oy++)
                    {
                        int nx = x + ox;
                        int ny = y + oy;

                        if (nx < 0 || nx >= LevelWidth ||
                            ny < 0 || ny >= LevelHeight)
                            continue;

                        if (nx == x && ny == y)
                            continue;

                        if (_tileGrid[nx, ny] ==
                            TileType.Hazard)
                        {
                            nearbyHazards++;
                        }
                    }
                }

                reward -= nearbyHazards * 0.12f;
            }
        }
        
        float hazardRatio =
            Mathf.Lerp(0.01f, 0.08f, Difficulty);

        float idealHazards =
            LevelWidth *
            LevelHeight *
            hazardRatio;

        float diff =
            hazardCount - idealHazards;

        reward -= diff * diff * 0.03f;

        reward += validHazards * 0.15f;

        return reward;
    }
    
    private float EvaluateTopDownCoins()
    {
        float reward = 0f;

        int coinCount = 0;
        int validCoins = 0;

        for (int x = 1; x < LevelWidth - 1; x++)
        {
            for (int y = 1; y < LevelHeight - 1; y++)
            {
                if (_tileGrid[x, y] != TileType.Coin)
                    continue;

                coinCount++;

                int nearbyWalls = 0;

                if (_tileGrid[x + 1, y] == TileType.Ground) nearbyWalls++;
                if (_tileGrid[x - 1, y] == TileType.Ground) nearbyWalls++;
                if (_tileGrid[x, y + 1] == TileType.Ground) nearbyWalls++;
                if (_tileGrid[x, y - 1] == TileType.Ground) nearbyWalls++;

                if (nearbyWalls <= 2)
                {
                    reward += 0.35f;
                    validCoins++;
                }

                if (nearbyWalls >= 3)
                {
                    reward -= 1.0f;
                }

                int nearbyCoins = 0;

                for (int ox = -2; ox <= 2; ox++)
                {
                    for (int oy = -2; oy <= 2; oy++)
                    {
                        int nx = x + ox;
                        int ny = y + oy;

                        if (nx < 0 || nx >= LevelWidth ||
                            ny < 0 || ny >= LevelHeight)
                            continue;

                        if (nx == x && ny == y)
                            continue;

                        if (_tileGrid[nx, ny] == TileType.Coin)
                            nearbyCoins++;
                    }
                }

                if (nearbyCoins >= 1 && nearbyCoins <= 3)
                    reward += 0.2f;

                if (nearbyCoins > 5)
                    reward -= 0.5f;
            }
        }

        float coinRatio = Mathf.Lerp(0.01f, 0.10f, Difficulty);

        float idealCoins =
            LevelWidth * LevelHeight * coinRatio;

        float diff = coinCount - idealCoins;

        reward -= diff * diff * 0.02f;

        reward += validCoins * 0.1f;

        return reward;
    }
    
    private void InstantiateEnemies()
    {
        if(DoNotInstantiate) return;
        
        if (_tileGrid == null) return;

        List<GameObject> enemyList = new List<GameObject>();
        
        for (int x = 0; x < LevelWidth; x++)
        {
            for (int y = 0; y < LevelHeight; y++)
            {
                Vector3 position = new Vector3(x * BlockSize, y * BlockSize, 0);

                if (_tileGrid[x, y] == TileType.Enemy && generationMode == GenerationMode.SideScroller) enemyList.Add(Instantiate(SideScrollingEnemyPerfab, position, Quaternion.identity, EnemyParent));
                else if (_tileGrid[x, y] == TileType.Enemy && generationMode == GenerationMode.TopDown) enemyList.Add(Instantiate(TopDownEnemyPerfab, position, Quaternion.identity, EnemyParent));
            }
        }

        if (haveEnemyScaling)
        {
            foreach (var e in enemyList)
            {
                AdjustEnemyDifficulty(e.GetComponent<EnemyDifficultyAdjuster>());
            }
        }
    }
    
    private void InstantiatePickups()
    {
        if(DoNotInstantiate) return;
        
        if (_tileGrid == null) return;
        
        for (int x = 0; x < LevelWidth; x++)
        {
            for (int y = 0; y < LevelHeight; y++)
            {
                Vector3 position = new Vector3(x * BlockSize, y * BlockSize, 0);

                switch (_tileGrid[x, y])
                {
                    case TileType.Coin:
                        Instantiate(CoinPrefab, position, Quaternion.identity, CoinParent);
                        break;
                    case TileType.Pickup:
                        Instantiate(PickupPrefab, position, Quaternion.identity, CoinParent);
                        break;
                }
            }
        }
    }
    
    private void InstantiateHazards()
    {
        if(DoNotInstantiate) return;
        if (_tileGrid == null) return;
        
        for (int x = 0; x < LevelWidth; x++)
        {
            for (int y = 0; y < LevelHeight; y++)
            {
                Vector3 position = new Vector3(x * BlockSize, y * BlockSize, 0);

                if(_tileGrid[x, y] == TileType.Hazard) Instantiate(HazardPrefab, position, Quaternion.identity, HazardParent);
            }
        }
    }
    
    private void PlaceDeathPlane()
    {
        if(DoNotInstantiate) return;
        if (_tileGrid == null) return;

        Vector3 deathPlanePos = GridCoordinatesToWorldCoordinates((LevelWidth / 2), (-LevelHeight / 2) - 5);

        var deathPlane = Instantiate(DeathPlane, deathPlanePos, Quaternion.identity);

        deathPlane.transform.localScale = new Vector3(LevelWidth * 1.5f, LevelHeight);
    
    }
    
    private Vector3 GridCoordinatesToWorldCoordinates(int x, int y)
    {
        return new Vector3(x * BlockSize, y * BlockSize, 0f);
    }
    
    private Vector2Int SetPlayerSpawnPoint()
    {
        for (int x = 0; x < LevelWidth; x++)
        {
            if (_groundHeight[x] < 0) continue;

            int spawnY = _groundHeight[x] + 2;
            
            if (spawnY >= LevelHeight) continue;
            
            if (_tileGrid[x, spawnY] == TileType.Empty && _decorationGrid[x, spawnY] == TileType.Empty) return new Vector2Int(x, spawnY);
            
        }
        
        return new Vector2Int(0, LevelHeight - 1);
    }
    
    private Vector2Int SetTopDownPlayerSpawnPoint()
    {
        List<Vector2Int> validSpawns = new();

        for (int x = 1; x < LevelWidth - 1; x++)
        {
            for (int y = 1; y < LevelHeight - 1; y++)
            {
                if (_tileGrid[x, y] != TileType.Empty)
                    continue;
                
                if (_tileGrid[x, y] == TileType.Enemy)
                    continue;
                
                int nearbyWalls = 0;

                if (_tileGrid[x + 1, y] == TileType.Ground)
                    nearbyWalls++;

                if (_tileGrid[x - 1, y] == TileType.Ground)
                    nearbyWalls++;

                if (_tileGrid[x, y + 1] == TileType.Ground)
                    nearbyWalls++;

                if (_tileGrid[x, y - 1] == TileType.Ground)
                    nearbyWalls++;
                
                if (nearbyWalls > 1)
                    continue;

                validSpawns.Add(new Vector2Int(x, y));
            }
        }
        
        if (validSpawns.Count > 0)
        {
            return validSpawns[Random.Range(0, validSpawns.Count)];
        }
        
        return new Vector2Int(
            LevelWidth / 2,
            LevelHeight / 2
        );
    }
    
    private void OnDrawGizmos()
    {
        if (_tileGrid == null) return;

        Vector3 blockSizeVector = new Vector3(1, 1, 0);
        
        for (int x = 0; x < LevelWidth; x++)
        {
            for (int y = 0; y < LevelHeight; y++)
            {
                Vector3 position = new Vector3(x, y, 0);

                switch (_tileGrid[x, y])
                {
                    case TileType.Ground:
                        Gizmos.color = Color.green;
                        break;

                    case TileType.Platform:
                        Gizmos.color = Color.yellow;
                        break;

                    case TileType.Enemy:
                        Gizmos.color = Color.magenta;
                        break;
                    
                    case TileType.Coin:
                        Gizmos.color = Color.red;
                        break;
                    
                    case TileType.Pickup:
                        Gizmos.color = Color.white;
                        break;
                    
                    case TileType.Hazard:
                        Gizmos.color = Color.black;
                        break;

                    default:
                        Gizmos.color = Color.gray;
                        break;
                }

                switch (_decorationGrid[x, y])
                {
                    case TileType.EdgeLeft:
                        Gizmos.color = Color.blue;
                        break;
                    case TileType.EdgeRight:
                        Gizmos.color = Color.cyan;
                        break;
                }

                Gizmos.DrawCube(position, blockSizeVector * 0.9f);
            }
        }
    }
}
