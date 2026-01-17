using System;
using System.Threading.Tasks;

namespace TripleDots
{
    public interface IChainReactionService
    {
        event Action<ChainStep> OnChainStep;
        event Action OnChainComplete;

        Task<ChainReactionResult> ExecuteAsync(HexCoord startCoord);
        ChainReactionResult CalculateChainReaction(HexCoord startCoord);
    }
}