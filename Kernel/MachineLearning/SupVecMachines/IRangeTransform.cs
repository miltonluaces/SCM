namespace MachineLearning  {
    
    //Interface implemented by range transforms.
    public interface IRangeTransform {
        
        // Transform the input value using the transform stored for the provided index.
        double Transform(double input, int index);
        Node[] Transform(Node[] input);
    }
}
