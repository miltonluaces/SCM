#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RDotNet;
using Maths;

#endregion 

namespace MachineLearning {
    
    
    internal class NNMultiRegression {

        
    #region Fields

    private RNNet rnn;
    private MethodType method = MethodType.NN;
    private S4Object model=null;
    
    private int nHidden=0;
    private double dec=-1;
    private int maxIt=100;
    
    private int nTree=100;
    private int mTry=5;
    
    private double[][] data;
    private int p=0;
    
    #endregion 
    
    #region Constructor

    internal NNMultiRegression(string path, string version) {
        this.rnn = new RNNet(path, version);
    }
        
    #endregion
  
    #region Internal Methods

    internal void LoadParamsNeuNet(int nHidden, double dec, int maxIt) {
      this.method = MethodType.NN;
      this.nHidden = nHidden;
      this.dec = dec;
      this.maxIt = maxIt;
    }
    
    internal void LoadParamsRndFor(int nTree, int mTry) {
      this.method = MethodType.RF;
      this.nTree = nTree;
      this.mTry = mTry;
    }
    
    internal void LoadParamsLinMod() {
      this.method = MethodType.LM;
    }
    
    internal void LoadParamsSuVeMa() {
      this.method = MethodType.SVM;
    }
    
    internal void LoadForCalculate(double[][] data) {
      this.data = data;
      this.p = data.Length-1; //ojo
      DF X = new DF(data); //extract first p columns
      AR Y = new AR(1); // data[data.Length]; //extract last column
      switch(method) {
          case MethodType.NN:
              rnn.CreateNN(X, Y, this.nHidden, this.dec, this.maxIt);
              //this.model = nnet(Y~., data, size=this.nHidden, linout=T, trace=F, decay=this.decay, rang=0.5, maxit=this.maxIt);
              break;
          case MethodType.RF:
              //randomForest(Y ~., data=data, type=regression, mtry=min(this.mTry, this.p), importance=TRUE, na.action=na.omit, ntree=this.nTree);
              break;
          case MethodType.LM:
              //lm(Y ~., data=data, na.action=na.omit);
              break;
          case MethodType.SVM:
              //svm(Y ~., data=data, na.action=na.omit, ranges = list(epsilon = seq(0,1,0.1), cost = 2^(2:9)));
              break;
      }
    }
    
    internal double Calculate(double[] x) {
       AR newData = new AR(x);
       double y = rnn.Predict(newData); 
       return y;
    }
    
    #endregion
    
    #region Private Methods
    
    #endregion

    #region Enums
    
    internal enum MethodType { NN, RF, LM, SVM };

    #endregion
    
    }
}
