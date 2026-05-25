public struct GeneratorState
{
    public int X;
    public int GroundHeight;
    public bool InGap;
    public int DistanceFromLastEnemy;
    public int DifficultyBucket;

    public override int GetHashCode()
    {
        int hash = 17;

        hash = hash * 31 + (X / 5);
        hash = hash * 31 + GroundHeight;
        hash = hash * 31 + (InGap ? 1 : 0);
        hash = hash * 31 + (DistanceFromLastEnemy / 3);
        hash = hash * 31 + DifficultyBucket;

        return hash;
    }
}
