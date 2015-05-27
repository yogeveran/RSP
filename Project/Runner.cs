using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project
{
    class Runner
    {
        public static int cFeatures = 10, LOOP = 10;
        public static string fileName = "";
        public static double trainSetSize = 0.95, SmallestDBSize = 0.1;
        
        List<SVDModel> list = new List<SVDModel>();
        private Dictionary<string, Dictionary<string, int>> testset = new Dictionary<string, Dictionary<string, int>>();


        static void Main(string[] args)
        {
            List<TimeSpan> regularTrainList = new List<TimeSpan>();
            for (int i = 0; i < LOOP; i++)
            {
                #region take_svd_avg_loop
                DateTime regTime1 = DateTime.Now;
                Runner regRun = new Runner();
                regRun.Load(fileName, trainSetSize, 1.0);
                SVDModel svd = regRun.RunAlgo();
                TimeSpan span = DateTime.Now.Subtract(regTime1);
                regularTrainList.Add(span);
                #endregion
            }

            List<TimeSpan> quickTrainList = new List<TimeSpan>();
            List<TimeSpan> fullTrainList = new List<TimeSpan>();
            Runner runner = null;
            SVDModel model = null, fullyTrainedSVD = null;
            
            for (int i = 0; i < LOOP; i++) { 
                #region take_algorithm_avg_loop
                DateTime time = DateTime.Now;
                runner = new Runner();
                runner.Load(fileName, trainSetSize, SmallestDBSize);
                model = runner.RunAlgo();
                DateTime time2 = DateTime.Now;
                fullyTrainedSVD = new SVDModel(model);
                DateTime time3 = DateTime.Now; //Ignore Time of DeepCopyConstructor
                fullyTrainedSVD.trainBaseModel(cFeatures);
                DateTime time4 = DateTime.Now;
                double sim = model.similiarity(fullyTrainedSVD);
    
                TimeSpan quickTrain = time2.Subtract(time);
                TimeSpan fullTrain = time4.Subtract(time).Subtract(time3.Subtract(time2));

                quickTrainList.Add(quickTrain);
                fullTrainList.Add(fullTrain);

                #endregion
            }
            double dConfidence,ourRMSE,svdRMSE;
            runner.Compute_RMSE_and_Confidence(model, fullyTrainedSVD, out dConfidence, out ourRMSE, out svdRMSE);

            Console.WriteLine("Average seconds time for quick train is: " + quickTrainList.Average(x => x.Seconds));
            Console.WriteLine("Average seconds time for full train is: " + fullTrainList.Average(x => x.Seconds));
            Console.WriteLine("Average seconds time for regular SVD train is: " + regularTrainList.Average(x => x.Seconds));
            Console.WriteLine("quickSVD is better than SVD with confidence: " + dConfidence);

        }

        public void Load(string sFileName, double dTrainSetSize,double sizeOfSmallestSVDModel){
            throw new NotImplementedException();
        }

        public SVDModel RunAlgo()
        {
            throw new NotImplementedException();
        }

        public void Compute_RMSE_and_Confidence(SVDModel ourModel, SVDModel SVD, out double dConfidence, out double ourRMSE, out double svdRMSE)
        {
            throw new NotImplementedException();
        }
    }
}
