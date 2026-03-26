using System.Globalization;

namespace SimplexMethodCLI
{
    public struct SimplexTableObject
    {
        public double UpperBound { get; set; }
        public double LowerBound { get; set; }
    }

    public static class DataLoader
    {
        public static async Task<List<string>> ReadFileAsync(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"File not exists by path: {Path.GetFullPath(path)}");

            List<string> lines = new List<string>();
            using (StreamReader streamReader = new StreamReader(path))
            {
                string? line;
                while ((line = await streamReader.ReadLineAsync()) != null)
                {
                    lines.Add(line);
                }
            }
            return lines;
        }

        public static double[][] GetConstraints(List<string> lines, char separator = ',')
        {
            if (lines.Count == 0)
                throw new FormatException("The file contains no data");

            double[][] constraints = new double[lines.Count - 2][];
            for (int i = 0; i < lines.Count - 2; i++)
            {
                string[] constraintStrings = lines[i].Split(separator, StringSplitOptions.RemoveEmptyEntries);
                double[] constraintVector = new double[constraintStrings.Length];

                for (int j = 0; j < constraintStrings.Length; j++)
                {
                    if (double.TryParse(constraintStrings[j], CultureInfo.InvariantCulture, out double value))
                        constraintVector[j] = value;
                    else
                        throw new FormatException($"Incorrect value in cell: {i}{j}");
                }
                constraints[i] = constraintVector;
            }
            return constraints;
        }

        public static double GetAdditionalVariable(List<string> lines)
        {
            if (lines.Count == 0)
                throw new FormatException("The file contains no data");

            Dictionary<string, double> constraintTypes = new Dictionary<string, double>()
            {
                {"<=", 1.0 }, { "=", 0.0 }, { ">=", -1.0 }
            };
            string constraintSign = lines[lines.Count - 1];
            return constraintTypes[constraintSign];
        }

        public static double[] GetObjectiveFunction(List<string> lines, char separator=',')
        {
            if (lines.Count == 0)
                throw new InvalidOperationException("The file contains no data");

            string[] objectiveFunction = lines[lines.Count - 2]
                .Split(separator, StringSplitOptions.RemoveEmptyEntries);

            double[] objective = new double[objectiveFunction.Length];
            for (int i = 0; i < objectiveFunction.Length; i++)
            {
                if (double.TryParse(objectiveFunction[i], CultureInfo.InvariantCulture, out double value))
                    objective[i] = value;
                else
                    throw new FormatException($"Incorrect value: {i}");
            }
            return objective;
        }
    }

    public class SimplexProcessor
    {
        private readonly SimplexTableObject[][] _simplexTable;
        private readonly int _tableRows;
        private readonly int _tableCols;
        private readonly int _objectiveRow;
        private readonly int[] _basisVarIndexes;
        private readonly int _originalVars;
        private readonly int _slackVars;

        public SimplexProcessor(double[][] constraints, double[] objective, double additionalVariable)
        {
            int constraintRows = constraints.Length;
            int constraintCols = constraints[0].Length - 1;
            
            _originalVars = constraints[0].Length - 1;
            _slackVars = constraints.Length;

            _simplexTable = new SimplexTableObject[constraintRows + 1][];
            for (int i = 0; i <= constraintRows; i++)
                _simplexTable[i] = new SimplexTableObject[constraintRows + constraintRows + 1];

            for (int i = 0; i < constraintRows; i++)
            {
                for (int j = 0; j < constraintRows; j++)
                    _simplexTable[i][j].UpperBound = constraints[i][j];
                    

                for (int j = 0; j < constraintRows; j++)
                    _simplexTable[i][constraintRows + j].UpperBound = (i == j) ? additionalVariable : 0.0;

                _simplexTable[i][constraintRows + constraintRows].UpperBound = constraints[i][constraintRows];
            }

            _objectiveRow = constraintRows;

            for (int j = 0; j < constraintRows; j++)
                _simplexTable[_objectiveRow][j].UpperBound = -objective[j];

            for (int j = 0; j < constraintRows; j++)
                _simplexTable[_objectiveRow][constraintRows + j].UpperBound = 0.0;
            
            _simplexTable[_objectiveRow][constraintRows + constraintRows].UpperBound = 0.0;

            _tableRows = _simplexTable.Length;
            _tableCols = _simplexTable[0].Length;
            
            _basisVarIndexes = new int[_tableRows - 1];
            for (int i = 0; i < _tableRows - 1; i++)
                _basisVarIndexes[i] = _tableCols - _tableRows + i; 
        }

        private int GetPivotColumn()
        {
            int col = -1;
            double minVal = 0.0;
            for (int j = 0; j < _tableCols - 1; j++)
            {
                if (_simplexTable[_objectiveRow][j].UpperBound < minVal)
                {
                    minVal = _simplexTable[_objectiveRow][j].UpperBound;
                    col = j;
                }
            }
            return col;
        }

        private int GetPivotRow(int pivotCol)
        {
            int row = -1;
            double minRatio = double.MaxValue;
            for (int i = 0; i < _tableRows - 1; i++)
            {
                if (_simplexTable[i][pivotCol].UpperBound > 0)
                {
                    double ratio = _simplexTable[i][_tableCols - 1].UpperBound / _simplexTable[i][pivotCol].UpperBound;
                    if (ratio < minRatio)
                    {
                        minRatio = ratio;
                        row = i;
                    }
                }
            }
            return row;
        }
        

        private void Pivot(int pivotRow, int pivotCol)
        {
            for (int i = 0; i < _tableRows; i++)
            {
                for (int j = 0; j < _tableCols; j++)
                {
                    _simplexTable[i][j].LowerBound = _simplexTable[i][j].UpperBound;
                }
            }
            
            double pivotVal = _simplexTable[pivotRow][pivotCol].UpperBound;

            for (int j = 0; j < _tableCols; j++)
                _simplexTable[pivotRow][j].UpperBound /= pivotVal;

            for (int i = 0; i < _tableRows; i++)
            {
                if (i == pivotRow) continue;
                double factor = _simplexTable[i][pivotCol].LowerBound;
                for (int j = 0; j < _tableCols; j++)
                {
                    _simplexTable[i][j].UpperBound -= factor * _simplexTable[pivotRow][j].UpperBound;
                }
            }

            _basisVarIndexes[pivotRow] = pivotCol;
        }

        private bool IsOptimal()
        {
            for (int j = 0; j < _tableCols - 1; j++)
                if (_simplexTable[_objectiveRow][j].UpperBound < 0)
                    return false;
            return true;
        }

        public void PrintTable(int pivotCol = -1, int pivotRow = -1)
        {
            Console.WriteLine();
            Console.Write("\"Basis\"");
            
            for (int x = 1; x < _originalVars; x++)
                Console.Write($"\"x{x}\";");
            for (int s = 1; s < _slackVars; s++)
                Console.Write($"\"s{s}\";");
            
            Console.Write($"\"Solution\";");
            Console.WriteLine();
            for (int i = 0; i < _tableRows; i++)
            {
                if (i < _tableRows - 1)
                {
                    int basisVarIndex = _basisVarIndexes[i];
                    if (basisVarIndex < _originalVars)
                        Console.Write($"\"x{basisVarIndex}\";");
                    else
                        Console.Write($"\"s{basisVarIndex - _originalVars + 1}\"");
                }
                else
                    Console.Write("\"Z\";");
                
                for (int j = 0; j < _tableCols; j++)
                {
                    double value = _simplexTable[i][j].UpperBound;
                    if (i == pivotRow && j == pivotCol)
                        Console.Write($"{value,7:F4}*;");
                    else
                        Console.Write($"{value,7:F4};");
                }
                Console.WriteLine();
            }
        }

        public void PrintCurrentSolution()
        {
            Console.WriteLine("\n--- Basis variables ---");
            for (int i = 0; i < _tableRows- 1; i++)
            {
                int varIndex = _basisVarIndexes[i];
                double value = _simplexTable[i][_tableCols - 1].UpperBound;
                if (varIndex < _originalVars)
                    Console.WriteLine($"x{varIndex + 1} = {value:F4}");
                else
                    Console.WriteLine($"s{varIndex - _originalVars + 1} = {value:F4}");
            }

            double objectiveValue = _simplexTable[_objectiveRow][_tableCols - 1].UpperBound;
            Console.WriteLine($"\nCurrent value F = {objectiveValue:F4}");
        }

        public void Solve()
        {
            int iteration = 0;
            Console.WriteLine("Start table:");
            PrintTable();
            PrintCurrentSolution();

            while (!IsOptimal())
            {
                int pivotCol = GetPivotColumn();
                if (pivotCol == -1) break;

                int pivotRow = GetPivotRow(pivotCol);
                if (pivotRow == -1)
                {
                    Console.WriteLine("The task is not limited");
                    return;
                }

                Pivot(pivotRow, pivotCol);
                iteration++;
                Console.WriteLine($"\nIteration {iteration}:");
                Console.WriteLine($"Pivot row: {pivotRow}, pivot column: {pivotCol}");
                PrintTable(pivotCol, pivotRow);
                PrintCurrentSolution();
            }
            Console.WriteLine("Current solution is optimal\nProblem solved");

        }
    }

    public static class Program
    {
        public static void PrintObjectiveFunction(double[] objective)
        {
            Console.Write("F: ");
            var objectiveString = string.Join(" + ", objective
                .Select((value, variable) => 
                    $"{value}x{variable + 1}")
                );
            Console.WriteLine(objectiveString);
        }
        public static void PrintConstraints(double[][] constraints)
        {
            Console.WriteLine("Constraints");
            foreach (var row in constraints)
            {
                var leftConstraints = row[..^1]
                    .Select((value, variable) => $"{value}x{variable + 1}");

                Console.WriteLine(string.Join(" + ", leftConstraints) + $" <= {row[^1]}");
            }
        }

        public static async Task Main(string[] args)
        {
            if (args.Length == 2)
            {
                string path = args[0];
                char separator = char.Parse(args[1]);
                List<string> lines = await DataLoader.ReadFileAsync(path);

                double[][] constraints = DataLoader.GetConstraints(lines, separator);
                double[] objective = DataLoader.GetObjectiveFunction(lines, separator);
                double additionalVariable = DataLoader.GetAdditionalVariable(lines);

                Console.WriteLine("-------------LINEAR PROGRAMMING SERVICE--------------");
                PrintObjectiveFunction(objective);
                PrintConstraints(constraints);
                var solver = new SimplexProcessor(constraints, objective, additionalVariable);
                solver.Solve();
            }
            else
            {
                Console.WriteLine("Command line argument is incorrect");
            }
        }
    }
}