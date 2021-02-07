using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace N2T_Assembler
{
    public class Assembler
    {
        public static string CleanData(string dataIn)
        {
            return dataIn.Split(new[] { "//" }, StringSplitOptions.None)[0].Trim().Replace(" ", string.Empty);
        }

        public static bool IsWhitespaceOrComment(string lineToCheck)
        {
            return lineToCheck.StartsWith("//") || string.IsNullOrWhiteSpace(lineToCheck);
        }

        private static void Main(string[] args)

        {
            //file input & naming
            Console.WriteLine("Please enter filepath: ");
            string sourceFilePath = Console.ReadLine();

            while (!File.Exists(sourceFilePath))
            {
                Console.WriteLine("Please enter filepath: ");
                sourceFilePath = Console.ReadLine();
            }

            string fileNameOut = Path.GetFileNameWithoutExtension(sourceFilePath) + ".hack";
            string destFilePath = Path.GetDirectoryName(sourceFilePath) + "\\" + fileNameOut;

            //initialise symbol table
            // dictionary for pre-defined symbols
            Dictionary<string, int> symbolDictionary =
                new Dictionary<string, int>
                {
                    {"R0", 0},
                    {"R1", 1},
                    {"R2", 2},
                    {"R3", 3},
                    {"R4", 4},
                    {"R5", 5},
                    {"R6", 6},
                    {"R7", 7},
                    {"R8", 8},
                    {"R9", 9},
                    {"R10", 10},
                    {"R11", 11},
                    {"R12", 12},
                    {"R13", 13},
                    {"R14", 14},
                    {"R15", 15},
                    {"SCREEN", 16384},
                    {"KBD", 24576},
                    {"SP", 0},
                    {"LCL", 1},
                    {"ARG", 2},
                    {"THIS", 3},
                    {"THAT", 4}
                };


            //read file
            IEnumerable<string> lines = File.ReadLines(sourceFilePath);

            // first pass - look for labels only
            string[] linesToRead = lines.ToArray();
            var linesRead = 0;

            foreach (string lineToRead in linesToRead)
            {
                if (IsWhitespaceOrComment(lineToRead))
                {
                    continue;
                }

                if (lineToRead.StartsWith("("))
                {
                    var label = lineToRead.Split('(', ')')[1];
                    symbolDictionary.Add(label, linesRead);
                }
                else
                {
                    linesRead++;
                }
            }

            //second pass - look for variables
            var assignedAddress = 16;
            foreach (string lineToRead in linesToRead)
            {
                if (IsWhitespaceOrComment(lineToRead))
                {
                    continue;
                }

                if (lineToRead.Contains("@"))
                {
                    var customSymbol = lineToRead.Split('@')[1];
                    bool isNumeric = int.TryParse(customSymbol, out _);
                    bool symbolExists = symbolDictionary.ContainsKey(customSymbol);

                    if (!symbolExists && !isNumeric)
                    {
                        symbolDictionary.Add(customSymbol, assignedAddress);
                        assignedAddress++;
                    }
                }
                else
                {
                    continue;
                }
            }

            // comp bits dictionary
            Dictionary<string, string> compDictionary =
                new Dictionary<string, string>
                {
                        {"0", "101010"},
                        {"1", "111111"},
                        {"-1", "111010"},
                        {"D", "001100"},
                        {"A", "110000"},
                        {"M", "110000"},
                        {"!D", "001101"},
                        {"!A", "110001"},
                        {"!M", "110001"},
                        {"-D", "001111"},
                        {"-A", "110011"},
                        {"-M", "110011"},
                        {"D+1", "011111"},
                        {"A+1", "110111"},
                        {"M+1", "110111"},
                        {"D-1", "001110"},
                        {"A-1", "110010"},
                        {"M-1", "110010"},
                        {"D+A", "000010"},
                        {"D+M", "000010"},
                        {"D-A", "010011"},
                        {"D-M", "010011"},
                        {"A-D", "000111"},
                        {"M-D", "000111"},
                        {"D&A", "000000"},
                        {"D&M", "000000"},
                        {"D|A", "010101"},
                        {"D|M", "010101"}
                };

            // destination bits dictionary
            Dictionary<string, string> destDictionary =
                new Dictionary<string, string>
                {
                        {"null", "000"},
                        {"M", "001"},
                        {"D", "010"},
                        {"MD", "011"},
                        {"A", "100"},
                        {"AM", "101"},
                        {"AD", "110"},
                        {"AMD", "111"}
                };

            // jump bits dictionary
            Dictionary<string, string> jumpDictionary =
                new Dictionary<string, string>
                {
                        {"null", "000"},
                        {"JGT", "001"},
                        {"JEQ", "010"},
                        {"JGE", "011"},
                        {"JLT", "100"},
                        {"JNE", "101"},
                        {"JLE", "110"},
                        {"JMP", "111"}
                };


            //third pass - check for A or C instruction, translate to binary & write to file
            Console.WriteLine("Translating & writing to file...");
            foreach (string lineToRead in linesToRead)
            {
                // write to file
                var dataToWrite = lines.ToArray();

                using StreamWriter outputFile = new StreamWriter(destFilePath);
                foreach (var line in dataToWrite)
                {
                    if (IsWhitespaceOrComment(lineToRead))
                    {
                        continue;
                    }
                    else
                    {
                        var cleanLine = CleanData(line); // remove inline comments
                        if (cleanLine.StartsWith("@"))
                        {
                            //process A instruction
                            var addressPart = cleanLine.Split('@')[1];

                            var address = symbolDictionary.ContainsKey(addressPart)
                                ? Convert.ToString(symbolDictionary[addressPart])
                                : addressPart;

                            var convertedAddress = Convert.ToString(int.Parse(address), 2).PadLeft(16, '0');
                            outputFile.WriteLine(convertedAddress);
                        }

                        if (cleanLine.Contains("=") || cleanLine.Contains(";"))
                        {
                            //process C instruction
                            string dest, comp, jump, 
                                destConverted, compConverted, jumpConverted, 
                                aBit, convertedString;

                            if (cleanLine.Contains("=") && !cleanLine.Contains(";")) //e.g. MD=M+1
                            {
                                dest = cleanLine.Split('=')[0];
                                destConverted = destDictionary[dest];

                                comp = cleanLine.Split('=', ';')[1];
                                compConverted = compDictionary[comp];
                                aBit = comp.Contains("M") ? "1" : "0";

                                convertedString = compConverted + destConverted + "000";
                                outputFile.WriteLine("111" + aBit + convertedString);
                            }

                            if (!cleanLine.Contains("=") && cleanLine.Contains(";")) //e.g. 0;JMP
                            {
                                comp = cleanLine.Split('=', ';')[0];
                                compConverted = compDictionary[comp];
                                aBit = comp.Contains("M") ? "1" : "0";

                                jump = cleanLine.Split('=', ';')[1];
                                jumpConverted = jumpDictionary[jump];

                                convertedString = compConverted + "000" + jumpConverted;
                                outputFile.WriteLine("111" + aBit + convertedString);
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }

                }
            }

            Console.WriteLine("File successfully assembled.");
        }
    }
}