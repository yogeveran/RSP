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
        private int sizeOfTest = 0;
        
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
            double predictedRankSVD, error, squared,OURpredictedRankSVD,OURerror,OURsquared;
            double userRMSESVD=0, moneSVD=0,OURuserRMSESVD=0,OURmoneSVD=0;
            int AScore = 0, BScore = 0;
            int nA = 0, nB = 0;
            
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
                    OURerror = testset[user][item]-OURpredictedRankSVD;
                    OURsquared = Math.Pow(error, 2);
                    OURuserRMSESVD += OURsquared;
                    OURmoneSVD += OURsquared;
                    #endregion
                }

                userRMSESVD /= testset[user].Count;
                computeRewards(OURuserRMSESVD, userRMSESVD, out AScore, out BScore);
                nA += AScore;
                nB += BScore;
            }
            //update result:
            svdRMSE = Math.Sqrt(moneSVD / sizeOfTest);
            ourRMSE = Math.Sqrt(OURmoneSVD / sizeOfTest);
            //update confidence:
            dConfidence = signTest(nA, nB);
        }

        private void computeRewards(double ARMSE, double BRMSE, out int AScore, out int BScore)
        {
            if (ARMSE > BRMSE)
            {
                AScore = 0;
                BScore = 1;
            }
            else if (ARMSE == BRMSE)
            {
                AScore = 0;
                BScore = 0;
            }
            else
            {
                AScore = 1;
                BScore = 0;
            }
        }
        private double signTest(int nA, int nB)
        {
            double result = 0;
            int n = nA + nB;
            for (int i = nA; i <= n; i++)
            {
                result += binom(n, i);
            }
            double mult = Math.Pow(0.5, n);
            result *= mult;
            return 1 - result;
        }

        public double binom(int n, int k)
        {
            double answer = 0;
            for (int i = k + 1; i <= n; i++)
            {
                answer += Math.Log(i, 2);
            }
            for (int j = 1; j <= n - k; j++)
            {
                answer -= Math.Log(j, 2);
            }
            answer = Math.Pow(2, answer);
            return answer;
        }
    }
}
