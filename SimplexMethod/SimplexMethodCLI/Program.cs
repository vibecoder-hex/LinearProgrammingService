namespace SimplexMethodCLI
{

    public enum ConstraintType
    {
        Less, Greater, Equal, LessOrEqual, GreaterOrEqual
    }

    public struct ConstraintObject
    {
        public int ConstraintNumber { get; set; }
        public ConstraintType ConstraintType { get; set; }
    }
    public class MatrixPreprocessor
    {
        private int[][] _conditionVectors;
        private ConstraintObject[] _constraintsObjects;

        public MatrixPreprocessor(int[][] conditionVectors,  ConstraintObject[] constraintsObjects)
        {
            _conditionVectors = conditionVectors;
            _constraintsObjects = constraintsObjects;
        }

        private int[][] TransposedMatrix(int[][] matrix)
        {
            if (matrix.Length == 0 || matrix[0].Length == 0) return new int[][] { };
            int rows = matrix.Length;
            int columns = matrix[0].Length;
            
            int[][] transposedMatrix = new int[columns][];
            
            for (int i = 0; i < columns; i++)
            {
                transposedMatrix[i] = new int[rows];
                for (int j = 0; j < rows; j++)
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
        
        public ConstraintObject[] GetConstraintsObject()
        {
            if  (_constraintsObjects.Length == 0) 
                return new ConstraintObject[] { };
            return _constraintsObjects;
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
            ConstraintObject[] constraintsObjects = preprocessor.GetConstraintsObject();
            if (matrix.Length == 0 || constraintsObjects.Length == 0) return;
            
            Dictionary<ConstraintType, string> constraintTypes = new Dictionary<ConstraintType, string>
            {
                { ConstraintType.Less, "<" },
                { ConstraintType.Greater, ">" },
                { ConstraintType.Equal, "=" },
                { ConstraintType.LessOrEqual, "<=" },
                { ConstraintType.GreaterOrEqual, ">=" }
            };
            
            int rows = matrix.Length;
            int columns = matrix[0].Length;
            for (int i = 0; i < rows; i++)
            {
                string[] vectorComponents = new string[columns];
                for (int j = 0; j < columns; j++)
                {
                    vectorComponents[j] = $"{matrix[i][j]}X{j + 1}";
                }
                ConstraintType constraintType = constraintsObjects[i].ConstraintType;
                int constraintNumber = constraintsObjects[i].ConstraintNumber;
                Console.WriteLine(string.Join(" + ", vectorComponents) + $" {constraintTypes[constraintType]} {constraintNumber}");
            }
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

        public static ConstraintObject[] CreateConstraintsObjects(int variableCount)
        {
            Console.Write("B: ");
            string[] constraintVector = Console.ReadLine()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            if (constraintVector.Length != variableCount)
            {
                Console.WriteLine("Constraint vector length is incorrect");
                return new ConstraintObject[] {};
            }
            ConstraintObject[] constraintsObjects = new ConstraintObject[constraintVector.Length];

            for (int i = 0; i < constraintVector.Length; i++)
            {
                if (int.TryParse(constraintVector[i], out int constraint))
                {
                    Console.WriteLine($"Select constraint type for {constraintVector[i]}: \n 1.Less, 2.Greater, 3.Equal, 4.LessOrEqual, 5.GreaterOrEqual");
                    ConstraintObject constraintObject = new ConstraintObject();
                    string? selectedType = Console.ReadLine();
                    if (int.TryParse(selectedType, out int type))
                    {
                        switch (type)
                        {
                            case 1:
                                constraintObject.ConstraintType = ConstraintType.Less;
                                break;
                            case 2:
                                constraintObject.ConstraintType = ConstraintType.Greater;
                                break;
                            case 3:
                                constraintObject.ConstraintType = ConstraintType.Equal;
                                break;
                            case 4:
                                constraintObject.ConstraintType = ConstraintType.LessOrEqual;
                                break;
                            case 5:
                                constraintObject.ConstraintType = ConstraintType.GreaterOrEqual;
                                break;
                        }
                    }
                    else
                    {
                        Console.Error.WriteLine("Invalid constraint type");
                        return new ConstraintObject[] {};
                    }
                    constraintObject.ConstraintNumber = constraint;
                    constraintsObjects[i]  = constraintObject;
                }
                else
                {
                    Console.Error.WriteLine("Invalid constraint input");
                }
            }
            return constraintsObjects;
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
                ConstraintObject[] constraintsObjects = CreateConstraintsObjects(variableCount);
                var preprocessor = new MatrixPreprocessor(conditionVectors, constraintsObjects);
                Console.WriteLine();
                EquationDataPrinter.PrintFunctionVector(preprocessor);
                EquationDataPrinter.PrintConditionMatrix(preprocessor);
            }
            else
            {
                Console.WriteLine("Invalid input");
            }
        }
    }
}