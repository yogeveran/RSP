using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project
{
    class Runner
    {
        public static int cFeatures = 10;
        List<SVDModel> list = new List<SVDModel>();
        private Dictionary<string, Dictionary<string, int>> testset = new Dictionary<string, Dictionary<string, int>>();
        static void Main(string[] args)
        {
            DateTime time = DateTime.Now;
            Runner runner = new Runner();
            runner.Load("",0.95,0.1);
            SVDModel model = runner.RunAlgo();
            DateTime time2 = DateTime.Now;
            SVDModel toTrain = new SVDModel(model);
            DateTime time3 = DateTime.Now; //Ignore Time of DeepCopyConstructor
            toTrain.trainBaseModel(cFeatures);

        }

        public void Load(string sFileName, double dTrainSetSize,double sizeOfSmallestSVDModel){
            throw new NotImplementedException();
        }

        public SVDModel RunAlgo()
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, double> ComputeRMSE(List<string> lMethods, out Dictionary<string, Dictionary<string, double>> dConfidence)
        {
            throw new NotImplementedException();
        }
    }
}
