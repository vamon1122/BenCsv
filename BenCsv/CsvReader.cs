using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenLog;

namespace BenCsv
{
    public static class Csv
    {
        private static LogWriter MyLog;

        static Csv()
        {
            MyLog = new LogWriter("BenCsv.log", AppDomain.CurrentDomain.BaseDirectory);
        }


        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\
        //Public methods


        public static bool CanReadFile(string pDir)
        {
            try
            {
                ParseCsvFile(pDir);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static List<string[]> ReadFile(string pDir)
        {   
            if(pDir.First() == '"')
            {
                pDir = pDir.Substring(1, pDir.Length - 1);
            }

            if (pDir.Last() == '"')
            {
                pDir = pDir.Substring(0, pDir.Length - 1);
            }

            return ParseCsvFile(pDir);
        }

        public static bool WriteToFile(List<string[]> pRows, string pDir)
        {
            try
            {
                using(StreamWriter sr = new StreamWriter(pDir))
                {
                    sr.WriteLine(GenCsvString(pRows));
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static string GenCsvString(string[] pRow)
        {
            string CsvString = "";

            foreach (string cell in pRow)
            {
                string newCell = cell;
                if (cell.Contains('"'))
                {
                    newCell.Replace("\"", "\"\"");
                }

                if (cell.Contains(" ") || cell.Contains("\"") || cell.Contains(","))
                {
                    //newCell.Insert(1, "\""); This doesn't work for some reason
                    newCell = '"' + newCell;
                    newCell += '"';
                }

                CsvString += newCell;

                if (cell != pRow.Last())
                {
                    CsvString += ',';
                }
            }
            return CsvString;
        }

        public static string GenCsvString(List<string[]> pRows)
        {
            string CsvString = "";

            foreach (string[] row in pRows)
            {
                GenCsvString(row);
                CsvString += Environment.NewLine;
            }
            return CsvString;
        }

        public static string[] ParseCsvLine(string pLine)
        {
            MyLog.Info("Parsing CSV line...");
            MyLog.Debug(String.Format("Parsing line \"{0}\"", pLine));

            var Line = pLine;
            string[] Values;


            List<string> TempValues = new List<string>();
            //1.8.10 - Added this if statement. Sorts out values which have commas in them!
            int Zero = 0;
            int ValueNo = 0;
            while (Line.Length != 0)
            {


                MyLog.Debug(String.Format("LINE VALUE {0}", ValueNo));
                ValueNo++;

                //Get rid of the comma if it's the next value on the line
                if (Line.Contains(","))
                {
                    if (Line.IndexOf(",") == 0)
                    {

                        Line = Line.Substring(1, Line.Length - 1);

                    }
                }

                //Gets the next value
                string ActiveVal;
                if (Line.Contains(","))
                {
                    MyLog.Debug("There are more commas to parse.");
                    ActiveVal = Line.Substring(Zero, Line.IndexOf(","));
                }
                else
                {
                    MyLog.Debug("There are NO more commas to parse.");
                    ActiveVal = Line;
                }


                MyLog.Debug(String.Format("NEXT ActiveVal = \"{0}\"", ActiveVal));
                Line = Line.Substring(ActiveVal.Length, Line.Length - ActiveVal.Length);
                MyLog.Debug(String.Format("Meaning that Line now = \"{0}\"", Line));

                if (ActiveVal.Contains('"'))
                {
                    if (ActiveVal.Length >= 2)
                    {
                        if (ActiveVal.Substring(ActiveVal.Length - 1, 1) == "\"" && (ActiveVal.Split('"').Length - 1) % 2 == 0) //Added the second argument because if there is an odd number
                        {                                                                                                       //the string is not complete (some values were being marked
                            MyLog.Debug(String.Format("ActiveVal \"{0}\" is already complete!", ActiveVal));                    //as complete when they weren't

                        }
                        else
                        {
                            CompleteLine();
                        }
                    }
                    else
                    {
                        CompleteLine();
                    }
                }
                else
                {
                    MyLog.Debug(String.Format("ActiveVal \"{0}\" is already complete!", ActiveVal));
                }

                void CompleteLine()
                {
                    MyLog.Debug(String.Format("ActiveVal \"{0}\" is NOT complete. Completing ActiveVal...", ActiveVal));
                    if (Line.Contains("\","))
                    {
                        MyLog.Debug("Line contains \",");
                        MyLog.Debug("Index of \", = " + Line.IndexOf("\","));

                        ActiveVal += Line.Substring(0, Line.IndexOf("\",") + 1);
                        Line = Line.Substring(Line.IndexOf("\",") + 1, Line.Length - Line.IndexOf("\",") - 1); //Added the +1/-1 because " wasn't being removed

                    }
                    else
                    {
                        MyLog.Debug("Line does not contain \",");
                        ActiveVal += Line;
                        Line = "";
                    }


                    if ((ActiveVal.Split('"').Length - 1) % 2 != 0)
                    {
                        MyLog.Debug(String.Format("ActiveVal still is incomplete. ActiveVal = \"{0}\"", ActiveVal));
                        MyLog.Debug(String.Format("Line = \"{0}\"", Line));
                        CompleteLine();
                    }
                    else
                    {
                        MyLog.Debug(String.Format("Completed ActiveVal successfully! ActiveVal = \"{0}\"", ActiveVal));
                        MyLog.Debug(String.Format("Line = \"{0}\"", Line));
                    }
                }

                if (ActiveVal.Length > 0) //Fixes bug where file would not be imported if value was blank
                {
                    if (ActiveVal[0] == '"')
                    {
                        MyLog.Debug("ActiveVal starts with a quotation mark. Removing quotation mark...");
                        ActiveVal = ActiveVal.Substring(1, ActiveVal.Length - 1);
                        MyLog.Debug(String.Format("Successfully removed quotation mark. ActiveVal now = \"{0}\"", ActiveVal));

                    }

                    if (ActiveVal.Last() == '"')
                    {
                        MyLog.Debug("ActiveVal ends with a quotation mark. Removing quotation mark.");
                        ActiveVal = ActiveVal.Substring(0, ActiveVal.Length - 1);
                        MyLog.Debug(String.Format("Successfully removed quotation mark. ActiveVal now = \"{0}\"", ActiveVal));
                    }

                    //Quotation marks (") are saved as ("") in CSV files. This changes them back to (")
                    if (ActiveVal.Contains('"'))
                    {
                        MyLog.Debug("Formatting quotation marks...");
                        ActiveVal = ActiveVal.Replace("\"\"", "\"");
                        MyLog.Debug(String.Format("Formatted quotation marks successfully! ActiveVal now = \"{0}\"", ActiveVal));
                    }
                }

                MyLog.Debug("Adding string to TempValues...");
                TempValues.Add(ActiveVal);
                MyLog.Debug("Successfully added string to TempValues");
            }
            Values = TempValues.ToArray();
            return Values;
        }


        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\
        //Private methods



        private static List<string[]> ParseCsvFile(string pDir)
        {
            MyLog.Info("Parsing CSV file...");
            List<string[]> MyLines = new List<string[]>();

            using (StreamReader pReader = new StreamReader(pDir))
            {
                while (!pReader.EndOfStream)
                {
                    MyLines.Add(ParseCsvLine(pReader.ReadLine()));
                    MyLog.Break();
                }
            }
            MyLog.Info("Successfully parsed CSV file!");
            return MyLines;
        }

        

            
        }
    }
