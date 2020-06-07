using System;
using System.Collections.Generic;
using Mapack;

namespace ParetoSet
{
    public class Program
    {
        private static Regims? Flag;
        private static List<double> Weights;
        private static void Main()
        {
            //Выбор режима работы
            SetFlag();
            if (Flag is null) goto end;

            //Ввод количества критериев
            var critCnt = FillCritCnt();

            //Ввод весов критериев
            if (Flag == Regims.Сужение_множества_Парето || Flag == Regims.Целевое_программирование || Flag == Regims.Подход_MAUT)
            {
                FillWeights(critCnt);
            }

            //Ввод количества векторов
            int vectorCnt = critCnt;
            if (Flag != Regims.Метод_анализа_иерархий)
            {
                vectorCnt = FillCaseCnt();
            }

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
                ЦелевоеПрограммирование(matrix, critCnt);
            }
            else if (Flag == Regims.Метод_анализа_иерархий)
            {
                МетодАнализаИерархий(matrix);
            }
            else if (Flag == Regims.Подход_MAUT)
            {
                MAUT(matrix);
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

        private static List<List<double>> FillMatrix(int critCnt, int vectorCnt)
        {
            var resultSet = new List<List<double>>();
            for (int i = 0; i < vectorCnt; i++)
            {
                var currentVector = new List<double>();
                Console.WriteLine($"Заполните вектор {i + 1} (через Enter)");
                for (int j = 0; j < critCnt; j++)
                {
                error:
                    Console.WriteLine($"Заполните критерий {j + 1}");
                    if (double.TryParse(Console.ReadLine(), out double result))
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

        private static List<List<double>> GetParetoSet(List<List<double>> matrix)
        {
            var paretoSet = new List<List<double>>();
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

        private static double GetSumOfVector(List<double> vector)
        {
            double sum = 0;
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
            Console.WriteLine("4. Метод анализа иерархий");
            Console.WriteLine("5. Подход MAUT");
            Console.WriteLine("Для выбора введите 1, 2, 3, 4 или 5 соответственно, ввод других символов приведет к завершению работы");
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
                case 4:
                    Flag = Regims.Метод_анализа_иерархий;
                    break;
                case 5:
                    Flag = Regims.Подход_MAUT;
                    break;
                default:
                    Flag = null;
                    break;
            }
        }

        private static void FillWeights(int critCnt)
        {
            Weights = new List<double>();
            for (int i = 0; i < critCnt; i++)
            {
                Console.WriteLine($"Введите вес критерия номер {i + 1}") ;
                if (double.TryParse(Console.ReadLine(), out double input))
                    Weights.Add(input);
                else
                {
                    Console.WriteLine("Некорректный ввод");
                    i -= 1;
                    continue;
                }
            }
        }

        private static void ЦелевоеПрограммирование(List<List<double>> matrix, int critCnt)
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

        private static void МетодАнализаИерархий(List<List<double>> matrix)
        {
            Matrix A = GetMatrixFromListOfLists(matrix);
            Console.WriteLine("Введенная матрица:");
            PrintMatrix(A);
            Console.WriteLine();

            var eigen = new EigenvalueDecomposition(A);
            double Lmax = 0;
            Console.WriteLine("Корни характеристического уравнения:");
            foreach (var item in eigen.RealEigenvalues)
            {
                Console.WriteLine(item.ToString("0.00"));
                Lmax = item > Lmax ? item : Lmax;
            }

            Console.WriteLine("Максимальный корень из найденных: " + Lmax.ToString("0.00"));
            Console.WriteLine();
            var CI = (Lmax - A.Columns) / (A.Columns - 1);
            Console.WriteLine("Индекс совместности: " + CI.ToString("0.00"));
            Console.WriteLine();
            if (CI > 0.1)
            {
                Console.WriteLine("Ошибка: Индекс совместности системы > 0.1");
                return;
            }

            // новое значение диагонального элемента матрицы
            for (int d = 0; d < A.Columns; d++)
            {
                A[d, d] = 1 - Lmax;
            }

            Console.WriteLine("Полученная матрица для составления однородной СЛАУ");
            PrintMatrix(A);

            // решить систему уравнений
            var B = new Matrix(A.Rows, 1);
            for (int i = 0; i < A.Rows; i++)
            {
                B[i, 0] = Math.Pow(0.1, 2);
            }

            // нормировать полученные значения
            var W = A.Solve(B);
            
            var summa = 0.0;

            for (int i = 0; i < A.Rows; i++)
                summa += Math.Round(W[i, 0], 2);

            for (int i = 0; i < A.Rows; i++)
                W[i, 0] = Math.Round(W[i, 0] / summa, 2);

            Console.WriteLine("Искомый нормированный весовой вектор:");
            PrintMatrix(W);
        }

        /// <summary>
        /// Костыль для преобразования List<List<double>> в объект Matrix
        /// </summary>
        private static Matrix GetMatrixFromListOfLists(List<List<double>> matrix)
        {
            var mtrx = new Matrix(matrix.Count, matrix[0].Count);

            for (int i = 0; i < matrix.Count; i++)
            {
                for (int j = 0; j < matrix[i].Count; j++)
                {
                    mtrx[i, j] = Math.Round(matrix[i][j], 2);
                }
            }

            return mtrx;
        }

        private static void PrintMatrix(Matrix matrix)
        {
            for (int i = 0; i < matrix.Rows; i++)
            {
                for (int j = 0; j < matrix.Columns; j++)
                {
                    Console.Write(matrix[i, j].ToString("0.00") + "  ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        private static void MAUT(List<List<double>> matrix)
        {
            //транспонируем матрицу для облегчения нахождения коэффициентов
            var mtrx = GetMatrixFromListOfLists(matrix);
            Console.WriteLine("Введена матрица:");
            PrintMatrix(mtrx);

            var matrixTrans = mtrx.Transpose();

            for (int i = 0; i < matrixTrans.Rows; i++)
            {
                Console.WriteLine("Обработка стобца: " + (i + 1));
                for (int j = 0; j < matrixTrans.Columns; j++)
                {
                    Console.WriteLine("Введите значение функции полезности для элемента " + matrixTrans[i, j]);

                    string input = Console.ReadLine();
                    if (!double.TryParse(input, out double result))
                    {
                        Console.WriteLine("Ввод некорректен");
                        j -= 1;
                        continue;
                    }
                    matrixTrans[i, j] = result;
                }
            }

            mtrx = matrixTrans.Transpose();
            var finalResult = new List<double>();

            Console.WriteLine("Умножение на веса критериев...");
            for (int i = 0; i < mtrx.Rows; i++)
            {
                double sum = 0;
                for (int j = 0; j < mtrx.Columns; j++)
                {
                    sum += mtrx[i, j] * Weights[j];
                }
                finalResult.Add(sum);
            }
            Console.WriteLine("Результаты: ");
            int k = 0;
            finalResult.ForEach(f => Console.WriteLine($"U({k++}) = {f}"));
        }

        public enum Regims
        {
            Алгоритм_Парето,
            Сужение_множества_Парето,
            Целевое_программирование,
            Метод_анализа_иерархий,
            Подход_MAUT
        }


    }
}
