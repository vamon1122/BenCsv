using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenCsv;

namespace TestCsv
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.Write("CSV Dir: ");
            List<string[]> MyCsv = CsvReader.ReadFile(Console.ReadLine());

            string[] Alphabet = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

            int Line = 0;

            foreach(string[] CsvLine in MyCsv)
            {
                int Cell = 0;
                foreach(string CsvCell in CsvLine)
                {
                    if(Alphabet[Cell] + (Line + 1) == CsvCell)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                    }
                    Console.WriteLine(String.Format("{0} = {1}",Alphabet[Cell] + (Line + 1), CsvCell));
                    Cell++;
                }
                Line++;
            }

            Console.ReadLine();
        }
    }
}
