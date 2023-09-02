using System;
using System.Collections.Generic;
using System.Text;

class Program
{
    static int k = 13; // Number of nodes
    static int[,] graph = new int[k, k]; // Graph connections

    static int numDays = 10; // Number of days
    static int numHours = 24; // Number of hours

    static int U = 10; // Slope constant
    static int E = 10; // Weather conditions constant

    static Random random = new Random();

    static void Main(string[] args)
    {
        int source = 0; // A
        int destination = 8; // I

        for (int day = 1; day <= numDays; day++)
        {

            char[] nodeNames = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I' };

            double[,] qij = GenerateObstaclesCount();
            double[,] pij = CalculateAbsenceObstaclesProbability(qij);
            (int[,] slope, int[,] weatherConditions) = GenerateSlopeAndWeatherConditions();
            double[,] Qij = CalculateRiskProbability(pij, slope, weatherConditions);
            double[,] Pij = CalculateAbsenceRiskProbability(Qij);

            Console.WriteLine();
            Console.WriteLine($"Day: {day}");


            (string, int, int)[] archPairs = new (string, int, int)[]
            {
                ("A to B", 0, 1),
                ("A to C", 0, 2),
                ("A to D", 0, 3),
                ("B to D", 1, 3),
                ("C to D", 2, 3),
                ("D to E", 3, 4),
                ("D to F", 3, 5),
                ("E to F", 4, 5),
                ("F to G", 5, 6),
                ("F to H", 5, 7),
                ("F to I", 5, 8),
                ("G to I", 6, 8),
                ("H to I", 7, 8)
            };

            foreach (var (pairName, src, dest) in archPairs)
            {
                PrintArchProbability(pairName, Pij[src, dest]);
            }

            InitializeGraph();

            BellmanFord(source, destination, Pij, out double leastRiskyPathProb, out List<int> fullPath);

            Console.WriteLine($"\nLeast Risky Path Probability from {nodeNames[source]} to {nodeNames[destination]}: {leastRiskyPathProb:0.000}");
            Console.Write("Final Path: ");
            for (int i = 0; i < fullPath.Count; i++)
            {
                Console.Write($"{nodeNames[fullPath[i]]}");
                if (i != fullPath.Count - 1)
                {
                    Console.Write(" -> ");
                }
            }
            Console.WriteLine();
        }
    }



    static void PrintArchProbability(string archName, double probability)
    {
        Console.WriteLine($"Probability for Absence of Risk for {archName}: {probability:0.000}");
    }

    static double[,] GenerateObstaclesCount()
    {
        double[,] EXij = new double[k, k];
        double[,] qij = new double[k, k];

        for (int i = 0; i < k - 1; i++)
        {
            for (int j = i + 1; j < k; j++)
            {
                for (int day = 1; day <= numDays; day++)
                {
                    for (int hour = 1; hour <= numHours; hour++)
                    {
                        double xst;
                        do
                        {
                            xst = random.Next(0, 121);
                        }
                        while (EXij[i, j] == xst && Math.Abs(EXij[i, j] - xst) < 30);

                        EXij[i, j] += xst;
                    }
                }

                EXij[i, j] *= (1.0 / (24 * numDays));

                double sum_EXij = 0;
                for (int x = 0; x < k - 1; x++)
                {
                    for (int y = x + 1; y < k; y++)
                    {
                        sum_EXij += EXij[x, y];
                    }
                }

                qij[i, j] = EXij[i, j] / sum_EXij;
            }
        }

        return qij;
    }

    static double[,] CalculateAbsenceObstaclesProbability(double[,] qij)
    {
        double[,] pij = new double[k, k];

        for (int i = 0; i < k - 1; i++)
        {
            for (int j = i + 1; j < k; j++)
            {
                pij[i, j] = 1 - qij[i, j];
            }
        }

        return pij;
    }

    static (int[,] slope, int[,] weatherConditions) GenerateSlopeAndWeatherConditions()
    {
        int[,] slope = new int[k, k];
        int[,] weatherConditions = new int[k, k];

        for (int i = 0; i < k - 1; i++)
        {
            for (int j = i + 1; j < k; j++)
            {
                slope[i, j] = random.Next(0, U + 1);
                weatherConditions[i, j] = random.Next(0, E + 1);
            }
        }

        return (slope, weatherConditions);
    }

    static double[,] CalculateRiskProbability(double[,] pij, int[,] slope, int[,] weatherConditions)
    {
        double[,] Qij = new double[k, k];

        for (int i = 0; i < k - 1; i++)
        {
            for (int j = i + 1; j < k; j++)
            {
                Qij[i, j] = pij[i, j] * (slope[i, j] * 1.0 / U) * (weatherConditions[i, j] * 1.0 / E);
            }
        }

        return Qij;
    }

    static double[,] CalculateAbsenceRiskProbability(double[,] Qij)
    {
        double[,] pij = new double[k, k];

        for (int i = 0; i < k - 1; i++)
        {
            for (int j = i + 1; j < k; j++)
            {
                pij[i, j] = 1 - Qij[i, j];
            }
        }

        return pij;
    }

    static void InitializeGraph()
    {
        graph[0, 1] = 1;
        graph[0, 2] = 1;
        graph[0, 3] = 1;
        graph[1, 3] = 1;
        graph[2, 3] = 1;
        graph[3, 4] = 1;
        graph[3, 5] = 1;
        graph[4, 5] = 1;
        graph[5, 6] = 1;
        graph[5, 7] = 1;
        graph[5, 8] = 1;
        graph[6, 8] = 1;
        graph[7, 8] = 1;
    }

    static void BellmanFord(int source, int destination, double[,] pij, out double leastRiskyPathProb, out List<int> fullPath)
    {
        int k = pij.GetLength(0);

        int[] parent = new int[k];
        double[] lowestRisk = new double[k];

        for (int i = 0; i < k; i++)
        {
            lowestRisk[i] = double.NegativeInfinity;
        }

        lowestRisk[source] = 1;

        for (int i = 0; i < k - 1; i++)
        {
            for (int u = 0; u < k; u++)
            {
                for (int v = u + 1; v < k; v++)
                {
                    if (graph[u, v] == 1 && lowestRisk[u] * pij[u, v] > lowestRisk[v])
                    {
                        lowestRisk[v] = lowestRisk[u] * pij[u, v];
                        parent[v] = u;
                    }
                }
            }
        }


        for (int u = 0; u < k; u++)
        {
            for (int v = u + 1; v < k; v++)
            {
                if (graph[u, v] == 1 && lowestRisk[u] * pij[u, v] > lowestRisk[v])
                {
                    leastRiskyPathProb = double.NaN;
                    fullPath = null;
                    return;
                }
            }
        }

        fullPath = new List<int>();
        int current = destination;
        while (current != source)
        {
            fullPath.Insert(0, current);
            current = parent[current];
        }
        fullPath.Insert(0, source);

        leastRiskyPathProb = 1;
        for (int i = 0; i < fullPath.Count - 1; i++)
        {
            int u = fullPath[i];
            int v = fullPath[i + 1];
            leastRiskyPathProb *= pij[u, v];
        }
    }
}


