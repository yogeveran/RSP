using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Project
{
    public class Vector
    {
        private double[] vector;
        public static Random rnd = new Random();

        public Vector(int size)
        {
            double rand1, rand2;
            vector = new double[size];
            for (int i=0; i<size; i++)
            {
                rand1 = rnd.Next(100, 800);
                do
                {
                    rand2 = rnd.Next(100, 800);
                } while (rand1 == rand2);
                vector[i] = 1 / (rand2-rand1);
            }
        }

        public Vector (double[] arr)
        {
            vector = arr;
        }

        public double getIndex(int index)
        {
            if (index< vector.Length)
                return vector[index];
            return 0;
        }

        public void setIndex(int index, double value)
        {
            if (index < vector.Length)
                vector[index] = value;
        }

        public int size()
        {
            return vector.Length;
        }

        public static double operator *(Vector vector, Vector vector2)
        {
            int size = vector.size();
            if (size != vector2.size())
                return double.MaxValue;
            double res = 0;
            for (int i = 0; i < size; i++)
            {
                res += vector.getIndex(i) * vector2.getIndex(i);
            }
            return res;
        }

        public static Vector operator +(Vector vector, Vector vector2)
        {
            int size = vector.size();
            if (size != vector2.size())
                return null;
            double[] res = new double[size];
            for (int i = 0; i < size; i++)
            {
                res[i] = vector.getIndex(i) + vector2.getIndex(i);
            }
            return new Vector(res);
        }

        public static Vector operator *(Vector vector, double scalar)
        {
            for (int i = 0; i < vector.size(); i++)
            {
                vector.setIndex(i, vector.getIndex(i)*scalar);
            }
            return vector;
        }

        public static Vector operator /(Vector vector, double scalar)
        {
            for (int i = 0; i < vector.size(); i++)
            {
                vector.setIndex(i, vector.getIndex(i) / scalar);
            }
            return vector;
        }

        public static Vector operator -(Vector vector1, Vector vector2)
        {
            int size= vector1.size();
            if (size != vector2.size())
                return null;
            double[] res = new double[size];
            for (int i = 0; i < size; i++)
            {
                res[i] = vector1.getIndex(i) - vector2.getIndex(i);
            }
            return new Vector(res);
        }

    }
}
