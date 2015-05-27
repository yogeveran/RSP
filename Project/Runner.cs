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
                SVDModel svd = regRun.RunAlgo(0, regRun.list.Count - 1);
                TimeSpan span = DateTime.Now.Subtract(regTime1);
                regularTrainList.Add(span);
                #endregion
            }

            List<TimeSpan> quickTrainList = new List<TimeSpan>();
            List<TimeSpan> fullTrainList = new List<TimeSpan>();
            Runner runner = null;
            SVDModel ourModel = null, fullyTrainedSVD = null;
            
            for (int i = 0; i < LOOP; i++) { 
                #region take_algorithm_avg_loop
                DateTime time = DateTime.Now;
                runner = new Runner();
                runner.Load(fileName, trainSetSize, SmallestDBSize);
                ourModel = runner.RunAlgo(0,runner.list.Count-1);
                DateTime time2 = DateTime.Now;
                fullyTrainedSVD = new SVDModel(ourModel);
                DateTime time3 = DateTime.Now; //Ignore Time of DeepCopyConstructor
                fullyTrainedSVD.trainBaseModel(cFeatures);
                DateTime time4 = DateTime.Now;
                double sim = ourModel.similiarity(fullyTrainedSVD);
    
                TimeSpan quickTrain = time2.Subtract(time);
                TimeSpan fullTrain = time4.Subtract(time).Subtract(time3.Subtract(time2));

                quickTrainList.Add(quickTrain);
                fullTrainList.Add(fullTrain);

                #endregion
            }
            double dConfidence,ourRMSE,svdRMSE;
            runner.Compute_RMSE_and_Confidence(ourModel, fullyTrainedSVD, out dConfidence, out ourRMSE, out svdRMSE);
            Console.WriteLine("Distance from quickSVD to SVD: ")

            Console.WriteLine("Average seconds time for quick train is: " + quickTrainList.Average(x => x.Seconds));
            Console.WriteLine("Average seconds time for full train is: " + fullTrainList.Average(x => x.Seconds));
            Console.WriteLine("Average seconds time for regular SVD train is: " + regularTrainList.Average(x => x.Seconds));
            Console.WriteLine("quickSVD is better than regular SVD with confidence: " + dConfidence);



        }

        public void Load(string sFileName, double dTrainSetSize,double sizeOfSmallestSVDModel){
            throw new NotImplementedException();
        }

        public SVDModel RunAlgo(int from, int to)
        {
            if ((from > to) || (to > list.Count-1))
                throw new ArgumentException();
            if (to == from)
            {
                list[from].trainBaseModel(cFeatures);
                return list[from];
            }
            int pivot = from + (to - from) / 2;

            SVDModel a=null,b=null;
            Task taskA = Task.Factory.StartNew(() => a = RunAlgo(from, pivot));
            Task taskB = Task.Factory.StartNew(() => b = RunAlgo(pivot+1, pivot));
            taskA.Wait();
            taskB.Wait();
            return a.merge(b);
        }

        public void Compute_RMSE_and_Confidence(SVDModel ourModel, SVDModel SVD, out double dConfidence, out double ourRMSE, out double svdRMSE)
        {
            double predictedRankSVD, error, squared;
            double userRMSESVD=0, moneSVD=0;
            int AScore = 0, BScore = 0;
            int[,] nAnB = new int[4,4]; //0- pearson, 1- cosine, 2- random, 3- svd
            int counter = 0;
            foreach(string user in testset.Keys)
            {
                foreach(string item in testset[user].Keys)
                {
                    counter++;
                    #region svd_calc
                    predictedRankSVD = SVD.PredictRating(user, item);
                    error = testset[user][item]-predictedRankSVD;
                    squared = Math.Pow(error, 2);
                    userRMSESVD += squared;
                    moneSVD += squared;
                    #endregion

                    #region our_calc
                    OURpredictedRankSVD = ourModel.PredictRating(user, item);
                    OURerror = testset[user][item]-predictedRankSVD;
                    OURsquared = Math.Pow(error, 2);
                    OURuserRMSESVD += squared;
                    OURmoneSVD += squared;
                    #endregion
                }

                userRMSESVD /= testset[user].Count;
                computeRewards(userRMSESVD, OURuserRMSESVD, out AScore, out BScore);
            }
            //update result:
            svdRMSE = Math.Sqrt(moneSVD / sizeOfTest);
            //update confidence:

                dConfidence =  = signTest(AScore, BScore);
        }
        }
    }
}
