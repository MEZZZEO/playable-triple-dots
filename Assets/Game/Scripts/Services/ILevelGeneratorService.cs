using System.Collections.Generic;

namespace TripleDots
{
    public interface ILevelGeneratorService
    {
        GeneratedLevel Generate();
        GeneratedLevel Generate(int seed);
        List<HexStackData> GenerateNewPlayerStacks(GridData gridData, int count);
    }
}