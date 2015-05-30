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
        private double miu = double.NaN;

        private Dictionary<string, double> Bi = null;
        private Dictionary<string, double> Bu = null;

        private Dictionary<string, Vector> Pu = null;
        private Dictionary<string, Vector> Qi = null;
        private SVDModel model;

        public SVDModel(Data data){
            this.data = data;
        }

        public SVDModel(SVDModel model)
        {
            // TODO: Create Deep Copy Constructor.
            throw new NotImplementedException();
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

            return this;
        }

        public void trainBaseModel(int cFeatures){
            throw new NotImplementedException();
        }


        public double similiarity(SVDModel other)
        {
            if (other == null)
                throw new ArgumentNullException();
            double res = Math.Abs(miu-other.miu);

            foreach(string str in Qi.Keys)
                res += Vector.dist(Qi[str],other.Qi[str]);
            foreach (string str in Qi.Keys)
                res += Vector.dist(Pu[str], other.Pu[str]);
            foreach (string str in Bi.Keys)
                res += Math.Abs(Bi[str] - other.Bi[str]);
            foreach (string str in Bi.Keys)
                res += Math.Abs(Bu[str] - other.Bu[str]);
            return res;
        }
    }
}
