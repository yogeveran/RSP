using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project
{
    class SVDModel
    {
        private Data data = null;
        private double miu { get; set; }
        private Random rnd;
        private Dictionary<string, double> Bi = null;
        private Dictionary<string, double> Bu = null;

        private Dictionary<string, Vector> Pu = null;
        private Dictionary<string, Vector> Qi = null;
        //private SVDModel model;



        public SVDModel(Data data){
            this.data = data;
            miu = 0;
            rnd = new Random();
        }

        public SVDModel(SVDModel model)
        {
            data = new Data(model.data);
            rnd = new Random();
            miu = model.miu;
            Bu = new Dictionary<string, double>();
            Bi = new Dictionary<string, double>();
            Pu = new Dictionary<string, Vector>();
            Qi = new Dictionary<string, Vector>();
            foreach (string user in model.Bu.Keys)
            {
                Bu[user] = model.Bu[user];
                Pu[user] = new Vector(model.Pu[user]);
            }
            foreach (string business in model.Bi.Keys)
            {
                Bi[business] = model.Bi[business];
                Qi[business] = new Vector(model.Qi[business]);
            }
        }

        public List<string> getDataUsers ()
        {
            return data.getUsers();
        }

        public Dictionary<string, int> getKRanksOfUser(string userID, int remainingRecordsForTest)
        {
            return data.getKRanksOfUser(userID, remainingRecordsForTest);
        }

        public double PredictRating(string sUID, string sIID)
        {
            if (Bi.ContainsKey(sIID) && Bu.ContainsKey(sUID))
                return miu + Bi[sIID] + Bu[sUID] + (Pu[sUID] * Qi[sIID]);
            else
                return miu;
        }

        public SVDModel merge(SVDModel other)
        {
            #region check_not_null
            if (other == null)
                throw new ArgumentNullException();
            if (Bi == null || Bu == null || Pu == null || Qi == null || miu == double.NaN)
                throw new InvalidOperationException("SVDModel must be trained before merge");
            #endregion
            #region BiUpdate
            foreach (string item in other.Bi.Keys) {
                if (this.Bi.ContainsKey(item))
                    this.Bi[item] = (this.Bi[item] + other.Bi[item]) / 2.0;
                else
                    this.Bi[item] = other.Bi[item];
            }
            #endregion
            #region BuUpdate
            foreach (string user in other.Bu.Keys)
            {
                if (this.Bu.ContainsKey(user))
                    this.Bu[user] = (this.Bu[user] + other.Bu[user]) / 2.0;
                else
                    this.Bu[user] = other.Bu[user];
            }
            #endregion
            #region QiUpdate
            foreach (string item in other.Qi.Keys)
            {
                if (this.Qi.ContainsKey(item))
                    this.Qi[item] = (this.Qi[item] + other.Qi[item]) / 2.0;
                else
                    this.Qi[item] = other.Qi[item];
            }
            #endregion
            #region PuUpdate
            foreach (string user in other.Pu.Keys)
            {
                if (this.Pu.ContainsKey(user))
                    this.Pu[user] = (this.Pu[user] + other.Pu[user]) / 2.0;
                else
                    this.Pu[user] = other.Pu[user];
            }
            #endregion
            data.merge(other.data);
            return this;
        }

        public void trainBaseModel(int cFeatures, bool isNewModel){
            if (isNewModel)
            {
                Bi = new Dictionary<string, double>();
                Bu = new Dictionary<string, double>();
                Pu = new Dictionary<string, Vector>();
                Qi = new Dictionary<string, Vector>();
            }
            Dictionary<string, Dictionary<string, int>> train = new Dictionary<string, Dictionary<string, int>>();
            Dictionary<string, Dictionary<string, int>> validation = new Dictionary<string, Dictionary<string, int>>();
            int requestedTrainSize = data.getNumOfRanks() / 2;
            List<string> users = data.getUsers();
            List<string> businesses;
            int counter = 0, validationSize = 0;
            for (int i = 0; i < users.Count; i++)
            {
                string user = users[i];
                businesses = data.getUserBusinesses(user);
                if (counter < requestedTrainSize)
                {
                    int rand1 = 1, rand2 = 1;
                    int k = rnd.Next(0, businesses.Count + 1);
                    if (k != 0)
                    {
                        train[user] = new Dictionary<string, int>();
                        if (isNewModel || !Pu.ContainsKey(user))
                            Pu[user] = new Vector(cFeatures);
                    }
                    for (int j = 0; j < k; j++)
                    {
                        string item = businesses[j];
                        train[user][item] = data.getRank(user, item);
                        counter++;
                        rand1 = rnd.Next(1, 1000);
                        do
                        {
                            rand2 = rnd.Next(1, 1000);
                        } while (rand2 == rand1);
                        if (!Bi.ContainsKey(item))
                            Bi[item] = 1 / (rand1 - rand2);
                        if (isNewModel)
                            miu += train[user][item];
                        if (!Qi.ContainsKey(item))
                            Qi[item] = new Vector(cFeatures);
                    }
                    if (k != 0 && (isNewModel || !Bu.ContainsKey(user)))
                        Bu[user] = 1 / (rand2 - rand1);
                    if (k != businesses.Count)
                        validation[user] = new Dictionary<string, int>();
                    for (int j = k; j < businesses.Count; j++)
                    {
                        validation[user][businesses[j]] = data.getRank(user, businesses[j]);
                        validationSize++;
                    }
                }
                else
                    validation[user] = data.getUserBusinessesDic(user);
            }
            if (isNewModel)
                miu /= counter;
            double e, oldRMSE, newRMSE = double.MaxValue;
            double gamma = 0.01, lambda = 0.01;
            do
            {
                foreach (string user in train.Keys)
                {
                    foreach (string item in train[user].Keys)
                    {
                        e = train[user][item] - miu - Bu[user] - Bi[item] - (Pu[user] * Qi[item]);
                        Bu[user] += gamma * (e - lambda * Bu[user]);
                        Bi[item] += gamma * (e - lambda * Bi[item]);
                        Qi[item] = Qi[item] + ((Pu[user] * e - Qi[item] * lambda) * gamma);
                        Pu[user] = Pu[user] + ((Qi[item] * e - Pu[user] * lambda) * gamma);
                    }
                }
                double mone = 0, prediction;
                foreach (string user in validation.Keys)
                {
                    foreach (string item in validation[user].Keys)
                    {
                        if (Bi.ContainsKey(item) && Bu.ContainsKey(user))
                            prediction = miu + Bi[item] + Bu[user] + (Pu[user] * Qi[item]);
                        else
                            prediction = miu;
                        mone += Math.Pow(prediction - validation[user][item], 2);
                    }
                }
                oldRMSE = newRMSE;
                newRMSE = mone;
            } while (newRMSE < oldRMSE);
        }


        public double similiarity(SVDModel other)
        {
            if (other == null)
                throw new ArgumentNullException();
            double res = Math.Abs(miu-other.miu);
            foreach (string str in Qi.Keys)
                res += Vector.dist(Qi[str], other.Qi[str]);
            foreach (string str in Pu.Keys)
                res += Vector.dist(Pu[str], other.Pu[str]);
            foreach (string str in Bi.Keys)
                res += Math.Abs(Bi[str] - other.Bi[str]);
            foreach (string str in Bu.Keys)
                res += Math.Abs(Bu[str] - other.Bu[str]);
            return res;
        }
    }
}
