namespace ATCG.HexGrids
{
    public interface IHexMember
    {
        void LeaveCell(HexCell hexCell);
        void EnterCell(HexCell hexCell);
    }
}