namespace Maths {

    internal interface ISearchAlg   {

        void Initialize(int nGenes, int dim, double[] mins, double[] maxs, IMeritFunction mf, IValidation va);
        double[][] Search(int maxIteraciones);
        void SetDebug(bool debug);
        bool GetResult();

    }
}