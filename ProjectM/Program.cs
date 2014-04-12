using System;
using System.Diagnostics;
using System.Windows.Forms;
using ProdigalSoftware.TiVE;

namespace ProdigalSoftware.ProjectM
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //Jagged();
            //Multi();
            //Single();

            TiVEController.RunStarter();
        }

        const string Format = "{0,7:0.000} ";
        const int dim = 200;
        static void Jagged()
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
            for (var passes = 0; passes < 100; passes++)
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
            Console.WriteLine(Format, total / 100.0);
        }

        static void Multi()
        {
            var multi = new int[dim, dim, dim];
            var timer = new Stopwatch();
            double total = 0.0;
            for (var passes = 0; passes < 100; passes++)
            {
                timer.Restart();
                for(var i = 0; i < dim; i++)
                {
                    for(var j = 0; j < dim; j++)
                    {
                        for(var k = 0; k < dim; k++)
                            multi[i,j,k] = i * j * k;
                    }
                }
                timer.Stop();
                total += timer.ElapsedTicks * 1000.0 / Stopwatch.Frequency;
            }
            Console.WriteLine(Format, total / 100.0);
        }

        static void Single()
        {
            var single = new int[dim * dim * dim];
            var timer = new Stopwatch();
            double total = 0.0;
            for (var passes = 0; passes < 100; passes++)
            {
                timer.Restart();
                for(var i = 0; i < dim; i++)
                {
                    for(var j = 0; j < dim; j++)
                    {
                        for(var k = 0; k < dim; k++)
                            single[i*dim*dim+j*dim+k] = i * j * k;
                    }
                }
                timer.Stop();
                total += timer.ElapsedTicks * 1000.0 / Stopwatch.Frequency;
            }
            Console.WriteLine(Format, total / 100.0);
            Console.WriteLine();
        }
    }
}
