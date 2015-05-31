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

        /*public Dictionary<string,Dictionary<string,int>> getDic()
        {
            return data;
        }*/

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

        /*public List<string> getUserBusinesses (string userID)
        {
            return data[userID].Keys.ToList();
        }*/

        /*public int getRank (string userID, string businessID)
        {
            return data[userID][businessID];
        }*/

        public Dictionary<string,int> getKRanksOfUser (string userID, int remainingRecordsForTest)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            int k = rnd.Next(0, data[userID].Count+1);
            k = Math.Min(data[userID].Count - k, remainingRecordsForTest);
            List<string> userRankedItems = data[userID].Keys.ToList();
            string currItem;
            for (int i = 0; i < k; i++)
            {
                currItem = userRankedItems[i];
                result[currItem] = data[userID][currItem];
                data[userID].Remove(currItem);
            }
            if (data[userID].Count == 0)
                data.Remove(userID);
            return result;
        }
    }
}
