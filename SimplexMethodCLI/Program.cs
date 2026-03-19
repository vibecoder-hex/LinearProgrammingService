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
                throw new FileNotFoundException($"Файла по пути: {Path.GetFullPath(path)} не существует");

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
                throw new FormatException("Файл не содержит данных.");

            double[][] constraints = new double[lines.Count - 2][];
            for (int i = 0; i < lines.Count - 2; i++)
            {
                string[] constraintStrings = lines[i].Split(separator, StringSplitOptions.RemoveEmptyEntries);
                double[] constraintVector = new double[constraintStrings.Length];

                for (int j = 0; j < constraintStrings.Length; j++)
                {
                    if (double.TryParse(constraintStrings[j], out double value))
                        constraintVector[j] = value;
                    else
                        throw new FormatException($"Неккоректное значение в ячейке: {i}{j}");
                }
                constraints[i] = constraintVector;
            }
            return constraints;
        }

        public static double GetAdditionalVariable(List<string> lines)
        {
            if (lines.Count == 0)
                throw new FormatException("Файл не содержит данных");

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
                throw new InvalidOperationException("Файл не содержит данных.");

            string[] objectiveFunction = lines[lines.Count - 2]
                .Split(separator, StringSplitOptions.RemoveEmptyEntries);

            double[] objective = new double[objectiveFunction.Length];
            for (int i = 0; i < objectiveFunction.Length; i++)
            {
                if (double.TryParse(objectiveFunction[i], out double value))
                    objective[i] = value;
                else
                    throw new FormatException($"Некорректное значение: {i}");
            }
            return objective;
        }
    }

    public class SimplexProcessor
    {
        private readonly SimplexTableObject[][] _simplexTable;
        private readonly int _rows;
        private readonly int _cols;
        private readonly int _objectiveRow;
        private readonly int[] _basis;

        public SimplexProcessor(double[][] constraints, double[] objective, double additionalVariable)
        {
            _rows = constraints.Length;
            _cols = constraints[0].Length - 1;

            _simplexTable = new SimplexTableObject[_rows + 1][];
            for (int i = 0; i <= _rows; i++)
                _simplexTable[i] = new SimplexTableObject[_cols + _rows + 1];

            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _cols; j++)
                    _simplexTable[i][j].UpperBound = constraints[i][j];
                    

                for (int j = 0; j < _rows; j++)
                    _simplexTable[i][_cols + j].UpperBound = (i == j) ? additionalVariable : 0.0;

                _simplexTable[i][_cols + _rows].UpperBound = constraints[i][_cols];
            }

            _objectiveRow = _rows;

            for (int j = 0; j < _cols; j++)
            {
                _simplexTable[_objectiveRow][j].UpperBound = -objective[j];
            }

            for (int j = 0; j < _rows; j++)
            {
                _simplexTable[_objectiveRow][_cols + j].UpperBound = 0.0;
            }
            _simplexTable[_objectiveRow][_cols + _rows].UpperBound = 0.0;

            _rows = _simplexTable.Length;
            _cols = _simplexTable[0].Length;

            _basis = new int[_rows - 1];
            for (int i = 0; i < _rows - 1; i++)
                _basis[i] = _cols - _rows + i; 
        }

        private int GetPivotColumn()
        {
            int col = -1;
            double minVal = 0.0;
            for (int j = 0; j < _cols - 1; j++)
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
            for (int i = 0; i < _rows - 1; i++)
            {
                if (_simplexTable[i][pivotCol].UpperBound > 0)
                {
                    double ratio = _simplexTable[i][_cols - 1].UpperBound / _simplexTable[i][pivotCol].UpperBound;
                    if (ratio < minRatio)
                    {
                        minRatio = ratio;
                        row = i;
                    }
                }
            }
            return row;
        }

        private void SaveLowerBounds()
        {
            for (int i = 0; i < _rows; i++)
                for (int j = 0; j < _cols; j++)
                    _simplexTable[i][j].LowerBound = _simplexTable[i][j].UpperBound;
        }

        private void Pivot(int pivotRow, int pivotCol)
        {
            SaveLowerBounds();

            double pivotVal = _simplexTable[pivotRow][pivotCol].UpperBound;

            for (int j = 0; j < _cols; j++)
                _simplexTable[pivotRow][j].UpperBound /= pivotVal;

            for (int i = 0; i < _rows; i++)
            {
                if (i == pivotRow) continue;
                double factor = _simplexTable[i][pivotCol].LowerBound;
                for (int j = 0; j < _cols; j++)
                {
                    _simplexTable[i][j].UpperBound -= factor * _simplexTable[pivotRow][j].UpperBound;
                }
            }

            _basis[pivotRow] = pivotCol;
        }

        private bool IsOptimal()
        {
            for (int j = 0; j < _cols - 1; j++)
                if (_simplexTable[_objectiveRow][j].UpperBound < 0)
                    return false;
            return true;
        }

        public void PrintTable(int pivotCol = -1, int pivotRow = -1)
        {
            Console.WriteLine();
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _cols; j++)
                {
                    if (i == pivotRow && j == pivotCol)
                        Console.Write($"{_simplexTable[i][j].UpperBound,7:F4}|{_simplexTable[i][j].LowerBound,7:F4}* ");
                    else
                        Console.Write($" {_simplexTable[i][j].UpperBound,7:F4}|{_simplexTable[i][j].LowerBound,7:F4} ");
                }
                Console.WriteLine();
            }
        }

        public void PrintCurrentSolution()
        {
            Console.WriteLine("\n--- Базисные переменные ---");
            for (int i = 0; i < _rows - 1; i++)
            {
                int varIndex = _basis[i];
                double value = _simplexTable[i][_cols - 1].UpperBound;
                Console.WriteLine($"x{varIndex + 1} = {value:F4}");
            }

            double objectiveValue = _simplexTable[_objectiveRow][_cols - 1].UpperBound;
            Console.WriteLine($"\nТекущее значение F = {objectiveValue:F4}");
        }

        public void Solve()
        {
            int iteration = 0;
            Console.WriteLine("Начальная таблица:");
            PrintTable();
            PrintCurrentSolution();

            while (!IsOptimal())
            {
                int pivotCol = GetPivotColumn();
                if (pivotCol == -1) break;

                int pivotRow = GetPivotRow(pivotCol);
                if (pivotRow == -1)
                {
                    Console.WriteLine("Задача не ограничена");
                    return;
                }

                Pivot(pivotRow, pivotCol);
                iteration++;
                Console.WriteLine($"\nИтерация {iteration}:");
                Console.WriteLine($"Ведущая строка: {pivotRow}, ведущий столбец: {pivotCol}");
                PrintTable(pivotCol, pivotRow);
                PrintCurrentSolution();
            }
            Console.WriteLine("Текущее решение оптимально\nЗадача решена");

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
            Console.WriteLine("Ограничения");
            foreach (var row in constraints)
            {
                var leftConstraints = row[..^1]
                    .Select((value, variable) => $"{value}x{variable + 1}");

                Console.WriteLine(string.Join(" + ", leftConstraints) + $" <= {row[^1]}");
            }
        }

        public static async Task Main(string[] args)
        {
            if (args.Length == 1)
            {
                string path = args[0];
                List<string> lines = await DataLoader.ReadFileAsync(path);

                double[][] constraints = DataLoader.GetConstraints(lines, ';');
                double[] objective = DataLoader.GetObjectiveFunction(lines, ';');
                double additionalVariable = DataLoader.GetAdditionalVariable(lines);

                Console.WriteLine("-------------ЗАДАЧА ЛИНЕЙНОГО ПРОГРАММИРОВНИЯ--------------");
                PrintObjectiveFunction(objective);
                PrintConstraints(constraints);
                var solver = new SimplexProcessor(constraints, objective, additionalVariable);
                solver.Solve();
            }
            else
            {
                Console.WriteLine("Неверный формат аргументов командной строки");
            }
        }
    }
}