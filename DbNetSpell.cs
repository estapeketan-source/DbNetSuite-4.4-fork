using System;
using System.Text.RegularExpressions;
using System.Web;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using DbNetLink.Data;


namespace DbNetLink.DbNetSuite
{
    public class DbNetSpell : Shared
    {
        internal String SoundexGroups = "";
        internal String DictionaryTableName = "";
        internal String TokenBoundary = "";
        internal int MaximumSuggestions = 20;
        internal String[] Correlations = { "AÀÀÁÃÅÆ", "EÈÉË", "IÍÏ", "OÒÓÔÕ", "UÚÙÛÜ", "CÇ" };


        ///////////////////////////////////////////////
        public override void ProcessRequest(HttpContext context)
        ///////////////////////////////////////////////
        {
            base.ProcessRequest(context);

            switch (Req["method"].ToString())
            {
                case "initialize":
                    Initialize();
                    break;
                case "check":
                    CheckSpelling();
                    break;
                case "dialog":
                    BuildDialog();
                    break;
                case "add":
                    AddTokenToDictionary();
                    break;
            }

            context.Response.Write(JSON.Serialize(Resp));
        }

        ///////////////////////////////////////////////
        internal void Initialize()
        ///////////////////////////////////////////////
        {
            string V = "";
            if (this.ConnectionString == "")
            {
                V = ConfigurationManager.AppSettings["DbNetSpellConnectionString"];
                if (V != null)
                    this.ConnectionString = V;
            }

            if (this.DictionaryTableName == "")
            {
                V = ConfigurationManager.AppSettings["DbNetSpellDictionaryTableName"];
                if (V != null)
                    this.DictionaryTableName = V;   
            }

            Resp["connectionString"] = this.ConnectionString;
            Resp["dictionaryTableName"] = this.DictionaryTableName;
        }


        ///////////////////////////////////////////////
        internal void BuildDialog()
        ///////////////////////////////////////////////
        {
            DbNetSpellDialog D = new DbNetSpellDialog(this);
            Resp["html"] = RenderControlToString(D.Build());
        }

        ///////////////////////////////////////////////
        private void CheckSpelling()
        ///////////////////////////////////////////////
        {
            Regex TokenBoundaryRE = new Regex(this.TokenBoundary);
            Regex EnclosingQuote = new Regex("^[']+|[']+$");
            String SampleText = Req["text"].ToString();
            String[] Tokens = TokenBoundaryRE.Split(SampleText);
            String[] Suggestions;
            String Pattern;
            String UseDBMSSoundex = "no";

            this.OpenConnection();

            Dictionary<string, object> Corrections = new Dictionary<string, object>();
            List<string> Errors = new List<string>();

            for (int i = 0; i < Tokens.Length; i++)
            {
                Tokens[i] = EnclosingQuote.Replace(Tokens[i], "");

                if (Tokens[i].Length == 0)
                    continue;

                Pattern = TokenBoundaryRE.Replace(Tokens[i], "").Replace("'", "''");

                if (Pattern.Length == 0)
                    continue;

                if (MatchesDictionary(Pattern))
                    continue;

                Errors.Add(Tokens[i]);

                if (Corrections.ContainsKey(Tokens[i]))
                    continue;

                Suggestions = FindSuggestions(Pattern, UseDBMSSoundex);

                if (Pattern.ToUpper() == Pattern)
                {
                    for (int j = 0; j < Suggestions.Length; j++)
                        Suggestions[j] = Suggestions[j].ToUpper();
                }
                else
                {
                    if (Char.IsUpper(Pattern, 0))
                        for (int j = 0; j < Suggestions.Length; j++)
                            if (Suggestions[j].Length > 0)
                                Suggestions[j] = char.ToUpper(Suggestions[j][0]) + Suggestions[j].Substring(1, Suggestions[j].Length - 1);
                }

                List<string> SuggestionList = new List<string>();

                for (int j = 0; j < Suggestions.Length; j++)
                    SuggestionList.Add(Suggestions[j]);

                Corrections.Add(Tokens[i], SuggestionList);
            }

            this.CloseConnection();

            Resp["corrections"] = Corrections;
            Resp["errors"] = Errors;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void AddTokenToDictionary()
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            this.OpenConnection();

            string Word = Req["word"].ToString();

            if (!MatchesDictionary( Word ))
            {
                CommandConfig InsertConfig = new CommandConfig();
                InsertConfig.Sql = this.DictionaryTableName;
                InsertConfig.Params["pattern"] = Word.ToLower();
                InsertConfig.Params["soundex"] = Soundex(Word.ToLower());
                Database.ExecuteInsert(InsertConfig);
            }

            this.CloseConnection();
        }
        
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        private bool MatchesDictionary(String Pattern)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            QueryCommandConfig Query = new QueryCommandConfig();
            Query.Sql = "select pattern from " + this.DictionaryTableName + " where pattern = " + Database.ParameterName("pattern");
            Query.Params["pattern"] = Pattern.ToLower();
            return Database.ExecuteSingletonQuery(Query);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        private string[] FindSuggestions(String s, String UseDBMSSoundex)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            List<string> Suggest = new List<string>();
            ArrayList Ranking = new ArrayList();
            SortableWordSimilarityPairing Swp;

            QueryCommandConfig Query = new QueryCommandConfig();

            if (UseDBMSSoundex == "yes")
                Query.Sql = "select pattern, soundex(pattern) from " + this.DictionaryTableName +
                    " where soundex(pattern) = soundex('" + s + "')";
            else
            {
                Query.Sql = "select pattern, soundex from " + this.DictionaryTableName + " where soundex = " + Database.ParameterName("soundex") + " order by soundex";
                Query.Params["soundex"] = Soundex(s);
            }

            Database.ExecuteQuery(Query);

            while (Database.Reader.Read())
            {
                if (Database.ReaderValue(0).ToString() != "")
                {
                    Swp = new SortableWordSimilarityPairing(
                        Database.ReaderValue(0).ToString(),
                        Similarity(s, Database.ReaderValue(0).ToString()));
                    Ranking.Add(Swp);
                }
            }

            Database.Reader.Close();

            Ranking.Sort();
            Ranking.Reverse();
            if (Ranking.Count > this.MaximumSuggestions)
                Ranking.RemoveRange(this.MaximumSuggestions, Ranking.Count - this.MaximumSuggestions);

            System.Collections.IEnumerator RankingEnumerator = Ranking.GetEnumerator();

            while (RankingEnumerator.MoveNext())
            {
                Suggest.Add(((SortableWordSimilarityPairing)RankingEnumerator.Current).Word);
            }

            return Suggest.ToArray();
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        private double Similarity(string s1, string s2)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            int WordLen = s1.Length;
            int SimilarWordLen = s2.Length;
            int MaxBonus = 3;
            double PerfectValue = WordLen + SimilarWordLen + MaxBonus;
            double Similarity = MaxBonus - Math.Abs(WordLen - SimilarWordLen);

            for (int i = 0; i < WordLen; i++)
            {
                if (i < SimilarWordLen)
                {
                    if (s1.ToLower().Substring(i, 1) == s2.ToLower().Substring(i, 1))
                        Similarity++;

                    if (s1.ToLower().Substring((WordLen - 1) - i, 1) == s2.ToLower().Substring((SimilarWordLen - 1) - i, 1))
                        Similarity++;
                }
            }

            return (Similarity / PerfectValue);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        private string Soundex(string s)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            String sx = GetSoundexInitial(s), code;

            string[] SoundexGroupsArray = SoundexGroups.Split(',');

            for (int i = 1; i < s.Length; i++)
            {
                code = "";
                for (int j = 0; j < SoundexGroupsArray.Length; j++)
                {
                    if (SoundexGroupsArray[j].IndexOf(s.ToUpper().Substring(i, 1)) >= 0)
                    {
                        code = (j + 1).ToString();
                        break;
                    }
                }
                if (!sx.EndsWith(code))
                    sx = sx + code;
            }

            sx += "000";

            return sx.Substring(0, 4);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        private string GetSoundexInitial(string s)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            string init = s.ToUpper().Substring(0, 1);

            foreach (string Correlation in this.Correlations)
                if (Correlation.IndexOf(init) >= 0)
                    return Correlation.Substring(0, 1);

            return init;
        }
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    public class SortableWordSimilarityPairing : IComparable
    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    {
        public String Word;
        public Double Similarity;

        public SortableWordSimilarityPairing(String w, Double s)
        {
            this.Word = w;
            this.Similarity = s;
        }


        public int CompareTo(object O)
        {
            SortableWordSimilarityPairing TmpSwp = (SortableWordSimilarityPairing)O;

            if (this.Similarity < TmpSwp.Similarity)
                return -1;

            if (this.Similarity > TmpSwp.Similarity)
                return 1;

            return 0;
        }
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    public class SWSPComparer : IComparer
    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    {
        int IComparer.Compare(object x, object y)
        {
            SortableWordSimilarityPairing Swp1 = (SortableWordSimilarityPairing)x;
            SortableWordSimilarityPairing Swp2 = (SortableWordSimilarityPairing)y;

            return Swp1.Similarity.CompareTo(Swp2.Similarity);
        }
    }
}