using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelGenerator : MonoBehaviour
{
    [Header("Seed")] 
    [SerializeField] private int BaseSeed;
    
    [Header("Grid Settings")]
    [SerializeField] private int GridWidth;
    [SerializeField] private int GridHeight;

    [Header("Ground Settings")] 
    [SerializeField] private int MinGroundHeight;
    [SerializeField] private int MaxGroundHeight;

    [Header("Gap Settings")] 
    [SerializeField][Range(0f, 1f)] private float GapChance;
    [SerializeField] private int MinGap;
    [SerializeField] private int MaxGap;
    [SerializeField] private int MinDistanceBetweenGaps;
    
    [Header("Platform Settings")]
    [SerializeField] private int MaxPlatformHeightFromGround;
    [SerializeField] private int MinPlatformHeightFromGround;
    [SerializeField] private int MinPlatformLength;
    [SerializeField] private int MaxPlatformLength;
    [SerializeField] private int PlatformAttempts;
    [SerializeField] private int MinVerticalSeparation;

    [Header("Enemy Settings")] 
    [SerializeField] private GameObject EnemyPerfab;
    [SerializeField] private Transform EnemyParent;
    [SerializeField] [Range(0f, 1f)] private float EnemyDensity;
    [SerializeField] private int MinDistanceBetweenEnemies;
    [SerializeField][Range(0f, 1f)] private float PlatformEnemyRatio;

    [Header("Prefabs")] 
    [SerializeField] private int BlockSize;
    [SerializeField] private Transform BlockParent;
    [SerializeField] private Transform EdgeParent;
    [SerializeField] private GameObject GroundBlock;
    [SerializeField] private GameObject PlatformBlock;
    [SerializeField] private GameObject LeftEdgeTrigger;
    [SerializeField] private GameObject RightEdgeTrigger;
    [SerializeField] private GameObject DeathPlane;

    [Header("Player")] 
    [SerializeField] private GameObject Player;
    
    [Header("Score")] 
    [SerializeField] private float DesiredScore;

    [Header("Training")]
    [SerializeField] private bool TrainingMode;
    
    [SerializeField] private TileType[,] TileGrid;
    [SerializeField] private TileType[,] DecorationGrid;
    private int[] _groundHeight;
    private int _maxAmountOfEnemies;
    private int _actualEnemyCount;

    public void GenerateRawLevel(int seed)
    {
        // Initialize grids
        TileGrid = new TileType[GridWidth, GridHeight];
        DecorationGrid = new TileType[GridWidth, GridHeight];
        _groundHeight = new int[GridWidth];

        System.Random rngGround = new System.Random(seed + 1);
        System.Random rngPlatforms = new System.Random(seed + 2);
        System.Random rngEnemiesPlatform = new System.Random(seed + 3);
        System.Random rngEnemiesGround = new System.Random(seed + 4);
        
        // Generate components deterministically
        GenerateGround(rngGround);
        AddGaps(rngGround);
        AddPlatforms(rngPlatforms);
        AddEdges();             // If AddEdges uses randomness, pass rng too

        CalculateMaxEnemies();

        AddSpawnPointsEnemies(rngEnemiesPlatform, rngEnemiesGround); // Pass RNG here for deterministic enemy placement
    }
    
    private void GenerateLevelWithEvaluation()
    {
        bool validLevel = false;
        Vector2Int spawn = new Vector2Int();
        int attempts = 0;

        while (!validLevel)
        {
            int currentSeed = BaseSeed + attempts;

            // --- Feature-specific RNGs ---
            System.Random rngGround = new System.Random(currentSeed + 1);
            System.Random rngPlatforms = new System.Random(currentSeed + 2);
            System.Random rngEnemiesPlatform = new System.Random(currentSeed + 3);
            System.Random rngEnemiesGround = new System.Random(currentSeed + 4);

            // Reset grids
            TileGrid = new TileType[GridWidth, GridHeight];
            DecorationGrid = new TileType[GridWidth, GridHeight];
            _groundHeight = new int[GridWidth];

            // --- Generation ---
            GenerateGround(rngGround);
            AddGaps(rngGround);
            AddPlatforms(rngPlatforms);
            AddEdges();
            CalculateMaxEnemies();
            AddSpawnPointsEnemies(rngEnemiesPlatform, rngEnemiesGround);

            spawn = SetPlayerSpawnPoint();

            float levelScore = EvaluateLevel(spawn);
            bool reachable = AllEnemiesReachable(spawn);

            attempts++;

            if (reachable && levelScore >= DesiredScore)
            {
                validLevel = true;
            }
        }

        InstantiateBlocks();
        InstantiateEnemies();
        PlaceDeathPlane();

        Vector3 worldPos = GridCoordinatesToWorldCoordinates(spawn.x, spawn.y);
        Player.transform.position = worldPos;
    }
    
    private void GenerateGround(System.Random rng)
    {
        int x = 0;

        int currentHeight = rng.Next(MinGroundHeight, MaxGroundHeight + 1);

        while (x < GridWidth)
        {
            // Minimum plateau length = 3
            int segmentLength = rng.Next(3, 7); // 3 to 6 wide

            // Clamp so we don't exceed grid
            segmentLength = Mathf.Min(segmentLength, GridWidth - x);

            // Fill this segment
            for (int i = 0; i < segmentLength; i++)
            {
                _groundHeight[x] = currentHeight;

                for (int y = 0; y <= currentHeight; y++)
                {
                    TileGrid[x, y] = TileType.Ground;
                }
                x++;
            }

            // Decide next height variation
            int heightChange = rng.Next(-1, 2); // -1, 0, or +1
            currentHeight += heightChange;

            currentHeight = Mathf.Clamp(currentHeight, MinGroundHeight, MaxGroundHeight);
        }
    }

    private void AddGaps(System.Random rng)
    {
        int lastGapEnd = -MinDistanceBetweenGaps;

        for (int x = 0; x < GridWidth; x++)
        {
            // Too close to previous gap
            if (x - lastGapEnd < MinDistanceBetweenGaps)
                continue;

            if ((float)rng.NextDouble() < GapChance)
            {
                int gapSize = rng.Next(MinGap, MaxGap + 1);

                for (int i = 0; i < gapSize && x + i < GridWidth; i++)
                {
                    int column = x + i;

                    for (int y = 0; y <= _groundHeight[column]; y++)
                    {
                        TileGrid[column, y] = TileType.Empty;
                    }

                    _groundHeight[column] = -1;
                }

                lastGapEnd = x + gapSize;
                x += gapSize;
            }
        }
    }
    
    private void AddPlatforms(System.Random rng)
    {
        int attempts = 0;

        while (attempts < PlatformAttempts)
        {
            attempts++;

            int length = rng.Next(MinPlatformLength, MaxPlatformLength + 1);
            int x = rng.Next(1, GridWidth - length - 1);
            int y = rng.Next(2, GridHeight - 2);

            if (IsAreaClear(x, y, length) && IsPlatformStructurallyValid(x, y, length))
            {
                for (int i = 0; i < length; i++)
                {
                    TileGrid[x + i, y] = TileType.Platform;
                }
            }
        }
    }
    
    private void AddSpawnPointsEnemies(System.Random rngPlatform, System.Random rngGround)
    {
        List<Vector2Int> platformSpawns = new List<Vector2Int>();
        List<Vector2Int> groundSpawns = new List<Vector2Int>();

        List<Vector2Int> placedPlatformEnemies = new List<Vector2Int>();
        List<Vector2Int> placedGroundEnemies = new List<Vector2Int>();

        int desiredPlatformCount = Mathf.RoundToInt(_maxAmountOfEnemies * PlatformEnemyRatio);
        int desiredGroundCount = _maxAmountOfEnemies - desiredPlatformCount;

        int platformPlaced = 0;
        int groundPlaced = 0;

        // Collect PLATFORM spawns
        for (int y = 0; y < GridHeight - 1; y++)
            for (int x = 0; x < GridWidth; x++)
                if (TileGrid[x, y] == TileType.Platform &&
                    TileGrid[x, y + 1] == TileType.Empty &&
                    DecorationGrid[x, y + 1] == TileType.Empty)
                    platformSpawns.Add(new Vector2Int(x, y + 1));

        // Collect GROUND spawns
        for (int x = 0; x < GridWidth; x++)
            if (_groundHeight[x] >= 0)
            {
                int y = _groundHeight[x] + 1;
                if (y < GridHeight &&
                    TileGrid[x, y] == TileType.Empty &&
                    DecorationGrid[x, y] == TileType.Empty)
                    groundSpawns.Add(new Vector2Int(x, y));
            }

        // Shuffle deterministically
        ShuffleList(platformSpawns, rngPlatform);
        ShuffleList(groundSpawns, rngGround);

        // Place PLATFORM enemies
        foreach (var point in platformSpawns)
        {
            if (platformPlaced >= desiredPlatformCount) break;
            if (TooCloseHorizontal(point, placedPlatformEnemies)) continue;

            TileGrid[point.x, point.y] = TileType.Enemy;
            placedPlatformEnemies.Add(point);
            platformPlaced++;
        }

        // Place GROUND enemies
        foreach (var point in groundSpawns)
        {
            if (groundPlaced >= desiredGroundCount) break;
            if (TooCloseHorizontal(point, placedGroundEnemies)) continue;

            TileGrid[point.x, point.y] = TileType.Enemy;
            placedGroundEnemies.Add(point);
            groundPlaced++;
        }

        // Fallback fill
        if (platformPlaced + groundPlaced < _maxAmountOfEnemies)
        {
            foreach (var point in platformSpawns)
            {
                if (platformPlaced >= desiredPlatformCount) break;
                if (TileGrid[point.x, point.y] != TileType.Empty) continue;
                if (TooCloseHorizontal(point, placedPlatformEnemies)) continue;

                TileGrid[point.x, point.y] = TileType.Enemy;
                placedPlatformEnemies.Add(point);
                platformPlaced++;
            }

            foreach (var point in groundSpawns)
            {
                if (groundPlaced >= desiredGroundCount) break;
                if (TileGrid[point.x, point.y] != TileType.Empty) continue;
                if (TooCloseHorizontal(point, placedGroundEnemies)) continue;

                TileGrid[point.x, point.y] = TileType.Enemy;
                placedGroundEnemies.Add(point);
                groundPlaced++;
            }
        }
    }

    private void AddEdges()
    {
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                // ----- GROUND EDGES (next to gaps) -----
                if (TileGrid[x, y] == TileType.Ground)
                {
                    bool leftGap = (x == 0) || _groundHeight[x - 1] < 0;
                    bool rightGap = (x == GridWidth - 1) || _groundHeight[x + 1] < 0;

                    if (leftGap && y == _groundHeight[x])
                    {
                        DecorationGrid[x, y + 1] = TileType.EdgeLeft;
                    }

                    if (rightGap && y == _groundHeight[x])
                    {
                        DecorationGrid[x, y + 1] = TileType.EdgeRight;
                    }
                }

                // ----- PLATFORM EDGES -----
                if (TileGrid[x, y] == TileType.Platform)
                {
                    bool leftEmpty = (x == 0) || TileGrid[x - 1, y] != TileType.Platform;
                    bool rightEmpty = (x == GridWidth - 1) || TileGrid[x + 1, y] != TileType.Platform;

                    if (leftEmpty)
                    {
                        if (y + 1 < GridHeight) DecorationGrid[x, y + 1] = TileType.EdgeLeft;
                    }

                    if (rightEmpty)
                    {
                        if (y + 1 < GridHeight) DecorationGrid[x, y + 1] = TileType.EdgeRight;
                    }
                }
            }
        }
    }
    
    public Vector2Int SetPlayerSpawnPoint()
    {
        for (int x = 0; x < GridWidth; x++)
        {
            // Skip gaps
            if (_groundHeight[x] < 0) continue;

            int spawnY = _groundHeight[x] + 2;

            // Make sure it's inside grid
            if (spawnY >= GridHeight) continue;

            // Make sure space is empty (no enemies or decorations)
            if (TileGrid[x, spawnY] == TileType.Empty && DecorationGrid[x, spawnY] == TileType.Empty) return new Vector2Int(x, spawnY);
            
        }

        // Fallback (should never happen unless grid is broken)
        return new Vector2Int(0, GridHeight - 1);
    }

    private Vector3 GridCoordinatesToWorldCoordinates(int x, int y)
    {
        return new Vector3(x * BlockSize, y * BlockSize, 0f);
    }
    private bool IsAreaClear(int startX, int y, int length)
    {
        for (int x = startX; x < startX + length; x++)
        {
            if (x < 0 || x >= GridWidth) return false;

            if (_groundHeight[x] < 0) return false;

            if (y <= _groundHeight[x] + MinPlatformHeightFromGround - 1) return false;

            if (y > _groundHeight[x] + MaxPlatformHeightFromGround) return false;

            if (y <= _groundHeight[x]) return false;
        }

        
        for (int checkY = y - MinVerticalSeparation; checkY <= y + MinVerticalSeparation; checkY++)
        {
            if (checkY < 0 || checkY >= GridHeight) continue;

            for (int i = 0; i < length; i++)
            {
                int checkX = startX + i;

                if (TileGrid[checkX, checkY] == TileType.Platform)
                    return false;
            }
        }

        return true;
    }

    private bool TooCloseHorizontal(Vector2Int newPos, List<Vector2Int> placedEnemies)
    {
        foreach (var pos in placedEnemies)
        {
            if (Mathf.Abs(pos.x - newPos.x) < MinDistanceBetweenEnemies)
                return true;
        }
        return false;
    }
    
        private bool IsPlatformStructurallyValid(int startX, int y, int length)
        {
            int minVerticalClearance = 2;

            // 1. Ensure platform is sufficiently above nearby ground (including edges)
            for (int i = -1; i <= length; i++)
            {
                int x = startX + i;

                if (x < 0 || x >= GridWidth)
                    continue;

                int ground = _groundHeight[x];

                if (ground >= 0)
                {
                    if (y - ground <= minVerticalClearance)
                        return false;
                }
            }

            // 2. Prevent platforms near upward terrain steps
            for (int x = startX - 1; x <= startX + length; x++)
            {
                if (x <= 0 || x >= GridWidth)
                    continue;

                int diff = _groundHeight[x] - _groundHeight[x - 1];

                // If terrain steps upward, require extra clearance
                if (diff > 0)
                {
                    if (y - _groundHeight[x] <= minVerticalClearance + 1)
                        return false;
                }
            }

            return true;
        }
    private void ShuffleList<T>(List<T> list, System.Random rng)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = rng.Next(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }
    
    private void InstantiateBlocks()
    {
        if (TileGrid == null) return;
        
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                Vector3 position = new Vector3(x * BlockSize, y * BlockSize, 0);

                switch (TileGrid[x, y])
                {
                    case TileType.Ground:
                        Instantiate(GroundBlock, position, Quaternion.identity, BlockParent);
                        break;

                    case TileType.Platform:
                        Instantiate(PlatformBlock, position, Quaternion.identity, BlockParent);
                        break;
                }

                switch (DecorationGrid[x, y])
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

    private void InstantiateEnemies()
    {
        if (TileGrid == null) return;
        
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                Vector3 position = new Vector3(x * BlockSize, y * BlockSize, 0);

                if (TileGrid[x, y] == TileType.Enemy) Instantiate(EnemyPerfab, position, Quaternion.identity, EnemyParent);
            }
        }
    }

    private void PlaceDeathPlane()
    {
        if (TileGrid == null) return;

        Vector3 deathPlanePos = GridCoordinatesToWorldCoordinates((GridWidth / 2), (-GridHeight / 2) - 5);

        var deathPlane = Instantiate(DeathPlane, deathPlanePos, Quaternion.identity);

        deathPlane.transform.localScale = new Vector3(GridWidth * 1.5f, GridHeight);

    }
    
    private int CountValidEnemySpawnTiles()
    {
        int count = 0;

        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight - 1; y++)
            {
                // Ground surface tile
                if (TileGrid[x, y] == TileType.Ground && TileGrid[x, y + 1] == TileType.Empty)
                {
                    count++;
                }

                // Platform tile
                if (TileGrid[x, y] == TileType.Platform && TileGrid[x, y + 1] == TileType.Empty)
                {
                    count++;
                }
            }
        }
        if (!TrainingMode) Debug.Log("The total amount of valid tiles for enemies is: " + count);
        return count;
    }
    
    private void CalculateMaxEnemies()
    {
        int validTiles = CountValidEnemySpawnTiles();

        int maxPossible = Mathf.FloorToInt(validTiles / (float)MinDistanceBetweenEnemies);
        if (!TrainingMode) Debug.Log("The possible max amount of enemies is: " + maxPossible);

        _maxAmountOfEnemies = Mathf.RoundToInt(maxPossible * EnemyDensity);
        if (!TrainingMode) Debug.Log("The amount of enemies after applying Enemy Density is: " + _maxAmountOfEnemies);
    }

    public int GetActualEnemyCount()
    {
        return _actualEnemyCount;
    }

    public int GetGridWidth()
    {
        return GridWidth;
    }
    
    public int GetGridHeight()
    {
            return GridHeight;
    }
    
    ///////----AI STUFF PROBABLY----///////
    
    private bool IsInsideBounds(int x, int y)
    {
        return x >= 0 && x < GridWidth && y >= 0 && y < GridHeight;
    }
    
    private bool IsStandable(int x, int y)
    {
        if (!IsInsideBounds(x, y))
            return false;

        // Must be empty OR enemy spawn marker
        if (TileGrid[x, y] != TileType.Empty &&
            TileGrid[x, y] != TileType.Enemy)
            return false;

        // Can't be bottom row
        if (y - 1 < 0)
            return false;

        // Must stand on ground or platform
        if (TileGrid[x, y - 1] != TileType.Ground &&
            TileGrid[x, y - 1] != TileType.Platform)
            return false;

        return true;
    }
    
    private List<Vector2Int> GetAllStandableTiles()
    {
        List<Vector2Int> tiles = new List<Vector2Int>();

        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 1; y < GridHeight; y++)
            {
                if (IsStandable(x, y))
                    tiles.Add(new Vector2Int(x, y));
            }
        }

        return tiles;
    }
    
    private bool CanJump(Vector2Int from, Vector2Int to)
    {
        int dx = Mathf.Abs(from.x - to.x);
        int dy = to.y - from.y;

        if (dy < -4) return false; // falling too far //Probably will need to set up a max jump height variable
        if (dy > 4) return false;

        if (dx > 6) return false; //Probably will need to set up a max jump distance variable

        return true;
    }
    
    private HashSet<Vector2Int> GetReachableTiles(Vector2Int start)
    {
        List<Vector2Int> standable = GetAllStandableTiles();

        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            foreach (var tile in standable)
            {
                if (visited.Contains(tile))
                    continue;

                if (CanJump(current, tile))
                {
                    visited.Add(tile);
                    queue.Enqueue(tile);
                }
            }
        }

        return visited;
    }
    
    private bool AllEnemiesReachable(Vector2Int spawn)
    {
        var reachable = GetReachableTiles(spawn);

        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                if (TileGrid[x, y] == TileType.Enemy)
                {
                    Vector2Int pos = new Vector2Int(x, y);

                    if (!reachable.Contains(pos))
                        return false;
                }
            }
        }

        return true;
    }
    
    private int CountEnemies()
    {
        int count = 0;

        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                if (TileGrid[x, y] == TileType.Enemy)
                    count++;
            }
        }

        return count;
    }
    
    private float ScoreEnemyCount()
    {
        int enemies = CountEnemies();

        int ideal = _maxAmountOfEnemies;

        float score = 1f - Mathf.Abs(enemies - ideal) / (float)ideal;

        return Mathf.Clamp01(score);
    }
    
    private List<Vector2Int> GetEnemyTiles()
    {
        List<Vector2Int> enemies = new List<Vector2Int>();

        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                if (TileGrid[x, y] == TileType.Enemy)
                    enemies.Add(new Vector2Int(x, y));
            }
        }

        return enemies;
    }
    
    private float ScoreEnemySpread()
    {
        var enemies = GetEnemyTiles();

        if (enemies.Count < 2)
            return 0;

        float totalDistance = 0;
        int pairs = 0;

        for (int i = 0; i < enemies.Count; i++)
        {
            for (int j = i + 1; j < enemies.Count; j++)
            {
                totalDistance += Mathf.Abs(enemies[i].x - enemies[j].x);
                pairs++;
            }
        }

        float avgDistance = totalDistance / pairs;
        
        //Debug.Log("Enemy Score: "+ avgDistance / GridWidth);
        return avgDistance / GridWidth;
    }
    
    
    private float ScoreEnemySpacing(int distance)
    {
        if (distance < MinDistanceBetweenEnemies)
            return -1f;        // too close

        if (distance < 8)
            return 0.3f;       // acceptable

        if (distance < 15)
            return 0.6f;       // good

        return 0.4f;           // very far apart
    }
    private int CountGaps()
    {
        int gaps = 0;

        for (int x = 0; x < GridWidth; x++)
        {
            if (_groundHeight[x] == 0)
                gaps++;
        }

        return gaps;
    }
    
    private float ScoreGapCount()
    {
        int gaps = CountGaps();

        int ideal = GridWidth / 12;

        float score = 1f - Mathf.Abs(gaps - ideal) / (float)ideal;

        return Mathf.Clamp01(score);
    }
    
    private float EvaluateGapDifficulty()
    {
        float score = 0f;
        int gapWidth = 0;

        for (int x = 0; x < GridWidth; x++)
        {
            bool isGap = _groundHeight[x] == -1;

            if (isGap)
            {
                gapWidth++;
            }
            else
            {
                if (gapWidth > 0)
                {
                    score += ScoreGap(gapWidth);
                    gapWidth = 0;
                }
            }
        }

        // Handle gap at end of level
        if (gapWidth > 0)
        {
            score += ScoreGap(gapWidth);
        }

        return score;
    }
    
    private float ScoreGap(int width)
    {
        if (width == 1)
            return 0.2f;

        if (width <= 3)
            return 0.6f;

        if (width <= 6)
            return 0.3f;

        return -2f;
    }
    
    private float ScorePathDensity(Vector2Int spawn)
    {
        var standable = GetAllStandableTiles();
        var reachable = GetReachableTiles(spawn);

        if (standable.Count == 0)
            return 0;

        int reachableStandable = 0;

        foreach (var tile in standable)
        {
            if (reachable.Contains(tile))
                reachableStandable++;
        }

        float density = (float)reachableStandable / standable.Count;

        return density;
    }

    private float ScoreJumpFlow()
    {
        var standable = GetAllStandableTiles();

        if (standable.Count < 2) return 0;

        float jumps = 0;

        for (int i = 0; i < standable.Count; i++)
        {
            for (int j = i + 1; j < standable.Count; j++)
            {
                if (CanJump(standable[i], standable[j]))
                {
                    jumps++;
                }
            }
        }

        return jumps / standable.Count;
    }
    
    private float ScorePlatformChains(Vector2Int spawn)
    {
        var standable = GetAllStandableTiles();

        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        
        queue.Enqueue(spawn);
        visited.Add(spawn);

        int chainLength = 0;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            chainLength++;

            foreach (var tile in standable)
            {
                if (!visited.Contains(tile) && CanJump(current, tile))
                {
                    visited.Add(tile);
                    queue.Enqueue(tile);
                }
            }
        }

        return (float)chainLength / standable.Count;
    }

    private int CountPlatformTiles()
    {
        int count = 0;

        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                if (TileGrid[x, y] == TileType.Platform) count++;
            }
        }
        if (!TrainingMode) Debug.Log("Number of platform tiles: " + count);
        return count;
    }

    private float ScorePlatformAmount()
    {
        int tileCount = CountPlatformTiles();

        float avgLength = (MinPlatformLength + MaxPlatformLength) / 2f;

        int estimatedPlatforms = Mathf.RoundToInt(tileCount / avgLength);
        if (!TrainingMode) Debug.Log("Estimated amount of platforms: " + estimatedPlatforms);

        float ideal = GridWidth / 8f;

        float score = 1f - Mathf.Abs(estimatedPlatforms - ideal) / ideal;
        
        return Mathf.Clamp01(score);
    }
    
    private float ScoreEnemyDensityTarget()
    {
        float density = CountEnemies() / (float)GridWidth;

        if (density < 0.5f) return -2f;
        if (density < 1f) return -2f;
        return 1f - Mathf.Abs(density - 0.75f);
    }
    
    private float ScoreTerrainVariation()
    {
        float totalVariation = 0f;
        int samples = 0;

        for (int x = 1; x < GridWidth; x++)
        {
            int prev = _groundHeight[x - 1];
            int curr = _groundHeight[x];

            if (prev < 0 || curr < 0)
                continue;

            totalVariation += Mathf.Abs(curr - prev);
            samples++;
        }

        if (samples == 0)
            return 0f;

        return totalVariation / samples;
    }
    
    public float EvaluateLevel(Vector2Int spawn)
    {
        float score = 0f;

        // 1. Hard constraint
        if (!AllEnemiesReachable(spawn))
            return -1000f;

        // 2. Metrics
        score += ScoreEnemyCount();
        score += ScoreEnemySpread();
        score += ScoreEnemyDensityTarget();
        score += ScoreGapCount();
        score += EvaluateGapDifficulty();
        score += ScorePathDensity(spawn);
        score += ScoreJumpFlow();
        score += ScorePlatformAmount();
        score += ScorePlatformChains(spawn);
        return score;
    }

    public void ApplyParameters(GeneratorParameters p)
    {
        BaseSeed = p.BaseSeed;
        
        MinGroundHeight = p.MinGroundHeight;
        MaxGroundHeight = p.MaxGroundHeight;

        GapChance = p.GapChance;
        MinGap = p.MinGap;
        MaxGap = p.MaxGap;
        MinDistanceBetweenGaps = p.MinDistanceBetweenGaps;

        PlatformAttempts = p.PlatformAttempts;

        MinPlatformLength = p.MinPlatformLength;
        MaxPlatformLength = p.MaxPlatformLength;

        MinPlatformHeightFromGround = p.MinPlatformHeightFromGround;
        MaxPlatformHeightFromGround = p.MaxPlatformHeightFromGround;

        MinVerticalSeparation = p.MinVerticalSeparation;

        EnemyDensity = p.EnemyDensity;
        MinDistanceBetweenEnemies = p.MinDistanceBetweenEnemies;

        PlatformEnemyRatio = p.PlatformEnemyRatio;
    }
    
    public void SaveLevel(GeneratorParameters p, float score)
    {
        LevelData data = new LevelData();

        data.score = score;
        data.parameters = p;
        data.width = GridWidth;
        data.height = GridHeight;

        data.tileGrid = FlattenGrid(TileGrid);
        data.decorationGrid = FlattenGrid(DecorationGrid);

        string json = JsonUtility.ToJson(data, true);
        string path = Application.persistentDataPath + "/saved_level.json";

        File.WriteAllText(path, json);

        Debug.Log("Level saved to: " + path);
    }
    
    private TileType[] FlattenGrid(TileType[,] grid)
    {
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);

        TileType[] flat = new TileType[width * height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                flat[x + y * width] = grid[x, y];
            }
        }

        return flat;
        
    }
    
    private float ScoreEnemyDifficulty()
    {
        int enemies = CountEnemies();

        float maxExpected = _maxAmountOfEnemies * 1.5f;

        return Mathf.Clamp01(enemies / maxExpected);
    }
    
    private float ScoreGapDifficulty()
    {
        int gaps = CountGaps();

        float maxExpected = GridWidth / 6f; 

        return Mathf.Clamp01(gaps / maxExpected);
    }

    public float EstimateDifficulty()
    {
        float difficulty = 0f;

        difficulty += ScoreEnemyDifficulty();
        difficulty += ScoreGapDifficulty();
        difficulty += ScoreTerrainVariation();
        
        return difficulty;
    }

    public GeneratorParameters GetCurrentParameters()
    {
        return new GeneratorParameters
        {
            MinGroundHeight = MinGroundHeight,
            MaxGroundHeight = MaxGroundHeight,

            GapChance = GapChance,
            MinGap = MinGap,
            MaxGap = MaxGap,
            MinDistanceBetweenGaps = MinDistanceBetweenGaps,

            PlatformAttempts = PlatformAttempts,

            MinPlatformLength = MinPlatformLength,
            MaxPlatformLength = MaxPlatformLength,

            MinPlatformHeightFromGround = MinPlatformHeightFromGround,
            MaxPlatformHeightFromGround = MaxPlatformHeightFromGround,

            MinVerticalSeparation = MinVerticalSeparation,

            EnemyDensity = EnemyDensity,
            MinDistanceBetweenEnemies = MinDistanceBetweenEnemies,

            PlatformEnemyRatio = PlatformEnemyRatio
            
        };
    }

    public TileType[,] GetTileGrid()
    {
        return TileGrid;
    }
    
    public TileType[,] GetDecorationGrid()
    {
        return DecorationGrid;
    }
    
    
}
