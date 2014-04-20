using System;
using System.Diagnostics;
using System.Windows.Forms;
using ProdigalSoftware.TiVE;

namespace ProdigalSoftware.ProjectM
{
    static class Program
    {
        const string Format = "{0}: {1,7:0.000} ";
        const int dim = 80;
        private const int PassCount = 300;
        private const int TestSize = dim * dim * dim;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //TestStructClassAccess();
            //TestArrayMethods();

            TiVEController.RunStarter();
        }

        #region Test struct vs. class access speed
        private static void TestStructClassAccess()
        {
            Struct();
            StructUnsafe();
            Class();
        }

        private static void Struct()
        {
            TestStruct[] values = new TestStruct[TestSize];

            double total = 0.0;
            var timer = new Stopwatch();
            for (int passes = 0; passes < PassCount; passes++)
            {
                timer.Restart();
                for (int i = 0; i < TestSize; i++)
                    UpdateStruct(ref values[i]);
                timer.Stop();
                total += timer.ElapsedTicks * 1000.0 / Stopwatch.Frequency;
                //Console.WriteLine("values: {0}, {1}, {2}, {3}, {4}, {5}, {6}", 
                //    values[0].Item1, values[0].Item2, values[0].Item3, values[0].Item4, values[0].Item5, values[0].Item6, values[0].Item7);
            }

            Console.WriteLine(Format, "Struct", total / PassCount);
        }

        private static void UpdateStruct(ref TestStruct val)
        {
            val.Item4 += 3.1f;
            val.Item5 += 1.2f;
            val.Item6 += 4.7f;
            val.Item1 += val.Item4;
            val.Item2 += val.Item5;
            val.Item3 += val.Item6;
            val.Item7++;
        }

        private static unsafe void StructUnsafe()
        {
            TestStruct[] values = new TestStruct[TestSize];

            double total = 0.0;
            var timer = new Stopwatch();
            for (int passes = 0; passes < PassCount; passes++)
            {
                timer.Restart();
                fixed (TestStruct* bla = &values[0])
                {
                    TestStruct* valueList = bla;
                    for (int i = 0; i < TestSize; i++)
                        UpdateStructUnsafe(valueList++);
                }
                timer.Stop();
                total += timer.ElapsedTicks * 1000.0 / Stopwatch.Frequency;
                //Console.WriteLine("values: {0}, {1}, {2}, {3}, {4}, {5}, {6}", 
                //    values[0].Item1, values[0].Item2, values[0].Item3, values[0].Item4, values[0].Item5, values[0].Item6, values[0].Item7);
            }

            Console.WriteLine(Format, "Unsafe struct", total / PassCount);
        }

        private static unsafe void UpdateStructUnsafe(TestStruct* val)
        {
            val->Item4 += 3.1f;
            val->Item5 += 1.2f;
            val->Item6 += 4.7f;
            val->Item1 += val->Item4;
            val->Item2 += val->Item5;
            val->Item3 += val->Item6;
            val->Item7++;
        }

        private static void Class()
        {
            TestClass[] values = new TestClass[TestSize];
            for (int i = 0; i < values.Length; i++)
                values[i] = new TestClass();

            double total = 0.0;
            var timer = new Stopwatch();
            for (int passes = 0; passes < PassCount; passes++)
            {
                timer.Restart();
                for (int i = 0; i < TestSize; i++)
                    UpdateClass(values[i]);
                timer.Stop();
                total += timer.ElapsedTicks * 1000.0 / Stopwatch.Frequency;
                //Console.WriteLine("values: {0}, {1}, {2}, {3}, {4}, {5}, {6}",
                //    values[0].Item1, values[0].Item2, values[0].Item3, values[0].Item4, values[0].Item5, values[0].Item6, values[0].Item7);
            }

            Console.WriteLine(Format, "Class", total / PassCount);
        }

        private static void UpdateClass(TestClass val)
        {
            val.Item4 += 3.1f;
            val.Item5 += 1.2f;
            val.Item6 += 4.7f;
            val.Item1 += val.Item4;
            val.Item2 += val.Item5;
            val.Item3 += val.Item6;
            val.Item7++;
        }

        private struct TestStruct
        {
            public float Item1;
            public float Item2;
            public float Item3;
            public float Item4;
            public float Item5;
            public float Item6;
            public int Item7;
        }

        private sealed class TestClass
        {
            public float Item1;
            public float Item2;
            public float Item3;
            public float Item4;
            public float Item5;
            public float Item6;
            public int Item7;
        }
        #endregion

        #region Test array-type speeds
        private static void TestArrayMethods()
        {
            Jagged();
            Multi();
            Single();
        }

        private static void Jagged()
        {
            var jagged = new int[dim][][];
            for (var i = 0; i < dim; i++)
            {
                jagged[i] = new int[dim][];
                for (var j = 0; j < dim; j++)
                    jagged[i][j] = new int[dim];
            }

            double total = 0.0;
            var timer = new Stopwatch();
            for (var passes = 0; passes < PassCount; passes++)
            {
                timer.Restart();
                for(var i = 0; i < dim; i++)
                {
                    for(var j = 0; j < dim; j++)
                    {
                        for(var k = 0; k < dim; k++)
                            jagged[i][j][k] = i * j * k;
                    }
                }
                timer.Stop();
                total += timer.ElapsedTicks * 1000.0 / Stopwatch.Frequency;
            }
            Console.WriteLine(Format, "Jagged arrays", total / PassCount);
        }

        private static void Multi()
        {
            var multi = new int[dim, dim, dim];
            var timer = new Stopwatch();
            double total = 0.0;
            for (var passes = 0; passes < PassCount; passes++)
            {
                timer.Restart();
                for(var i = 0; i < dim; i++)
                {
                    for(var j = 0; j < dim; j++)
                    {
                        for(var k = 0; k < dim; k++)
                            multi[i, j, k] = i * j * k;
                    }
                }
                timer.Stop();
                total += timer.ElapsedTicks * 1000.0 / Stopwatch.Frequency;
            }
            Console.WriteLine(Format, "Multi-dimensional arrays", total / PassCount);
        }

        private static void Single()
        {
            var single = new int[dim * dim * dim];
            var timer = new Stopwatch();
            double total = 0.0;
            for (var passes = 0; passes < PassCount; passes++)
            {
                timer.Restart();
                for(var i = 0; i < dim; i++)
                {
                    for(var j = 0; j < dim; j++)
                    {
                        for(var k = 0; k < dim; k++)
                            single[i * dim * dim + j * dim + k] = i * j * k;
                    }
                }
                timer.Stop();
                total += timer.ElapsedTicks * 1000.0 / Stopwatch.Frequency;
            }
            Console.WriteLine(Format, "Single array", total / PassCount);
            Console.WriteLine();
        }
        #endregion
    }
}
