namespace SimplexMethodCLI
{
    public class MatrixPreprocessor
    {
        private int[][] _conditionVectors;
        private List<int> _constraintsVectors;

        public MatrixPreprocessor(int[][] conditionVectors,  List<int> constraintsVectors)
        {
            _conditionVectors = conditionVectors;
            _constraintsVectors = constraintsVectors;
        }

        private int[][] TransposedMatrix(int[][] matrix)
        {
            int[][] transposedMatrix = new int[matrix.Length][];
            int rows = matrix.Length;
            int columns = matrix[0].Length;
            for (int i = 0; i < rows; i++)
            {
                transposedMatrix[i] = new int[rows];
                for (int j = 0; j < columns; j++)
                {
                    transposedMatrix[i][j] = matrix[j][i];
                }
            }
            return transposedMatrix;
        }
        public int[] GetFunctionVector() => TransposedMatrix(_conditionVectors)[0];
        
    }
    public static class Program
    {
        public static int[][] GetConditionVectors(int vectorCount, int variableCount)
        {
            int[][] vectors = new int[vectorCount][];
            for (int i = 0; i < vectorCount; i++)
            {
                Console.Write($"P{i + 1}: ");
                vectors[i] = Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(element =>
                        int.TryParse(element, out int condition) ? condition : 0)
                    .ToArray();
            }
            return vectors;
        }

        public static List<int> GetConstraintsVector()
        {
            Console.Write("B: ");
            List<int> constraintVector = Console.ReadLine()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(element => int.TryParse(element, out int constraint) ? constraint : 0)
                .ToList();
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
        
        

        public static void PrintConstraintsVector(List<int> constraintsVector) => 
            constraintsVector
                .ForEach(element => 
                    Console.Write($"{element} "));
        
        
        public static void Main(string[] args)
        {
            Console.Write("Vector count: ");
            string? vectorCountString = Console.ReadLine();
            Console.Write("Variable count: ");
            string? variableCountString = Console.ReadLine();
            if (int.TryParse(variableCountString, out int variableCount) && int.TryParse(vectorCountString, out int vectorCount))
            {
                int[][] conditionVectors = GetConditionVectors(vectorCount, variableCount);
                List<int> constraintsVectors = GetConstraintsVector();
                Console.WriteLine();
                PrintConditionMatrix(conditionVectors);
                Console.WriteLine();
                PrintConstraintsVector(constraintsVectors);
                Console.WriteLine();
                var preprocessor = new MatrixPreprocessor(conditionVectors, constraintsVectors);
                preprocessor.GetFunctionVector()
                    .ToList()
                    .ForEach(element => Console.Write($"{element} "));
            }
            else
            {
                Console.WriteLine("Invalid input");
            }
            
        }
    }
}

