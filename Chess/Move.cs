namespace Chess;

public struct Move
{
    public int fromIndex;
    public int toIndex;
    public Move(int fromIndex, int toIndex)
    {
        this.fromIndex = fromIndex;
        this.toIndex = toIndex;
    }

    public Move(int fromFile, int fromRank, int toFile, int toRank)
    {
        fromIndex = Utils.GetIndexFromFileRank(fromFile, fromRank);
        toIndex = Utils.GetIndexFromFileRank(toFile, toRank);
    }
}