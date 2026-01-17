namespace TripleDots
{
    public interface IMergeService
    {
        MergeResult CalculateMerge(HexCellData targetCell, GridData gridData, int maxStackSize);
        bool CanMerge(HexStackData source, HexStackData target);
    }
}
