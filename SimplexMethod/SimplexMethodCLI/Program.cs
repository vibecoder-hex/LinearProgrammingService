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
                for (int j = 0; j < variableCount; j++)
                {
                    Console.Write($"P{i + 1}: ");
                    int condition = Convert.ToInt32(Console.ReadLine());
                    vectors[i][j] = condition;
                }
            }
            return vectors;
        }

        public static int[] GetConstraintsVector(int constraintsCount)
        {
            int[] vectors = new int[constraintsCount];
            for (int i = 0; i < constraintsCount; i++)
            {
                Console.Write($"B: ");
                int constraint = Convert.ToInt32(Console.ReadLine());
                vectors[i] = constraint;
            }
            return vectors;
        }
        public static void Main(string[] args)
        {
            string? vectorCountString = Console.ReadLine();
            string? variableCountString = Console.ReadLine();
            string? constraintsCountString = Console.ReadLine();
            if (int.TryParse(variableCountString, out int variableCount) &&
                int.TryParse(constraintsCountString, out int constraintsCount) &&
                int.TryParse(vectorCountString, out int vectorCount))
            {
                int[][] conditionVectors = GetConditionVectors(vectorCount, variableCount);
                int[] constraintsVectors = GetConstraintsVector(constraintsCount);
                Console.WriteLine();
                foreach (int[] vector in conditionVectors)
                {
                    foreach (int element in vector)
                    {
                        Console.Write($"{element} ");
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
                constraintsVectors
                    .ToList()
                    .ForEach(element => Console.Write($"{element} "));
                Console.WriteLine();
            }
        }
    }
}

