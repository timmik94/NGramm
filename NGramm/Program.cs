
//using edu.stanford.nlp.coref.data;
using Newtonsoft.Json;
using OpenNLP.Tools.SentenceDetect;
using OpenNLP.Tools.Tokenize;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NGramm
{
    class Program
    {

        static Model[] models;
        public static int unknowncount;
        static void Main(string[] args)
        {
            Console.WriteLine("Corpus:");
            string corpus = Console.ReadLine();
            Console.WriteLine("WordType");
            string WordType = Console.ReadLine();

            GetFilter(WordType, out Func<string,bool> filter);
            Console.WriteLine("n");
            int n = int.Parse(Console.ReadLine());
            models = new Model[n];
            for (int i = 0; i < n; i++)
            {
                models[i] = new Model();
            }
            Console.WriteLine("smoothing");
            string sm = Console.ReadLine();
            bool laplas = false;
            if (sm == "laplas") { laplas = true; }
            Console.WriteLine("Unknown");
            int unk = int.Parse(Console.ReadLine());
            unknowncount = unk;
            Console.WriteLine("OutputFile");
            string path = Console.ReadLine();

            string[] corpfiles = Directory.GetFiles(corpus);
            //List<string> allTokens=new List<string>();
            foreach (var item in corpfiles)
            {
                List<string> filetokens = WorkFile(item);
                var tokens=FilterTokens(filetokens, filter);
                BuildModel(n, tokens);
            }



            models[n - 1].NormalizeModel(GetPrevModel(n), laplas);
            StreamWriter sw = new StreamWriter(path, false);// FileMode.Create);
            sw.WriteLine( JsonConvert.SerializeObject(models[n - 1]));
            sw.Close();
       //     model = new Model();


        }


        static Model GetPrevModel(int n)
        {
            if (n > 1)
            {
                return models[0];
            }
            else
            {
                return null;
            }
        }

        static void BuildModel(int n,List<string> tokens)
        {
            int modelIndex = n - 1;
            for(int i = 0; i <= modelIndex; i++)
            {
                Model currentmodel = models[i];
                for(int j = 0; j < tokens.Count - n; j++)
                {
                    string[] token = tokens.Skip(j).Take(i+1).ToArray();
                    currentmodel.AddToken(token);

                }
            }
        }

        public static void GetFilter(string input,out Func<string,bool> filter)
        {
            switch (input)
            {
                case "all":filter = ApplyAll;break;
                case "no_pm":filter = Applywords;break;
                default:filter = ApplyAll;
                    break;
            }
        }

        public static bool ApplyAll(string s)
        {
            if (s == "") { return false; }
            return true;
        }

        public static bool Applywords(string s)
        {
            Regex noAlphaNum = new Regex(@"\W+");
            Regex noNum = new Regex(@"\D+");
            if (noAlphaNum.Matches(s).Count > 0)
            {
                return false;
            }
            if (noNum.IsMatch(s)){
                return true;
            }
            return false;

        }


        public static List<string> WorkFile(string path)
        {
            //model = new Model[n];
            using (StreamReader sr = new StreamReader(path))
            {
                var modelPath = "EnglishTok.nbin";
                var tokenizer = new EnglishMaximumEntropyTokenizer(modelPath);

                var sentDetector = new EnglishMaximumEntropySentenceDetector("EnglishSD.nbin");
                var text = sr.ReadToEnd();
                sr.Close();
                var sentencies = sentDetector.SentenceDetect(text);
                List<string> tokens = new List<string>();
                foreach (var item in sentencies)
                {
                    tokens.AddRange(tokenizer.Tokenize(item));
                }
                return tokens;
            }
        }

        public static List<string> FilterTokens(List<string> tokens,Func<string,bool> apply)
        {
            return tokens.Where(apply).ToList();
        }

    }
}
