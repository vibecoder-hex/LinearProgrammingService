namespace SimplexMethodCLI
{

    public enum ConstraintType
    { 
        Equal, LessOrEqual, GreaterOrEqual
    }

    public struct ConstraintObject
    {
        public double ConstraintNumber { get; set; }
        public ConstraintType ConstraintType { get; set; }
    }

    public struct SimplexTableObject
    {
        public double UpperBound { get; set; }
        public double LowerBound { get; set; }
    }
    
    
    public class MatrixPreprocessor
    {
        private readonly double[][] _conditionVectors;
        private readonly ConstraintObject[] _constraintsObjects;
        private readonly int _normalizationScale;

        public MatrixPreprocessor(double[][] conditionVectors,  ConstraintObject[] constraintsObjects, int normalizationScale)
        {
            _conditionVectors = conditionVectors;
            _constraintsObjects = constraintsObjects;
            _normalizationScale = normalizationScale;
            NormalizedConditions(_conditionVectors);
            NormalizedConstraints(_constraintsObjects);
        }

        private void NormalizedConditions(double[][] conditionVectors)
        {
            if (conditionVectors.Length == 0 || conditionVectors[0].Length == 0)
                return;

            int rows = conditionVectors.Length;
            int columns = conditionVectors[0].Length;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    conditionVectors[i][j] /= _normalizationScale;
                }
            }
        }

        private void NormalizedConstraints(ConstraintObject[] constraintsObjects)
        {
            if (constraintsObjects.Length == 0) return;
            
            for (int i = 0; i < constraintsObjects.Length; i++)
            {
                constraintsObjects[i].ConstraintNumber /= _normalizationScale;
            }
        }

        private double[][] TransposedMatrix(double[][] matrix)
        {
            if (matrix.Length == 0 || matrix[0].Length == 0) return new double[][] { };
            int rows = matrix.Length;
            int columns = matrix[0].Length;
            
            double[][] transposedMatrix = new double[columns][];
            
            for (int i = 0; i < columns; i++)
            {
                transposedMatrix[i] = new double[rows];
                for (int j = 0; j < rows; j++)
                {
                    transposedMatrix[i][j] = matrix[j][i];
                }
            }
            return transposedMatrix;
        }

        public double[] GetFunctionVector()
        {
            if (_conditionVectors.Length == 0) return new double[] { };
            return TransposedMatrix(_conditionVectors)[0];
        }

        public double[][] GetConditionMatrix()
        {
            if (_conditionVectors.Length == 0) return new double[][] { };
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
        
        public SimplexTableObject[][] GetFirstSimplexTable(bool isMaximize = true)
        {
            double[][] matrix = GetConditionMatrix();
            double[] functionVector = GetFunctionVector();
            if (matrix.Length == 0 || functionVector.Length == 0 || _constraintsObjects.Length == 0) 
                return new SimplexTableObject[][] { };
            
            int rows = matrix.Length;
            int columns = matrix[0].Length;
            
            SimplexTableObject[][] simplexMatrix = new SimplexTableObject[rows + 1][];
                
            for (int i = 0; i < rows; i++)
            {
                simplexMatrix[i] = new SimplexTableObject[columns + 1];
                for (int j = 0; j < columns; j++)
                {
                    simplexMatrix[i][j].UpperBound = matrix[i][j];
                }
                simplexMatrix[i][columns].UpperBound = _constraintsObjects[i].ConstraintNumber;
            }
            simplexMatrix[rows] = new SimplexTableObject[columns + 1];
            for (int i = 0; i < columns; i++)
            {
                if (isMaximize)
                    simplexMatrix[rows][i].UpperBound = -functionVector[i];
                else
                    simplexMatrix[rows][i].UpperBound = functionVector[i];
            }
            return simplexMatrix;
        }
    }

    public static class EquationDataPrinter
    {
        public static void PrintConditionMatrix(MatrixPreprocessor preprocessor, int normalizationScale)
        {
            double[][] matrix = preprocessor.GetConditionMatrix();
            ConstraintObject[] constraintsObjects = preprocessor.GetConstraintsObject();
            if (matrix.Length == 0 || constraintsObjects.Length == 0) return;
            
            Dictionary<ConstraintType, string> constraintTypes = new Dictionary<ConstraintType, string>
            {
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
                    vectorComponents[j] = $"{matrix[i][j] * normalizationScale}X{j + 1}";
                }
                ConstraintType constraintType = constraintsObjects[i].ConstraintType;
                double constraintNumber = constraintsObjects[i].ConstraintNumber;
                Console.WriteLine(string.Join(" + ", vectorComponents) + $" {constraintTypes[constraintType]} {constraintNumber * normalizationScale}");
            }
        }
        

        public static void PrintFunctionVector(MatrixPreprocessor preprocessor, int normalizationScale)
        {
            double[] functionVector = preprocessor.GetFunctionVector();
            if (functionVector.Length == 0) return;
            
            Console.Write("F: ");
            string[] functionComponents = new string[functionVector.Length];
            for (int i = 0; i < functionVector.Length; i++)
            {
                functionComponents[i] = $"{functionVector[i] * normalizationScale}X{i + 1}";
            }
            Console.WriteLine(string.Join(" + ", functionComponents));
        }
    }

    public class SimplexProcessor
    {
        private readonly SimplexTableObject[][] _simplexTable;
        private readonly int _normalizationScale;

        public SimplexProcessor(SimplexTableObject[][] simplexTable, int normalizationScale)
        {
            _simplexTable = simplexTable;
            _normalizationScale = normalizationScale;
        }

        private int GetPivotColumnIndex(int rows, int columns)
        {
            int pivotColumn = -1;
            double minValue = double.MaxValue;

            for (int i = 0; i < columns; i++)
            {
                if (_simplexTable[rows - 1][i].UpperBound < minValue)
                {
                    minValue = _simplexTable[rows - 1][i].UpperBound;
                    pivotColumn = i;
                }
            }
            return pivotColumn;
        }

        private int GetPivotRowIndex(int pivotColumn, int rows, int columns)
        {
            int pivotRow = -1;
            double minValue = double.MaxValue;
            
            for (int i = 0; i < rows; i++)
            {
                if (_simplexTable[i][pivotColumn].UpperBound > 0)
                {
                    double ratio = _simplexTable[i][columns - 1].UpperBound / _simplexTable[i][pivotColumn].UpperBound;
                    if (ratio < minValue)
                    {
                        minValue = ratio;
                        pivotRow = i;
                    }
                }
            }
            return pivotRow;
        }

        private void FillPivotLowerBounds(int rows, int columns)
        {
            int pivotColumnIndex = GetPivotColumnIndex(rows, columns);
            int pivotRowIndex = GetPivotRowIndex(pivotColumnIndex, rows, columns);

            double midElement = _simplexTable[pivotRowIndex][pivotColumnIndex].UpperBound;

            for (int i = 0; i < rows; i++)
            {
                if (i == pivotRowIndex) continue;
                
                double ratio = _simplexTable[i][pivotColumnIndex].UpperBound / midElement; 
                _simplexTable[i][pivotColumnIndex].LowerBound = -ratio;
            }

            for (int i = 0; i < columns; i++)
            {
                if (i == pivotColumnIndex) continue;
                
                double ratio = _simplexTable[pivotRowIndex][i].UpperBound / midElement; 
                _simplexTable[pivotRowIndex][i].LowerBound = ratio;
            }
            _simplexTable[pivotRowIndex][pivotColumnIndex].LowerBound = 1 / midElement;
        }

        private void FillTable(int rows, int columns, int pivotColumnIndex, int pivotRowIndex)
        {
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    if (i != pivotColumnIndex && j != pivotRowIndex)
                    {
                        double pivotColumnBound = _simplexTable[i][pivotColumnIndex].LowerBound;
                        double pivotRowBound = _simplexTable[i][pivotRowIndex].UpperBound;
                        _simplexTable[i][j].LowerBound = pivotColumnBound * pivotRowBound;
                    }
                }
            }
        }

        private bool IsOptimal(int columns, int rows)
        {
            for (int i = 0; i < columns; i++)
            {
                if (_simplexTable[rows - 1][i].UpperBound < 0)
                {
                    return false;
                }
            }
            return true;
        }

        private void PivotReplacement(int columns, int pivotColumnIndex, int pivotRowIndex)
        {
            for (int i = 0; i < columns; i++)
            {
                (_simplexTable[pivotColumnIndex][i], _simplexTable[pivotRowIndex][i]) = (_simplexTable[pivotRowIndex][i], _simplexTable[pivotColumnIndex][i]);
            }
        }

        private void RecountTable(int rows, int columns)
        {
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    _simplexTable[i][j].UpperBound = _simplexTable[i][j].LowerBound;
                }
            }
        }
        
        public void PrintSimplexTable(int rows, int columns)
        {
            Console.WriteLine();
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    Console.Write($"{_simplexTable[i][j].UpperBound, 2:F3}|{_simplexTable[i][j].LowerBound:F3} ");
                }
                Console.WriteLine();
            }
        }

        private double GetTargetFunctionValue(int rows, int columns) => _simplexTable[rows - 1][columns - 1].UpperBound;

        public void TransformationLoop()
        {
            if (_simplexTable.Length == 0) return;
            
            int rows = _simplexTable.Length;
            int columns = _simplexTable[0].Length;

            PrintSimplexTable(rows, columns);

            while (!IsOptimal(columns, rows))
            {
                int pivotColumnIndex = GetPivotColumnIndex(rows, columns);
                int pivotRowIndex = GetPivotRowIndex(pivotColumnIndex, rows, columns);
                if (pivotRowIndex == -1)
                {
                    Console.WriteLine("No optimal solution found.");
                    return;
                }
                
                FillPivotLowerBounds(rows, columns);
                FillTable(rows, columns, pivotColumnIndex, pivotRowIndex);
                PivotReplacement(columns, pivotColumnIndex, pivotRowIndex);
                RecountTable(rows, columns);
                PrintSimplexTable(rows, columns);
            }
            double optimalValue = GetTargetFunctionValue(rows, columns) * _normalizationScale;
            Console.WriteLine($"\nOptimal F value: {optimalValue}");
        }
    }

    
    public static class Program
    {
        public static double[][] CreateConditionVectors(int vectorCount, int variableCount)
        {
            double[][] vectors = new double[vectorCount][];
            for (int i = 0; i < vectorCount; i++)
            {
                Console.Write($"P{i + 1}: ");
                string[] vectorComponents = Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (vectorComponents.Length != variableCount)
                {
                    Console.WriteLine("vector is incorrect");
                    return new double[][] {};
                }
                vectors[i] = vectorComponents
                    .Select(element =>
                        double.TryParse(element, out double condition) ? condition : 0)
                    .ToArray();
            }
            return vectors;
        }

        public static ConstraintObject[] CreateConstraintsObjects(int variableCount)
        {
            Console.Write("B: ");
            string[] constraintVector = Console.ReadLine()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            if (constraintVector.Length != variableCount - 1)
            {
                Console.WriteLine("Constraint vector length is incorrect");
                return new ConstraintObject[] {};
            }
            ConstraintObject[] constraintsObjects = new ConstraintObject[constraintVector.Length];

            for (int i = 0; i < constraintVector.Length; i++)
            {
                if (double.TryParse(constraintVector[i], out double constraint))
                {
                    Console.WriteLine($"Select constraint type for {constraintVector[i]}: \n 1.Equal, 2.LessOrEqual, 3.GreaterOrEqual");
                    ConstraintObject constraintObject = new ConstraintObject();
                    string? selectedType = Console.ReadLine();
                    if (int.TryParse(selectedType, out int type))
                    {
                        switch (type)
                        {
                            case 1:
                                constraintObject.ConstraintType = ConstraintType.Equal;
                                break;
                            case 2:
                                constraintObject.ConstraintType = ConstraintType.LessOrEqual;
                                break;
                            case 3:
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
                    return new ConstraintObject[] {};
                }
            }
            return constraintsObjects;
        }
        
        
        public static void Main(string[] args)
        {
            if (args.Length != 3) return;
            if (int.TryParse(args[0], out int vectorCount) && int.TryParse(args[1], out int variableCount) && int.TryParse(args[2], out int normalizationScale))
            {
                if (vectorCount == variableCount)
                {
                    double[][] conditionVectors = CreateConditionVectors(vectorCount, variableCount);
                    ConstraintObject[] constraintsObjects = CreateConstraintsObjects(variableCount);
                    var preprocessor = new MatrixPreprocessor(conditionVectors, constraintsObjects, normalizationScale);
                    var simplexProcessor = new SimplexProcessor(preprocessor.GetFirstSimplexTable(), normalizationScale);

                    Console.WriteLine();
                    Console.WriteLine("Function vector:");
                    EquationDataPrinter.PrintFunctionVector(preprocessor, normalizationScale);
                    Console.WriteLine("Condition matrix:");
                    EquationDataPrinter.PrintConditionMatrix(preprocessor, normalizationScale);
                    Console.WriteLine();

                    simplexProcessor.TransformationLoop();
                }
                else 
                {
                    Console.Error.WriteLine("Vector count should be equal to variable count");
                }

            }
            else
            {
                Console.Error.WriteLine("Invalid input");
            }

            Console.Error.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}

/*
5 2 1 2
6 1 5 2
8 3 2 4
4 1 4 1
20 80 50
2
2
2
*/

/*
2|0,000  2|0,000  4|-1,333  1|0,000 50|0,000 
1|0,000  5|-0,667  2|-0,667  4|-0,667 80|-0,667 
2|0,667  1|0,667  3|0,667  1|1,333 20|1,333 
-5|0,000 -6|-13,333 -8|-13,333 -4|66,667  0|66,667 
*/   