namespace SimplexMethodCLI
{

    public enum ConstraintType
    {
        Less, Greater, Equal, LessOrEqual, GreaterOrEqual
    }

    public struct ContraintStructure
    {
        public int ConstraintNumber { get; set; }
        public ConstraintType ConstraintType { get; set; }
    }
    public class MatrixPreprocessor
    {
        private int[][] _conditionVectors;
        private int[] _constraintsVectors;

        public MatrixPreprocessor(int[][] conditionVectors,  int[] constraintsVectors)
        {
            _conditionVectors = conditionVectors;
            _constraintsVectors = constraintsVectors;
        }

        private int[][] TransposedMatrix(int[][] matrix)
        {
            if (matrix.Length == 0) return new int[][] { };
            
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

        public int[] GetFunctionVector()
        {
            if (_conditionVectors.Length == 0) return new int[] { };
            return _conditionVectors[0];
        }

        public int[][] GetConditionMatrix()
        {
            if (_conditionVectors.Length == 0) return new int[][] { };
            return TransposedMatrix(_conditionVectors)
                .Skip(1)
                .ToArray();
        }
        
        public int[] GetConstraintsVector()
        {
            if  (_constraintsVectors.Length == 0) return new int[] { };
            return _constraintsVectors;
        }

        public int[][] GetCanninicalMatrix()
        {
            return new int[][] { };
        }
        
    }

    public static class EquationDataPrinter
    {
        public static void PrintConditionMatrix(MatrixPreprocessor preprocessor)
        {
            int[][] matrix = preprocessor.GetConditionMatrix();
            if (matrix.Length == 0) return;
            
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
            int[] constraintsVector = preprocessor.GetConstraintsVector();
            if (constraintsVector.Length == 0) return;
            
            Console.Write("B: ");
            constraintsVector
                .ToList()
                .ForEach(element => 
                    Console.Write($"{element} "));
        }


        public static void PrintFunctionVector(MatrixPreprocessor preprocessor)
        {
            int[] functionVector = preprocessor.GetFunctionVector();
            if (functionVector.Length == 0) return;
            
            Console.Write("F: ");
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
        public static int[][] CreateConditionVectors(int vectorCount, int variableCount)
        {
            int[][] vectors = new int[vectorCount][];
            for (int i = 0; i < vectorCount; i++)
            {
                Console.Write($"P{i + 1}: ");
                string[] vectorComponents = Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (vectorComponents.Length != variableCount)
                {
                    Console.WriteLine("vector is incorrect");
                    return new int[][] {};
                }
                vectors[i] = vectorComponents  
                    .Select(element =>
                        int.TryParse(element, out int condition) ? condition : 0)
                    .ToArray();
            }
            return vectors;
        }

        public static int[] CreateConstraintsVector(int variableCount)
        {
            Console.Write("B: ");
            string[] constraintVector = Console.ReadLine()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (constraintVector.Length != variableCount)
            {
                Console.WriteLine("Constraint vector length is incorrect");
                return new int[] {};
            }
            return constraintVector
                .Select(element => int.TryParse(element, out int constraint) ? constraint : 0)
                .ToArray();;
        }
        
        
        public static void Main(string[] args)
        {
            Console.Write("Vector count: ");
            string? vectorCountString = Console.ReadLine();
            Console.Write("Variable count: ");
            string? variableCountString = Console.ReadLine();
            if (int.TryParse(vectorCountString, out int vectorCount) && int.TryParse(variableCountString, out int variableCount))
            {
                int[][] conditionVectors = CreateConditionVectors(vectorCount, variableCount);
                int[] constraintsVectors = CreateConstraintsVector(variableCount);
                var preprocessor = new MatrixPreprocessor(conditionVectors, constraintsVectors);
                Console.WriteLine();
                EquationDataPrinter.PrintFunctionVector(preprocessor);
                EquationDataPrinter.PrintConditionMatrix(preprocessor);
                EquationDataPrinter.PrintConstraintsVector(preprocessor);
            }
            else
            {
                Console.WriteLine("Invalid input");
            }
        }
    }
}