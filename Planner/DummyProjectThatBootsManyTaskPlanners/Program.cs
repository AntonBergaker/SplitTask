using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DummyProjectThatBootsManyTaskPlanners
{
    class Program
    {
        static void Main(string[] args)
        {
            DirectoryInfo directory = Directory.GetParent(Directory.GetParent(Directory.GetParent(Environment.CurrentDirectory).ToString()).ToString());
            string path = directory + @"\TaskPlanner\bin\Debug\TaskPlanner.exe";
            for (int i = 0; i < 2; i++)
            { System.Diagnostics.Process.Start(path); }
        }
    }
}
