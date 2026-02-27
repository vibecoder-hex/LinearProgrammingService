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
                transposedMatrix[i] = new int[columns];
                for (int j = 0; j < columns; j++)
                {
                    transposedMatrix[i][j] = matrix[j][i];
                }
            }
            return transposedMatrix;
        }
        public int[] GetFunctionVector() => TransposedMatrix(_conditionVectors)[0];
        public int[][] GetConditionMatrix() => TransposedMatrix(_conditionVectors)
            .Skip(1)
            .ToArray();
        
        public List<int> GetConstraintsVector() => _constraintsVectors;
        
    }

    public static class DataPrinter
    {
        public static void PrintConditionMatrix(MatrixPreprocessor preprocessor)
        {
            int[][] matrix = preprocessor.GetConditionMatrix();
            int rows = matrix.Length;
            int columns = matrix[0].Length;
            for (int i = 0; i < rows; i++)
            {
                string[] vectorComponents = new string[columns];
                for (int j = 0; j < columns; j++)
                {
                    vectorComponents[j] = $"{matrix[i][j]}X{j + 1}";
                }
                Console.WriteLine(string.Join(" + ", vectorComponents));
            }
        }


        public static void PrintConstraintsVector(MatrixPreprocessor preprocessor)
        {
            Console.Write("B: ");
            preprocessor.GetConstraintsVector()
                .ForEach(element => 
                    Console.Write($"{element} "));
        }


        public static void PrintFunctionVector(MatrixPreprocessor preprocessor)
        {
            Console.Write("F: ");
            int[] functionVector = preprocessor.GetFunctionVector();
            string[] functionComponents = new string[functionVector.Length];
            for (int i = 0; i < functionVector.Length; i++)
            {
                functionComponents[i] = $"{functionVector[i]}X{i + 1}";
            }
            Console.WriteLine(string.Join(" + ", functionComponents));
        }
    }
    
    public static class Program
    {
        public static int[][] GetConditionVectors(int vectorCount)
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
        
        
        public static void Main(string[] args)
        {
            Console.Write("Vector count: ");
            string? vectorCountString = Console.ReadLine();
            if (int.TryParse(vectorCountString, out int vectorCount))
            {
                int[][] conditionVectors = GetConditionVectors(vectorCount);
                List<int> constraintsVectors = GetConstraintsVector();
                var preprocessor = new MatrixPreprocessor(conditionVectors, constraintsVectors);
                Console.WriteLine();
                DataPrinter.PrintFunctionVector(preprocessor);
                DataPrinter.PrintConditionMatrix(preprocessor);
                DataPrinter.PrintConstraintsVector(preprocessor);
            }
            else
            {
                Console.WriteLine("Invalid input");
            }
            
        }
    }
}

