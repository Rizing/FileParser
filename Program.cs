using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileParser
{
    internal class Program
    {
        private static readonly string outFileNameInt = "dest_int.txt";
        private static readonly string outFileNameDate = "dest_str.txt";
        private static readonly string outFileNameStr = "dest_dt.txt";

        private static string fileName;

        /// <summary>
        /// Основная функция
        /// </summary>
        /// <param name="args">Массив входных параметров</param>
        static void Main(string[] args)
        {
            GetFileName(args);
            IsCreateDataFile();
            ReadOrParseFile();
        }
        public static void ReadOrParseFile()
        {
            Parser();

            Console.ReadKey();
            Console.WriteLine("Press any key...");
        }

        public static void IsCreateDataFile()
        {
            if (!File.Exists(fileName))
            {
                Console.WriteLine();
                Console.WriteLine("Save file...");
                WriteTxt();
                Console.WriteLine("Save complate");
            }
            else
                Console.WriteLine();
        }


        public static void GetFileName(string[] args)
        {
            if (args.Length > 0)
            {
                fileName = args[0];
            }
            else
            {
                Console.WriteLine(@"Input file name...");
                fileName = Console.ReadLine();
            }
        }


        private static Random gen = new Random();
        public static DateTime RandomDateDay()
        {
            DateTime start = new DateTime(2022, 1, 1);
            int range = (DateTime.Today - start).Days;
            return start.AddDays(gen.Next(range));
        }


        /// <summary>
        /// Создаем файл со случайными данными (1000000 объектов)
        /// </summary>
        /// <param name="fileName">Имя создаваемого файла</param>
        public static void WriteTxt()
        {
            StringBuilder sb = new StringBuilder();

            //максимальная длинна блока 255 символов это ~ 250MB памяти (можем спокойно считать весь файл в память и не переживать что стэк кончится :) 
            for (var i = 0; i < 1000000; i++)
            {
                Random random = new Random();

                switch (random.Next(1, 3))
                {
                    case 1:
                        sb.Append(RandomDateDay().ToString("d"));
                        break;
                    case 2:
                        sb.Append(random.Next(1000000, 99999999).ToString());                        
                        break;
                    case 3:
                    default:
                        sb.Append(Guid.NewGuid().ToString());                        
                        break;
                }

                sb.Append(", ");
            }

            using (var writer = new StreamWriter(fileName))
            {
                writer.WriteLine(sb.ToString());
            }
        }

        public static string[] ReadTxt()
        {
            Console.WriteLine(@"Read file...");
            var allData = File.ReadAllText($"{fileName}", Encoding.Default).Replace("\n", "").Split(',');
            Console.WriteLine(@"Read comlate");
            return allData;
        }

        public static void Parser()
        {
            //Потокобезопасный справочник без дубликатов
            var dt = new ConcurrentDictionary<DateTime, string>();
            var di = new ConcurrentDictionary<int, string>();
            var ds = new ConcurrentDictionary<string, string>();

            var strAll = ReadTxt();

            ParallelLoopResult result = Parallel.ForEach(strAll, item =>
            {
                DateTime dtTemp;
                int iTemp;

                if (DateTime.TryParse(item, out dtTemp))
                    dt.TryAdd(dtTemp, item);
                else if (int.TryParse(item, out iTemp))
                    di.TryAdd(iTemp, item);
                else
                    ds.TryAdd(item, item);
            });

            var t = dt.OrderBy(x => x.Key).Select(date => date.ToString()).ToList();
            SaveData(outFileNameDate, t);

            var i = di.OrderBy(y => y.Key).Select(num => num.Key.ToString()).ToList();
            SaveData(outFileNameInt, i);

            var strS = ds.OrderBy(z => z.Key).Select(s => s.Key.ToString()).ToList();
            SaveData(outFileNameStr, strS);

            Console.WriteLine($"{result.IsCompleted} dt={dt.Count} di={di.Count} ds={ds.Count} нажмите что бы продолжить...");
            Console.ReadKey();
        }

        public static void SaveData(string writeFileName, List<string> data)
        {
            using (var writer = new StreamWriter(writeFileName))
            {
                foreach (var item in data)
                {
                    writer.WriteLine(item);
                }
            }
        }
    }
}
