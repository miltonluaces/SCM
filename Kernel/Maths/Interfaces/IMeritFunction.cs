namespace Maths {

    internal interface IMeritFunction {

        double Evaluate(double[][] genes, object datos);
        bool GetResult();

    }
}
