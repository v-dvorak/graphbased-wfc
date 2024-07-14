using GBWFC.Graph;

namespace GBWFC.Solver
{
    public static class ConstraintsExtensions
    {
        public static ConstraintById ConstraintById(this (int, int) constraint) => new ConstraintById(constraint);
        public static ConstraintByNode ConstraintByNode(this (Node, int) constraint) => new ConstraintByNode(constraint);
    }
}
