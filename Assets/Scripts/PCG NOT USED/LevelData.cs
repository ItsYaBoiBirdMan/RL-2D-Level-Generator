[System.Serializable]

public class LevelData
{
    public GeneratorParameters parameters;

    public int width;
    public int height;
    public float score;

    public TileType[] tileGrid;
    public TileType[] decorationGrid;
}
