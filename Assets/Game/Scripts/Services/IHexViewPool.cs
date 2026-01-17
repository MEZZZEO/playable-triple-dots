namespace TripleDots
{
    public interface IHexViewPool
    {
        void Initialize();
        HexPieceView Get();
        void Return(HexPieceView view);
        void Prewarm(int count);
    }
}
