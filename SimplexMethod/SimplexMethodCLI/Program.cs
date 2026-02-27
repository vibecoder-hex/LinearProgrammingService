namespace SimplexMethodCLI
{
    public static class Program
    {
        public static int[][] GetConditionVectors(int vectorCount, int variableCount)
        {
            int[][] vectors = new int[vectorCount][];
            
            for (int i = 0; i < vectorCount; i++)
            {
                vectors[i] = new int[variableCount];
                Console.Write($"P{i + 1}: ");
                string[] conditionVector = Console.ReadLine()
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < conditionVector.Length; j++)
                {
                    if (int.TryParse(conditionVector[j], out int condition))
                    {
                        vectors[i][j] = condition;
                    }
                    else
                    {
                        Console.Error.Write($"{conditionVector[j]} is not a number");
                        return new int[][] { };
                    }
                }
            }
            return vectors;
        }

        public static int[] GetConstraintsVector(int constraintsCount)
        {
            Console.Write("B: ");
            int[] constraintVector = Console.ReadLine()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToArray();
            return constraintVector;
        }

        public static void PrintConditionMatrix(int[][] conditionVectors)
        {
            foreach (int[] vector in conditionVectors)
            {
                foreach (int element in vector)
                {
                    Console.Write($"{element} ");
                }
                Console.WriteLine();
            }
        }
        
        

        public static void PrintConstraintsVector(int[] constraintsVector)
        {
            constraintsVector
                .ToList()
                .ForEach(element => Console.Write($"{element} "));
        }
        
        public static void Main(string[] args)
        {
            Console.Write("Vector count: ");
            string? vectorCountString = Console.ReadLine();
            Console.Write("Variable count: ");
            string? variableCountString = Console.ReadLine();
            Console.Write("Constraints count: ");
            string? constraintsCountString = Console.ReadLine();
            if (int.TryParse(variableCountString, out int variableCount) &&
                int.TryParse(constraintsCountString, out int constraintsCount) &&
                int.TryParse(vectorCountString, out int vectorCount))
            {
                int[][] conditionVectors = GetConditionVectors(vectorCount, variableCount);
                int[] constraintsVectors = GetConstraintsVector(constraintsCount);
                Console.WriteLine();
                PrintConditionMatrix(conditionVectors);
                Console.WriteLine();
                PrintConstraintsVector(constraintsVectors);
                Console.WriteLine();
            }
            else
            {
                Console.Error.WriteLine("Invalid input");
            }
        }
    }
}

