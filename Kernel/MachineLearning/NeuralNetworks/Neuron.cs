#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace MachineLearning {
    
    internal class Neuron {

        #region Fields
     
        private int net;    
        private int nConex;   
        private int layer;   
        private int index; 
        private double n;  
        private double m;  
        private double[,] matrix;		
        private List<Neuron> conex;
        private int nSinapsis;
        private TransFunctionType function;

        #endregion

        #region Constructor

        internal Neuron(int net, int layer, int index, int nConex) {
           this.net = net;
           this.layer = layer;
           this.index = index;
           this.nConex = nConex;
           conex = new List<Neuron>();
           this.function = TransFunctionType.logsig; // TODO: other function
           this.n = 1.0;
           this.m = 1.0;
           
           matrix = new double[nConex,4];
           for(int i=0;i<nConex;i++) {
               for(int j=0;j<3;j++) { 
                     matrix[i,j] = 0.00;
               }
               matrix[i,3] = net * 100000 + layer * 10000 + index * 100 + i; //id  net.layer.index.conex
           }

           if(layer != 0) { matrix[0,0] = 1.00; } //trend neuron
        }

        #endregion
        
        #region Properties

        internal int Net {
            get { return net; }
            set { net = value; }
        }

        internal int Layer {
            get { return layer; }
            set { layer = value; }
        }

        internal int Index {
            get { return index; }
            set { index = value; }
        }

        internal double M {
            get { return m; }
            set { m = value; }
        }

        internal double N {
            get { return n; }
            set { n = value; }
        }

        internal int NConex {
            get { return nConex; }
        }

        #endregion
        
        #region Setters and Getters

        internal void SetX(double x, int index) {
            matrix[index,0] = x;
        }

        internal double GetX(int index) {
            return matrix[index,0];
        }

        internal void SetW(double w, int index) {
            matrix[index,1] = w;
        }

        internal double GetW(int index) {
            return matrix[index,1];
        }

        internal void SetWPrev(double wPrev, int index) {
            matrix[index,2] = wPrev;
        }

        internal double GetWPrev(int index) {
            return matrix[index,2];
        }

        internal void SetID(long id, int index) {
            matrix[index,3] = id;
        }

        internal long GetID(int index) {
            return (long)matrix[index,3];
        }

        internal double[,] GetMatrix() {
            return matrix;
        }

        internal void AddConex(Neuron n) {
            conex.Add(n);
            //nConex++;
        }

        internal List<Neuron> GetConex() {
             return conex;
        }

        #endregion        

        #region Trainning & Process

        internal double Output() {
             return TransFunction(WeightedSum());
        }

        internal void Transfer() {
            double y;
	        if(layer == 0) { y = matrix[0,0]; } //single entry
            else          { y = Output(); }
	        foreach(Neuron n in conex) {
                n.SetX(y, index);
            }
        }

        internal void Learn(double d) {
            double x, w, wp, delta ;
            for(int i=0;i<nConex;i++) {
                x = GetX(i); 
                w = GetW(i);
                wp = GetWPrev(i);
		
                delta = GetDelta(d);
	            SetW(w + n * delta * x + m * (w - wp),i);  //nðx + momentum
                SetWPrev(w,i);  
            }
        }

        internal void Learn(double[] ds) {
	         double x, w, wp, delta ;
           for(int i=0;i<nConex;i++) { 
	            x = GetX(i); 
            	w = GetW(i);
                wp = GetWPrev(i);
                delta = GetDelta(ds);
		        SetW(w + n * delta * x + m * (w - wp),i);  //nðx + momento
		        SetWPrev(w,i);  
	        }
        }

        #endregion
        
        #region Private Methods

        private double WeightedSum() {
            double ws = 0;
            for(int i=0;i<nConex;i++) {
                ws += matrix[i,0] * matrix[i,1];
            }
            return ws;
        }

        private double TransFunction(double ws) {
            if(function == TransFunctionType.logsig)  {
                double minusWs = - ws;
                double exp = Math.Exp(minusWs);
                double output = 1 / (1 + exp); 
                return output;
            //return 1.00 / (1.00 + Math.Exp(-net)); 
            }
            if(function == TransFunctionType.tansig)  {
                Math.Tanh(ws);
            } 
            else {
                throw new NotImplementedException("Not implemented");
            }
            return -1;
        }

        private double GetDelta(double d) {
            double delta = 0.00;;
            double y = Output();
	
            if(layer == 2) {
                delta = y * (1-y) * (d-y); // y'(d-y)
            }
            
            else if(layer == 1) {
                foreach(Neuron n in conex) { delta = y * (1-y) * (n.GetW(index) * n.GetDelta(d));  }
            }
            return delta;
        }

        private double GetDelta(double[] ds) {
        	double delta = 0.00;
	        double y = Output();
	
        	if(layer == 2) {
		        delta = y * (1-y) * (ds[index]-y); // y'(d-y)
	        }
	    
	        else if(layer == 1) {
                foreach(Neuron n in conex) { delta = delta + y * (1-y) * (n.GetW(index) * n.GetDelta(ds));  }
	        }
	        return delta;
        }

        #endregion

        #region Internal Enum

        private enum TransFunctionType { logsig, tansig };

        #endregion

    }
}
