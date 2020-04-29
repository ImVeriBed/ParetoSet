using System;
using System.Collections.Generic;

namespace ParetoSet
{
    public class Program
    {
        private static Regims? Flag;
        private static List<int> Weights;
        private static void Main()
        {
            //Выбор режима работы
            SetFlag();
            if (Flag is null) goto end;

            //Ввод количества критериев
            var critCnt = FillCritCnt();

            //Ввод весов критериев
            if (Flag == Regims.Сужение_множества_Парето || Flag == Regims.Целевое_программирование)
            {
                FillWeights(critCnt);
            }

            //Ввод количества векторов
            var vectorCnt = FillCaseCnt();
            //Ввод значений векторов
            var matrix = FillMatrix(critCnt, vectorCnt);          
          
            if (Flag == Regims.Сужение_множества_Парето || Flag == Regims.Алгоритм_Парето)
            {
                Console.WriteLine("Результат: ");
                var paretoSet = GetParetoSet(matrix);
                if (Flag == Regims.Сужение_множества_Парето) Console.WriteLine("Элементы векторов домножены на веса");
                for (int i = 0; i < paretoSet.Count; i++)
                {
                    Console.Write($"Вектор {i + 1}: (");
                    Console.Write(string.Join(", ", paretoSet[i]));
                    Console.WriteLine(")");
                }
            }
            else if (Flag == Regims.Целевое_программирование)
            {
                TargetProgramming(matrix, critCnt);
            }

        end:
            Console.WriteLine("Завершение работы программы, нажмите любую клавишу...");
            Console.ReadLine();
        }

        private static int FillCritCnt()
        {
        critCnt:
            Console.WriteLine("Введите количество критериев");
            if (int.TryParse(Console.ReadLine(), out int result))
            {
                return result;
            }
            else
            {
                Console.WriteLine("Некорректный ввод");
                goto critCnt;
            }
        }

        private static int FillCaseCnt()
        {
        caseCnt:
            Console.WriteLine("Введите количество векторов");
            if (int.TryParse(Console.ReadLine(), out int result))
            {
                return result;
            }
            else
            {
                Console.WriteLine("Некорректный ввод");
                goto caseCnt;
            }
        }

        private static List<List<int>> FillMatrix(int critCnt, int vectorCnt)
        {
            var resultSet = new List<List<int>>();
            for (int i = 0; i < vectorCnt; i++)
            {
                var currentVector = new List<int>();
                Console.WriteLine($"Заполните вектор {i + 1} (через Enter)");
                for (int j = 0; j < critCnt; j++)
                {
                error:
                    Console.WriteLine($"Заполните критерий {j + 1}");
                    if (int.TryParse(Console.ReadLine(), out int result))
                    {
                        if (Flag == Regims.Сужение_множества_Парето || Flag == Regims.Целевое_программирование)
                        {
                            result *= Weights[j];
                        }

                        currentVector.Add(result);
                    }
                    else
                    {
                        Console.WriteLine("Надо ввести число");
                        goto error;
                    }
                }
                resultSet.Add(currentVector);
            }
            return resultSet;
        }

        private static List<List<int>> GetParetoSet(List<List<int>> matrix)
        {
            var paretoSet = new List<List<int>>();
            matrix.ForEach(v => paretoSet.Add(v));
            int i = 0;
            int j = 1;

        shag2:
            if (j == matrix.Count) return paretoSet;
            if (GetSumOfVector(matrix[i]) > GetSumOfVector(matrix[j]))
            {
                paretoSet.Remove(matrix[j]);
                if (j < matrix.Count)
                {
                    j++;
                    goto shag2;
                }
                else
                {
                    goto shag7;
                }
            }
            else
            {
                if (j == matrix.Count) return paretoSet;
                if (GetSumOfVector(matrix[j]) > GetSumOfVector(matrix[i]))
                {
                    paretoSet.Remove(matrix[i]);
                    goto shag7;
                }
                else
                {
                    if (j < matrix.Count)
                    {
                        j++;
                        goto shag2;
                    }
                    else
                    {
                        goto shag7;
                    }
                }
            }

        shag7:
            if (i < matrix.Count - 1)
            {
                i++;
                j = i + 1;
                goto shag2;
            }
            else
            {
                return paretoSet;
            }

        }

        private static int GetSumOfVector(List<int> vector)
        {
            int sum = 0;
            foreach (var element in vector)
                sum += element;
            return sum;
        }

        private static void SetFlag()
        {
            Console.WriteLine("Выберете режим работы:");
            Console.WriteLine("1. Алгоритм Парето");
            Console.WriteLine("2. Сужение множества Парето");
            Console.WriteLine("3. Целевое программирование");
            Console.WriteLine("Для выбора введите 1, 2 или 3 соответственно, ввод других символов приведет к завершению работы");
            string input = Console.ReadLine().Trim();
            if (!int.TryParse(input, out int reg))
            {
                Flag = null;
                return;
            }
            switch (reg)
            {
                case 1:
                    Flag = Regims.Алгоритм_Парето;
                    break;
                case 2:
                    Flag = Regims.Сужение_множества_Парето;
                    break;
                case 3:
                    Flag = Regims.Целевое_программирование;
                    break;
                default:
                    Flag = null;
                    break;
            }
        }

        private static void FillWeights(int critCnt)
        {
            Weights = new List<int>();
            for (int i = 0; i < critCnt; i++)
            {
                Console.WriteLine($"Введите вес критерия номер {i + 1}") ;
                if (int.TryParse(Console.ReadLine(), out int input))
                    Weights.Add(input);
                else
                {
                    Console.WriteLine("Некорректный ввод");
                    i -= 1;
                    continue;
                }
            }
        }

        private static void TargetProgramming(List<List<int>> matrix, int critCnt)
        {
            var zetSet = new List<double>();
            double result = 0;

            for (int i = 0; i < critCnt; i++)
            {
                Console.WriteLine($"Введите z[{i + 1}]");
                if (double.TryParse(Console.ReadLine(), out double input))
                    zetSet.Add(input);
                else
                {
                    Console.WriteLine("Некорректный ввод");
                    i -= 1;
                    continue;
                }
            }

            int number = 0;
            for (int j = 0; j < matrix.Count; j++)
            {
                double localResult = 0;               
                for (int i = 0; i < matrix[j].Count; i++)
                {
                    localResult += Math.Pow(matrix[j][i] - zetSet[i], 2);
                }

                if (localResult > result)
                {
                    result = localResult;
                    number = j;
                }
            }

            Console.WriteLine($"Победил вектор {number}");
            Console.WriteLine(string.Join(", ", matrix[number]));
            Console.WriteLine($"Результат: sqrt({result}) = {Math.Sqrt(result)}");
        }

        public enum Regims
        {
            Алгоритм_Парето,
            Сужение_множества_Парето,
            Целевое_программирование
        }


    }
}
