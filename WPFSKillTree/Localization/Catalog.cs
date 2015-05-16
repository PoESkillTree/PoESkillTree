using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using ExpressionEvaluator;

namespace POESKillTree.Localization
{
    // @see https://www.gnu.org/software/gettext/manual/html_node/PO-Files.html#PO-Files
    public class Catalog
    {
        // Plural 'n' expression.
        internal class NExpression : CompiledExpression<int>
        {
            // The scoped lambda function.
            private Func<NExpression, int> fn;

            // The value of 'n' in expression for evaluation.
            public int n { get; set; }

            // Compiles expression.
            public NExpression(string expr)
                : base(expr)
            {
                fn = ScopeCompile<NExpression>();
            }

            // Evaluates expression.
            public uint Eval(uint num)
            {
                n = (int) num;

                return (uint) fn(this);
            }
        }

        // PO format parser.
        private class Parser
        {
            // The keyword types.
            private enum KeywordType { None, MsgCtxt, MsgId, MsgIdPlural, MsgStr }

            // The flag whether Parser processed header entry.
            private bool HasHeader = false;
            // The current index of translated string.
            private int Index;
            // The flag whether Parser reported complete entry (i.e. ParseLine method returned true).
            private bool IsComplete;
            // The flag whether current entry is header entry.
            private bool IsHeader { get { return Id == ""; } }
            // The flag whether current entry is malformed.
            private bool IsMalformed;
            // The flag whether current entry is valid.
            private bool IsValid
            {
                get
                {
                    // Entry is valid if it has msgid
                    if (Id != null && Strings != null)
                    {
                        // For regular message, the number of translated strings must be exactly 1.
                        if (IdPlural == null && Strings.Length > 1)
                            return false;
                        // For plural entry the number of translated strings must match NPlurals.
                        else if (IdPlural != null && Strings.Length != NPlurals)
                            return false;

                        // All translated strings must be non-empty strings.
                        foreach (string str in Strings)
                            if (String.IsNullOrEmpty(str)) return false;

                        return true;
                    }

                    return false;
                }
            }
            // The current keyword being processed.
            private KeywordType Keyword;
            // The current string being parsed.
            private string Str;

            // The message context.
            public string Context;
            // The plural 'n' expression.
            public NExpression Plural = null;
            // The message identifier.
            public string Id;
            // The message plural identifier.
            public string IdPlural;
            // The number of plural forms.
            public uint NPlurals = 0;
            // The translated string(s).
            public string[] Strings;

            public Parser()
            {
                Reset();
            }

            // End of entry.
            // Returns true if entry is valid.
            private bool End()
            {
                if (!IsMalformed)
                {
                    // No keyword was processed, invalid operation.
                    if (Keyword == KeywordType.None)
                        throw new InvalidOperationException("No entry");

                    // Pending string.
                    if (Str != null)
                    {
                        Next(KeywordType.None);
                    }

                    // Check if entry is valid.
                    if (IsValid)
                    {
                        if (IsHeader)
                            Header();
                        else
                        {
                            // Flag entry as complete.
                            IsComplete = true;
                            return true;
                        }
                    }
                }

                Reset();

                return false;
            }

            // Process header entry.
            private void Header()
            {
                // Ignore duplicit header.
                if (HasHeader) return;

                HasHeader = true;

                string[] fields = Strings[0].TrimEnd().Split('\n');
                foreach (string field in fields)
                {
                    if (field.StartsWith("Plural-Forms:"))
                    {
                        string value = field.Replace(" ", "").Substring(13);
                        if (value.Length == 0) // No value.
                            return;

                        string[] assignments = value.ToLowerInvariant().TrimEnd(';').Split(';');
                        foreach (string assignment in assignments)
                        {
                            if (assignment.StartsWith("nplurals="))
                            {
                                if (!uint.TryParse(assignment.Substring(9), out NPlurals))
                                {
                                    // Invalid nplurals value.
                                    Plural = null;
                                    return;
                                }
                            }
                            else if (assignment.StartsWith("plural="))
                            {
                                try
                                {
                                    Plural = new NExpression(assignment.Substring(7));
                                }
                                catch
                                {
                                    // Invalid plural expression.
                                    NPlurals = 0;
                                    return;
                                }
                            }
                        }
                        break;
                    }
                }

                // Ignore partial Plural-Forms field (i.e. missing either nplurals or plural expression).
                if (NPlurals == 0) Plural = null;
                if (Plural == null) NPlurals = 0;
            }

            // Next keyword is being processed.
            private void Next(KeywordType type, int index = 0)
            {
                // Assign parsed string according to current keyword.
                switch (Keyword)
                {
                    case KeywordType.MsgCtxt:
                        Context = Str;
                        break;

                    case KeywordType.MsgId:
                        Id = Str;
                        break;

                    case KeywordType.MsgIdPlural:
                        IdPlural = Str;
                        break;

                    case KeywordType.MsgStr:
                        Strings[Index] = Str;
                        break;
                }

                // New string.
                Str = "";

                // Check next keyword type.
                switch (type)
                {
                    case KeywordType.MsgCtxt:
                        // Duplicit context.
                        if (Context != null)
                        {
                            IsMalformed = true;
                            return;
                        }
                        break;

                    case KeywordType.MsgId:
                        // Duplicit id.
                        if (Id != null)
                        {
                            IsMalformed = true;
                            return;
                        }
                        break;

                    case KeywordType.MsgIdPlural:
                        // Duplicit plural id or no plurals.
                        if (IdPlural != null || NPlurals == 0)
                        {
                            IsMalformed = true;
                            return;
                        }
                        break;

                    case KeywordType.MsgStr:
                        // Allocate translated strings.
                        if (Strings == null)
                        {
                            // NPlurals == 0: No plural forms defined.
                            // NPlurals == 1: Plural forms are defined, but plural form is same as singular form.
                            // NPlurals > 1: Plural forms are defined, and there are NPlurals - 1 plural forms beside singular form.
                            Strings = new string[NPlurals == 0 || IdPlural == null ? 1 : NPlurals];
                        }

                        // New index must be 1 more than Index and less than number of allocated strings.
                        if (index != Index + 1 || index >= Strings.Length)
                        {
                            IsMalformed = true;
                            return;
                        }

                        Index = index;
                        break;
                }

                Keyword = type;
            }

            // Parses line of entry.
            // Returns true if Parser contains valid entry data.
            public bool ParseLine(string line)
            {
                // Reset entry on first call after complete entry was reported.
                if (IsComplete) Reset();

                line = line.Trim();

                // Empty line.
                if (line.Length == 0)
                {
                    // If keyword is NONE, it's just empty line to ignore.
                    if (Keyword == KeywordType.None) return false;

                    return End();
                }

                // Ignore non-empty line when entry is malformed.
                if (IsMalformed) return false;

                // Ignore comment.
                if (line[0] == '#') return false;

                // Parse keyword.
                if (line.StartsWith("msgctxt"))
                {
                    Next(KeywordType.MsgCtxt);
                    line = line.Substring(7).TrimStart();
                }
                else if (line.StartsWith("msgid"))
                {
                    if (line.StartsWith("msgid_plural"))
                    {
                        Next(KeywordType.MsgIdPlural);
                        line = line.Substring(12).TrimStart();
                    }
                    else
                    {
                        Next(KeywordType.MsgId);
                        line = line.Substring(5).TrimStart();
                    }
                }
                else if (line.StartsWith("msgstr"))
                {
                    line = line.Substring(6).TrimStart();

                    uint index = 0;

                    // Parse index.
                    if (line.StartsWith("["))
                    {
                        int pos = line.IndexOf("]");
                        if (pos < 0)
                        {
                            IsMalformed = true;
                            return false;
                        }
                        else
                        {
                            string num = line.Substring(1, pos - 1);
                            if (!uint.TryParse(num, NumberStyles.Integer, CultureInfo.InvariantCulture, out index))
                            {
                                IsMalformed = true;
                                return false;
                            }
                            line = line.Substring(pos + 1).TrimStart();
                        }
                    }

                    Next(KeywordType.MsgStr, (int)index);
                }

                if (!line.StartsWith("\""))
                {
                    IsMalformed = true;
                    return false;
                }

                // Parse message.
                line = line.Substring(1); // Opening string quote.
                bool backslash = false;
                foreach (char c in line)
                {
                    if (backslash) // Escape sequence.
                    {
                        int i = "abfnrtv\"\\".IndexOf(c);
                        if (i < 0) break; // Invalid escape sequence.
                        Str += "\a\b\f\n\r\t\v\"\\"[i];
                        backslash = false;
                    }
                    else
                    {
                        if (c == '"') // Closing string quote.
                            break;
                        else if (c == '\\')
                            backslash = true;
                        else
                            Str += c;
                    }
                }
                // Invalid escape sequence or message ends with backslash.
                if (backslash) IsMalformed = true;

                return false;
            }

            // Resets entry.
            private void Reset()
            {
                IsComplete = IsMalformed = false;

                Index = -1;
                Str = "";
                Keyword = KeywordType.None;

                Context = Id = IdPlural = null;
                Strings = null;
            }
        }

        // The context separator for message key.
        private const char CONTEXT_SEPARATOR = '\x04';
        // The plural 'n' expression.
        private NExpression Expr;
        // The display language name.
        public string LanguageName;
        // The translated strings.
        private Dictionary<string, string[]> Messages;
        // The file name of language catalog messages.
        public static readonly string MessagesFilename = "Messages.txt";
        // The language (culture name).
        public string Name;
        // The number of plural forms.
        private uint NPlurals;
        // The path to catalog.
        private string Path;

        // Private constructor.
        private Catalog(string path)
        {
            Path = path;
        }

        // Adds translated message according to parser state.
        private void AddMessage(Parser parser)
        {
            // Messages with context have key in form of concatenated context and message identifier using \0x04 character as separator.
            Messages.Add(parser.Context == null ? parser.Id : parser.Context + CONTEXT_SEPARATOR + parser.Id, parser.Strings);
        }

        // Creates catalog.
        public static Catalog Create(string path, CultureInfo culture)
        {
            Catalog catalog = null;

            // Check if file exists and is readable.
            try
            {
                string file = System.IO.Path.Combine(path, MessagesFilename);
                using (FileStream stream = File.Open(file, FileMode.Open, FileAccess.Read))
                {
                    catalog = new Catalog(path);

                    catalog.LanguageName = culture.TextInfo.ToTitleCase(culture.NativeName);
                    catalog.Name = culture.Name;
                }
            }
            catch { }

            return catalog;
        }

        // Creates default catalog.
        public static Catalog CreateDefault(string path, string defaultLanguage, string defaultLanguageName)
        {
            Catalog catalog = new Catalog(path);

            catalog.LanguageName = defaultLanguageName;
            catalog.Name = defaultLanguage;

            return catalog;
        }

        // Loads translations into catalog.
        // Returns true if translations were successfuly loaded.
        public bool Load()
        {
            // Already loaded.
            if (Messages != null) return true;

            try
            {
                Parser parser = new Parser();

                string file = System.IO.Path.Combine(Path, MessagesFilename);
                using (StreamReader reader = new StreamReader(file))
                {
                    Messages = new Dictionary<string, string[]>();

                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (parser.ParseLine(line))
                            AddMessage(parser);
                    }
                    if (parser.ParseLine(""))
                        AddMessage(parser);

                    // Set plural forms.
                    NPlurals = parser.NPlurals;
                    Expr = parser.Plural;

                    return true;
                }
            }
            catch
            {
                Messages = null;
            }

            return false;
        }
        
        // Translates message.
        // Returns null if no translation was found.
        public string Message(string message, string context = null)
        {
            // Form new key based on context.
            if (context != null) message = context + CONTEXT_SEPARATOR + message;

            return Messages.ContainsKey(message) ? Messages[message][0] : null;
        }
        
        // Translates plural message.
        // Returns null if no translation was found.
        public string Plural(string message, uint n, string context = null)
        {
            // Form new key based on context.
            if (context != null) message = context + CONTEXT_SEPARATOR + message;

            // No plurals, no translation.
            if (NPlurals == 0 || !Messages.ContainsKey(message)) return null;

            string[] translations = Messages[message];

            // Evaluate 'n' into translation index.
            uint index = Expr.Eval(n);
            if (index >= translations.Length) index = 0;

            return translations[index];
        }

        // Reads all text from localized file.
        // Returns null if error occurs.
        public string ReadAllText(string filename)
        {
            try
            {
                return File.ReadAllText(System.IO.Path.Combine(Path, filename), Encoding.UTF8);
            }
            catch
            {
                return null;
            }
        }

        // Unloads translations.
        public void Unload()
        {
            if (Messages != null)
            {
                Expr = null;
                Messages.Clear();
                NPlurals = 0;

                Messages = null;
            }
        }
    }
}
