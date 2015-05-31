using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project
{
    public class Data
    {
        private Dictionary<string, Dictionary<string, int>> data;
        private int numOfRanks;
        private Random rnd = new Random();

        public Data()
        {
            data = new Dictionary<string, Dictionary<string, int>>();
            numOfRanks = 0;
        }

        public Data (Data toCopy)
        {
            data = new Dictionary<string, Dictionary<string, int>>();
            numOfRanks = 0;
            List<string> usersToCopy = toCopy.getUsers();
            List<string> userBusinesses;
            foreach (string user in usersToCopy)
            {
                userBusinesses = toCopy.getUserBusinesses(user);
                data[user] = new Dictionary<string, int>();
                foreach (string business in userBusinesses)
                {
                    data[user][business] = toCopy.getRank(user, business);
                    numOfRanks++;
                }
            }
        }

        public Dictionary<string, int> getUserBusinessesDic(string userID)
        {
            return data[userID];
        }

        public void addToDic(string userID, string businessID, int rank)
        {
            if (!data.ContainsKey(userID))
                data[userID] = new Dictionary<string, int>();
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

        public int getRank(string userID, string businessID)
        {
            return data[userID][businessID];
        }

        public Dictionary<string, int> getKRanksOfUser(string userID, int remainingRecordsForTest)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
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
    }
}
