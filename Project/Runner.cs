using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project
{
    class Runner
    {
        public static int cFeatures = 10, LOOP = 1;
        public static string fileName = @"C:\Users\eranyogev\Documents\לימודים\סמסר ח\Recommendation Systems\Assignment 1\yelp_training_set\yelp_training_set_review.json";
        public static double trainSetSize = 0.95, SmallestDBSize = 0.1;
        private int sizeOfTest = 0;
        private static int totalNumOfRanks = 0;
        
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
                if (i == 0)
                    regRun.Load(fileName, trainSetSize, 1.0, true);
                else
                    regRun.Load(fileName, trainSetSize, 1.0, false);
                SVDModel svd = regRun.RunAlgo(0, regRun.list.Count - 1);
                TimeSpan span = DateTime.Now.Subtract(regTime1);
                regularTrainList.Add(span);
                #endregion
            }

            List<TimeSpan> quickTrainList = new List<TimeSpan>();
            List<TimeSpan> fullTrainList = new List<TimeSpan>();
            Runner runner = null;
            SVDModel quickModel = null, fullyTrainedSVD = null;
            
            for (int i = 0; i < LOOP; i++) { 
                #region take_algorithm_avg_loop
                DateTime time = DateTime.Now;
                runner = new Runner();
                runner.Load(fileName, trainSetSize, SmallestDBSize, false);
                quickModel = runner.RunAlgo(0,runner.list.Count-1);
                DateTime time2 = DateTime.Now;
                fullyTrainedSVD = new SVDModel(quickModel);
                DateTime time3 = DateTime.Now; //Ignore Time of DeepCopyConstructor
                fullyTrainedSVD.trainBaseModel(cFeatures, false);
                DateTime time4 = DateTime.Now;
                double sim = quickModel.similiarity(fullyTrainedSVD);
    
                TimeSpan quickTrain = time2.Subtract(time);
                TimeSpan fullTrain = time4.Subtract(time).Subtract(time3.Subtract(time2));

                quickTrainList.Add(quickTrain);
                fullTrainList.Add(fullTrain);

                #endregion
            }
            double dConfidence,ourRMSE,svdRMSE;
            runner.Compute_RMSE_and_Confidence(quickModel, fullyTrainedSVD, out dConfidence, out ourRMSE, out svdRMSE);

            Console.WriteLine("Distance from quickSVD to SVD: " + quickModel.similiarity(fullyTrainedSVD));

            Console.WriteLine("Average seconds time for quick train is: " + quickTrainList.Average(x => x.Seconds));
            Console.WriteLine("Average seconds time for full train is: " + fullTrainList.Average(x => x.Seconds));
            Console.WriteLine("Average seconds time for regular SVD train is: " + regularTrainList.Average(x => x.Seconds));
            Console.WriteLine("Quick RMSE:" + ourRMSE+", SVD RMSE:"+svdRMSE+", Diff = " + (ourRMSE-svdRMSE));
            Console.WriteLine("quickSVD is better than regular SVD with confidence: " + dConfidence);
            Console.ReadLine();
        }

        public void Load(string sFileName, double dTrainSetSize,double sizeOfSmallestSVDModel, bool isFirstTime){
            string jsonLine = "";
            StreamReader r = new StreamReader(sFileName);
            jsonLine = r.ReadLine();
            JObject record;
            string user_id, business_id;
            int rank, sizeOfCurrentTrain = 0, numOfModels = 1;
            Data data = new Data();
            int requestedDataSize = (int) (totalNumOfRanks * sizeOfSmallestSVDModel) + 1;
            while (jsonLine != null)
            {
                record = JObject.Parse(jsonLine);
                user_id = record["user_id"].ToString();
                business_id = record["business_id"].ToString();
                rank = int.Parse(record["stars"].ToString());
                data.addToDic(user_id, business_id, rank);
                if (isFirstTime)
                    totalNumOfRanks++;
                if (sizeOfSmallestSVDModel != 1.0)
                {
                    if (sizeOfCurrentTrain + 1 == requestedDataSize)
                    {
                        list.Add(new SVDModel(data));
                        numOfModels++;
                        data = new Data();
                        sizeOfCurrentTrain = 0;
                    }
                    else
                        sizeOfCurrentTrain++;
                }
                jsonLine = r.ReadLine(); 
            }
            list.Add(new SVDModel(data));
            if (sizeOfSmallestSVDModel == 1.0)
                loadToTestset(1 - dTrainSetSize, 0, 0);
            else
                loadToTestset(1 - dTrainSetSize, 0, numOfModels - 1);
        }

        private void loadToTestset(double testsetSize, int startIndexInList, int endIndexInList)
        {
            SVDModel currModel = list[0];
            List<string> users = list[0].getDataUsers();
            int index = 0, modelIndex = 0;
            int testNumOfRecords = (int)(totalNumOfRanks * testsetSize);
            Random rnd = new Random();
            Dictionary<string, int> moveToTest;
            string currUser;
            bool lastModel = false;
            while (testNumOfRecords > 0)
            {
                if (endIndexInList > 0)
                {
                    currModel = list[modelIndex];
                    users = currModel.getDataUsers();
                    if (modelIndex < endIndexInList)
                    {
                        modelIndex++;
                        lastModel = false;
                    }
                    else
                    {
                        modelIndex = startIndexInList;
                        lastModel = true;
                    }
                }
                currUser = users[index]; 
                moveToTest = currModel.getKRanksOfUser(currUser, testNumOfRecords);
                if (moveToTest.Count > 0)
                {
                    if (testset.ContainsKey(currUser))
                    {
                        foreach(string record in moveToTest.Keys)
                        {
                            if (testset[currUser].ContainsKey(record))
                                Console.WriteLine("what");
                            testset[currUser].Add(record, moveToTest[record]);
                        }
                    }
                    else
                        testset[currUser] = moveToTest;
                    testNumOfRecords -= moveToTest.Count;
                    sizeOfTest += moveToTest.Count;
                }
                if (endIndexInList == 0 || lastModel)
                    index++;
            }
        }

        public SVDModel RunAlgo(int from, int to)
        {
            if ((from < 0) || (to > list.Count-1))
                throw new ArgumentException();
            if (to == from)
            {
                list[from].trainBaseModel(cFeatures, true);
                return list[from];
            }
            int pivot;
            if (to - from > 1)
            {
                if (((to-from+1)/2)% 2 == 1) //number of pairs
                    pivot = (from + (to - from) / 2) + 1;
                else
                    pivot = from + (to - from) / 2;
            }
            else
                pivot = from;

            SVDModel a=null,b=null;
            Task taskA = Task.Factory.StartNew(() => a = RunAlgo(from, pivot));
            Task taskB = Task.Factory.StartNew(() => b = RunAlgo(pivot+1, to));
            taskA.Wait();
            taskB.Wait();
            return a.merge(b);
        }

        public void Compute_RMSE_and_Confidence(SVDModel quickModel, SVDModel SVD, out double dConfidence, out double ourRMSE, out double svdRMSE)
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
                    OURpredictedRankSVD = quickModel.PredictRating(user, item);
                    OURerror = testset[user][item]-OURpredictedRankSVD;
                    OURsquared = Math.Pow(OURerror, 2);
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
