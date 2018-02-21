using OpenNLP.Tools.SentenceDetect;
using OpenNLP.Tools.Tokenize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGramm
{
    class Checkercs
    {
        public Model model;
       // Dictionary<string, int> DWORDS;
        const string alphabet = "abcdefghijklmnopqrstuvwxyz";

        //public string CheckString(string wrong)
        //{
        //    string[] words=
        //}

        private static IEnumerable<string> edits1(string word)
        {
            var splits = from i in Enumerable.Range(0, word.Length)
                         select new { a = word.Substring(0, i), b = word.Substring(i) };
            var deletes = from s in splits
                          where s.b != "" // we know it can't be null
                          select s.a + s.b.Substring(1);
            var transposes = from s in splits
                             where s.b.Length > 1
                             select s.a + s.b[1] + s.b[0] + s.b.Substring(2);
            var replaces = from s in splits
                           from c in alphabet
                           where s.b != ""
                           select s.a + c + s.b.Substring(1);
            var inserts = from s in splits
                          from c in alphabet
                          select s.a + c + s.b;

            return deletes
            .Union(transposes) // union translates into a set
            .Union(replaces)
            .Union(inserts);
        }
        private  List<string> known_edits2(string word)
        {
            return (from e1 in edits1(word)
                    from e2 in edits1(e1)
                    where model.ExistsWord(e2)!=null
                    select model.ExistsWord(e2).sts)
                   .Distinct().ToList();
        }
        private  List<Candidate> known(IEnumerable<string> words)
        {
            return words.Where(w => model.ExistsWord(w)!=null).Select(w => model.ExistsWord(w)).ToList();
        }

        public string Correct(string word)
        {
            Candidate currword = known(new[] { word }).First()??new Candidate() {sts=word,prob=0 };
            var candidates = new List<Candidate>();
            candidates.Add(currword);
            candidates.AddRange( known(edits1(word)));
            candidates.AddRange(known(known_edits2(word)));
            return candidates.OrderByDescending(cnd => cnd.prob).First().sts;// OrderByDe(cnd=>model.probability(str));
        }



        public string CorrectSentence(string sent)
        {
            var modelPath = "EnglishTok.nbin";
            var tokenizer = new EnglishMaximumEntropyTokenizer(modelPath);

            var sentDetector = new EnglishMaximumEntropySentenceDetector("EnglishSD.nbin");

           
            List<string> tokens = new List<string>();
            tokens.AddRange(tokenizer.Tokenize(sent));
            List<string> correct = new List<string>();
            foreach(var token in tokens)
            {
                if (Program.Applywords(token))
                {
                    correct.Add( Correct(token));
                }
                else { correct.Add(token); }
            }

            string res =string.Join("", correct.Select(str=>Program.Applywords(str)?str += " " : str));
            return res;


        }


    }

    public class Candidate
    {
        public string sts;
        public float prob;
    }

}

