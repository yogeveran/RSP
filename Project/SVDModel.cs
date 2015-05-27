using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project
{
    class SVDModel
    {
        private double miu;

        private Dictionary<string, double> Bi = null;
        private Dictionary<string, double> Bu = null;

        private Dictionary<string, Vector> Pu = null;
        private Dictionary<string, Vector> Qi = null;


        public double PredictRating(string sUID, string sIID)
        {
            if (Bi.ContainsKey(sIID) && Bu.ContainsKey(sUID))
                return miu + Bi[sIID] + Bu[sUID] + (Pu[sUID] * Qi[sIID]);
            else
                return miu;
        }

        public SVDModel merge(SVDModel other)
        {
            if (other == null)
                throw new ArgumentNullException();
            if (Bi == null)
                throw new InvalidOperationException("SVDModel must be trained before merge");
        }

        public void trainBaseModel(int cFeatures){

        }
    }
}
