using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LevelDataGenerator : MonoBehaviour
{
    [SerializeField] private GameObject Player;
    
    [Header("Prefabs and Parents")]
    [SerializeField] private int BlockSize;
    [SerializeField] private Transform BlockParent;
    [SerializeField] private Transform EdgeParent;
    [SerializeField] private GameObject GroundBlock;
    [SerializeField] private GameObject PlatformBlock;
    [SerializeField] private GameObject LeftEdgeTrigger;
    [SerializeField] private GameObject RightEdgeTrigger;
    [SerializeField] private GameObject EnemyPerfab;
    [SerializeField] private Transform EnemyParent;
    [SerializeField] private GameObject DeathPlane;


    private LevelData _levelData;
    private TileType[,] _tileGrid;
    private TileType[,] _decorationGrid;
    private int[] _groundHeight;
    private int _width;
    private int _height;
    private int _enemyCount;

    private void Awake()
    {
        _levelData = LoadLevel();
        
        if(_levelData == null) return;
        
        _width = _levelData.width;
        _height = _levelData.height;
    }

    private void Start()
    {
        if(_levelData == null) return;
        
        _tileGrid = UnflattenGrid(_levelData.tileGrid, _width, _height);
        _decorationGrid = UnflattenGrid(_levelData.decorationGrid, _width, _height);
        
        RebuildGroundHeight();
        InstantiateBlocks();
        InstantiateEnemies();
        PlaceDeathPlane();

        _enemyCount = EnemyCount();
        Debug.Log("LV GEN: " +_enemyCount);
        
        Vector2Int spawn = SetPlayerSpawnPoint();

        Vector3 worldPos = GridCoordinatesToWorldCoordinates(spawn.x, spawn.y);
        Player.transform.position = worldPos;

    }

    private void InstantiateBlocks()
    {
        if (_tileGrid == null) return;
        
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
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

    private void InstantiateEnemies()
    {
        if (_tileGrid == null) return;
        
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                Vector3 position = new Vector3(x * BlockSize, y * BlockSize, 0);

                if (_tileGrid[x, y] == TileType.Enemy) Instantiate(EnemyPerfab, position, Quaternion.identity, EnemyParent);
            }
        }
    }

    private void PlaceDeathPlane()
    {
        if (_tileGrid == null) return;

        Vector3 deathPlanePos = GridCoordinatesToWorldCoordinates((_width / 2), (-_height / 2) - 5);

        var deathPlane = Instantiate(DeathPlane, deathPlanePos, Quaternion.identity);

        deathPlane.transform.localScale = new Vector3(_width * 1.5f, _height);

    }
    
    private Vector3 GridCoordinatesToWorldCoordinates(int x, int y)
    {
        return new Vector3(x * BlockSize, y * BlockSize, 0f);
    }
    
    private LevelData LoadLevel()
    {
        string path = Application.persistentDataPath + "/saved_level.json";

        if (!File.Exists(path))
        {
            Debug.LogError("No saved level found!");
            return null;
        }

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<LevelData>(json);
    }
    
    private TileType[,] UnflattenGrid(TileType[] flat, int width, int height)
    {
        TileType[,] grid = new TileType[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = flat[x + y * width];
            }
        }

        return grid;
    }
    
    private void RebuildGroundHeight()
    {
        _groundHeight = new int[_width];

        for (int x = 0; x < _width; x++)
        {
            int highestGround = -1;

            for (int y = 0; y < _height; y++)
            {
                if (_tileGrid[x, y] == TileType.Ground)
                {
                    highestGround = y;
                }
            }

            _groundHeight[x] = highestGround;
        }
    }
    private Vector2Int SetPlayerSpawnPoint()
    {
        for (int x = 0; x < _width; x++)
        {
            // Skip gaps
            if (_groundHeight[x] < 0) continue;

            int spawnY = _groundHeight[x] + 2;

            // Make sure it's inside grid
            if (spawnY >= _height) continue;

            // Make sure space is empty (no enemies or decorations)
            if (_tileGrid[x, spawnY] == TileType.Empty && _decorationGrid[x, spawnY] == TileType.Empty) return new Vector2Int(x, spawnY);
            
        }

        // Fallback (should never happen unless grid is broken)
        return new Vector2Int(0, _height - 1);
    }
    
    private int EnemyCount()
    {
        int count = 0;

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (_tileGrid[x, y] == TileType.Enemy)
                    count++;
            }
        }

        return count;
    }

    public int GetEnemyCount()
    {
        return _enemyCount;
    }
    private void OnDrawGizmos()
    {
        if (_tileGrid == null) return;

        Vector3 blockSizeVector = new Vector3(BlockSize, BlockSize, 0);
        
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                Vector3 position = new Vector3(x * BlockSize, y * BlockSize, 0);

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
