using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project
{
    public class Data
    {
        private Dictionary<string, Dictionary<string, double>> data;
        private int numOfRanks;
        private Random rnd = new Random();

        public Data()
        {
            data = new Dictionary<string, Dictionary<string, double>>();
            numOfRanks = 0;
        }

        public Data (Data toCopy)
        {
            data = new Dictionary<string, Dictionary<string, double>>();
            numOfRanks = 0;
            List<string> usersToCopy = toCopy.getUsers();
            List<string> userBusinesses;
            foreach (string user in usersToCopy)
            {
                userBusinesses = toCopy.getUserBusinesses(user);
                data[user] = new Dictionary<string, double>();
                foreach (string business in userBusinesses)
                {
                    data[user][business] = toCopy.getRank(user, business);
                    numOfRanks++;
                }
            }
        }

        public Dictionary<string, double> getUserBusinessesDic(string userID)
        {
            return data[userID];
        }

        public void addToDic(string userID, string businessID, double rank)
        {
            if (!data.ContainsKey(userID))
                data[userID] = new Dictionary<string, double>();
            data[userID][businessID] = rank;
            numOfRanks++;
        }

        public int getNumOfRanks()
        {
            return numOfRanks;
        }

        public List<string> getUsers()
        {
            return data.Keys.ToList();
        }

        public List<string> getUserBusinesses(string userID)
        {
            return data[userID].Keys.ToList();
        }

        public double getRank(string userID, string businessID)
        {
            return data[userID][businessID];
        }

        public Dictionary<string, double> getKRanksOfUser(string userID, int remainingRecordsForTest)
        {
            Dictionary<string, double> result = new Dictionary<string, double>();
            int k = rnd.Next(0, data[userID].Count + 1);
            k = Math.Min(data[userID].Count - k, remainingRecordsForTest);
            List<string> userRankedItems = data[userID].Keys.ToList();
            string currItem;
            for (int i = 0; i < k; i++)
            {
                currItem = userRankedItems[i];
                result[currItem] = data[userID][currItem];
                data[userID].Remove(currItem);
                numOfRanks--;
            }
            if (data[userID].Count == 0)
                data.Remove(userID);
            return result;
        }

        public void merge (Data toMerge)
        {
            foreach (string user in toMerge.data.Keys)
            {
                if (!data.ContainsKey(user))
                {
                    data[user] = toMerge.data[user];
                    numOfRanks += toMerge.data[user].Count;
                }
                else
                {
                    foreach (string business in toMerge.data[user].Keys)
                    {
                        if (!data[user].ContainsKey(business))
                        {
                            data[user][business] = toMerge.data[user][business];
                            numOfRanks++;
                        }
                    }
                }
            }
        }
    }
}
