using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LigthStringEditor {
    public class DatTL {

        LightDat Editor;
        public DatTL(byte[] Script) { Editor = new LightDat(Script); }

        private Dictionary<SplitInf, string> Cutted;
        private Dictionary<uint, uint> SplitMap;

        private Dictionary<uint, string> Prefix;
        private Dictionary<uint, string> Sufix;

        private uint StrCount;
        public string[] Import() {
            SplitMap = new Dictionary<uint, uint>();
            Cutted = new Dictionary<SplitInf, string>();
            Prefix = new Dictionary<uint, string>();
            Sufix = new Dictionary<uint, string>();

            string[] Strings = Editor.Import();
            StrCount = (uint)Strings.LongLength;
            List<char> DenyChars = new List<char>(new char[] { '\t', '\n', '\r', '\a', '\b', '\0' });

            List<string> SplitedStrings = new List<string>();
            for (uint x = 0; x < Strings.LongLength; x++) {
                string String = Strings[x];
                Clear(ref String, x, DenyChars);

                List<string> Split = new List<string>();

                int Start = 0;
                for (int i = 0; i < String.Length; i++) {
                    string CutContent = string.Empty;
                    while (DenyChars.Contains(String[i]) || String[i] < 32) {
                        CutContent += String[i++];
                    }

                    if (CutContent != string.Empty) {
                        SplitInf Inf = new SplitInf();
                        Inf.Split = Split.Count;
                        Inf.String = x;
                        Cutted.Add(Inf, CutContent);

                        Split.Add(String.Substring(Start, i - CutContent.Length - Start));
                        Start = i;
                    }
                }

                Split.Add(String.Substring(Start, String.Length - Start));

                SplitMap.Add(x, (uint)Split.LongCount());
                SplitedStrings.AddRange(Split);
            }
            //string tmp = SplitedStrings[59946];

            return SplitedStrings.ToArray();
        }

        private void Clear(ref string String, uint ID, List<char> DenyChars) {
            string Prefix = string.Empty;
            while (String.Length > 0 && (DenyChars.Contains(String[0]) || String[0] < 32)) {
                Prefix += String[0];
                String = String.Substring(1, String.Length - 1);
            }

            string Sufix = string.Empty;
            while (String.Length > 0 && (DenyChars.Contains(String[String.Length - 1]) || String[String.Length - 1] < 32)) {
                Sufix = String[String.Length - 1] + Sufix;
                String = String.Substring(0, String.Length - 1);
            }

            this.Prefix.Add(ID, Prefix);
            this.Sufix.Add(ID, Sufix);
        }

        public byte[] Export(string[] Strings) {
            List<string> MergedStrings = new List<string>();
            for (uint x = 0, z = 0; x < StrCount; x++) {
                string Str = string.Empty;
                for (int i = 0; i < SplitMap[x]; i++) {
                    SplitInf Inf = new SplitInf();
                    Inf.Split = i;
                    Inf.String = x;

                    Str += Strings[z++];
                    if (i < SplitMap[x] - 1)
                        Str += Cutted[Inf];
                }

                MergedStrings.Add(Prefix[x] + Str + Sufix[x]);
            }

            if (StrCount != MergedStrings.LongCount())
                throw new Exception("Failed to Merge Strings.");

            return Editor.Export(MergedStrings.ToArray());
        }
        internal struct SplitInf {
            internal uint String;
            internal int Split;
        }
    }
}
