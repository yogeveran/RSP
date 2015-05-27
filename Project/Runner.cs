using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project
{
    class Runner
    {
        List<SVDModel> list = new List<SVDModel>();
        private Dictionary<string, Dictionary<string, int>> testset = new Dictionary<string, Dictionary<string, int>>();
        static void Main(string[] args)
        {
            Runner runner = new Runner();
            runner.Load("",0.95,0.1);
        }

        public void Load(string sFileName, double dTrainSetSize,double sizeOfSmallestSVDModel){
            throw new NotImplementedException();
        }

        public void RunAlgo()
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, double> ComputeRMSE(List<string> lMethods, out Dictionary<string, Dictionary<string, double>> dConfidence)
        {
            throw new NotImplementedException();
        }
    }
}
