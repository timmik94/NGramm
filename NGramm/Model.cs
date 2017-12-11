using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGramm
{
    class Model
    {
        [JsonIgnore]
       public  List<NGramm> bufffer = new List<NGramm>();


      public  List<NGramm> endmodel = new List<NGramm>();



        public void NormalizeModel(Model Prev,bool laplas)
        {
            if (Prev == null)
            {
                foreach (var item in endmodel)
                {
                    item.probability =((float) item.finded) / endmodel.Sum(ngr => ngr.finded);
                }
            }
            else
            {
                foreach (var item in endmodel)
                {
                    // var prev = item.text.Skip(item.text.Length - 1).Take(1).ToArray();

                    NGramm prevngr = Prev.endmodel.Find(ngr => new ArraysComparer().Compare(item.text.Skip(item.text.Length - 2).Take(1).ToArray(), ngr.text) == 0);
                    if (prevngr != null)
                    {
                        if (laplas)
                        {
                            item.probability = ((float)item.finded + 1) / prevngr.finded + Prev.endmodel.Count;
                        }
                        else
                        {
                            item.probability = ((float)item.finded) / prevngr.finded;
                        }
                    }
                   
                }
            }
        }



        public void AddToken(string[] token)
        {
            // bool Applied = false;
            NGramm ngr = endmodel.FirstOrDefault(ng => new ArraysComparer().Compare(ng.text, token) == 0);
            if (ngr != null)
            {
                // Applied = true;
                ngr.finded++;
            }
            else
            {
                ngr = bufffer.FirstOrDefault(ng => new ArraysComparer().Compare(ng.text, token) == 0);
                if (ngr != null)
                {
                    ngr.finded++;
                    if (ngr.finded > Program.unknowncount)
                    {
                        endmodel.Add(ngr);
                        bufffer.Remove(ngr);
                    }
                }
                else
                {
                    ngr = new NGramm(token);
                    if (Program.unknowncount == 0)
                    {
                        endmodel.Add(ngr);
                    }
                    else
                    {
                        bufffer.Add(ngr);
                    }
                }

            }


        }

    }
    public class NGramm
    {
        [JsonIgnore]
        public string[] text;
        public string textstr;
        [JsonIgnore]
        public int finded;
        public float probability;
        public NGramm(string[] t)
        {
            text = t;
            finded = 1;
            foreach (var item in t)
            {
                textstr += item;
                textstr += " ";
            }
        }

    }
    public class ArraysComparer : IComparer<string[]>
    {
        public int Compare(string[] x, string[] y)
        {
            if (x.Length != y.Length)
            {
                return -1;
            }
            int res = 0;
            for (int i = 0; i < x.Length; i++)
            {
                var x1 = x[i].ToLower();
                var y1 = y[i].ToLower();
                if (String.Compare(x1, y1) != 0)
                {
                    res = -1; break;
                }
                else
                {
                    res = 0;
                }
            }
            return res;
            //  throw new NotImplementedException();
        }
    }
}
