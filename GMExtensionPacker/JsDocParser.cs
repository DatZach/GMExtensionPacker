using GMExtensionPacker.Models.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMExtensionPacker
{
    public static class JsDocParser
    {
        public static JsDoc Parse(string content)
        {
            if (content == null)
                throw new ArgumentNullException("content");

            JsDoc jsDoc = new JsDoc();

            foreach (var line in YieldLines(content))
            {
                Directive directive;
                string arguments;

                if (!IdentifyDirective(line, out directive, out arguments))
                    continue;

                switch (directive)
                {
                    case Directive.Description:
                        jsDoc.Description += arguments;
                        break;

                    case Directive.Parameter:
                        jsDoc.Parameters.Add(ParseType(arguments, true));
                        break;

                    case Directive.Return:
                        jsDoc.ReturnType = ParseType(arguments, false).Type;
                        break;

                    case Directive.Hidden:
                        jsDoc.IsHidden = true;
                        break;
                }
            }

            return jsDoc;
        }

        private static JsDoc.Parameter ParseType(string content, bool takeName)
        {
            if (content == null)
                throw new ArgumentNullException("content");

            if (content.Length == 0)
                return new JsDoc.Parameter(VariableType.None, "", "");

            VariableType gmlType = VariableType.None;
            int startBraceIdx = content.IndexOf("{", StringComparison.Ordinal);
            int endBraceIdx = content.IndexOf("}", startBraceIdx + 1, StringComparison.Ordinal);

            if (startBraceIdx != -1 && endBraceIdx != -1)
            {
                string type = content.Substring(startBraceIdx + 1, endBraceIdx - startBraceIdx - 1).ToLowerInvariant();

                VariableType scratch;
                if (Enum.TryParse(type, true, out scratch))
                    gmlType = scratch;
            }

            string name = "";
            string desc;

            int startNameIdx = Math.Max(endBraceIdx, 0);

            if (takeName)
            {
                int endNameIdx = content.IndexOfAny(new[] { ' ', '\t' }, Math.Min(startNameIdx + 2, content.Length));
                if (endNameIdx == -1)
                    endNameIdx = content.Length;

                name = content.Substring(startNameIdx + 1, endNameIdx - startNameIdx - 1).Trim();
                desc = endBraceIdx == -1 ? content : content.Substring(endNameIdx).Trim();
            }
            else
                desc = content.Substring(startNameIdx + 1).Trim();

            if (desc.Length == 0)
                desc = name;

            return new JsDoc.Parameter(gmlType, name, desc);
        }

        private static bool IdentifyDirective(string line, out Directive directive, out string arguments)
        {
            if (line == null)
                throw new ArgumentNullException(nameof(line));

            if (!line.StartsWith("@"))
            {
                directive = Directive.Description;
                arguments = line;
                return true;
            }

            var token = new string(line.TakeWhile(x => char.IsLetter(x) || x == '@').ToArray()).ToLowerInvariant();
            switch (token)
            {
                case "@desc":
                case "@description":
                    directive = Directive.Description;
                    break;

                case "@param":
                case "@arg":
                case "@argument":
                    directive = Directive.Parameter;
                    break;

                case "@returns":
                case "@return":
                    directive = Directive.Return;
                    break;

                case "@hidden":
                    directive = Directive.Hidden;
                    break;

                default:
                    directive = Directive.None;
                    arguments = null;
                    return false;
            }

            arguments = line.Substring(token.Length);
            arguments = arguments.TrimStart();

            return true;
        }

        private static IEnumerable<string> YieldLines(string content)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            using (var reader = new StringReader(content))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    int atSymbolIdx = line.IndexOf("@", StringComparison.Ordinal);
                    if (atSymbolIdx == -1)
                        continue;

                    line = line.Substring(atSymbolIdx);
                    line = line.TrimEnd();

                    yield return line;
                }
            }
        }

        private enum Directive
        {
            None,
            Description,
            Parameter,
            Return,
            Hidden
        }
    }

    public sealed class JsDoc
    {
        public string Description { get; set; }

        public string HelpString => string.Join(", ", Parameters.Select(x => x.Name));

        public bool IsHidden { get; set; }

        public VariableType ReturnType { get; set; }

        public List<VariableType> Arguments => Parameters.Select(x => x.Type).ToList();

        public int ArgumentCount => Parameters.Count;

        public List<Parameter> Parameters { get; }

        public JsDoc()
        {
            Parameters = new List<Parameter>();
        }

        public sealed class Parameter
        {
            public VariableType Type { get; }

            public string Name { get; }

            public string Description { get; }

            public Parameter(VariableType type, string name, string description)
            {
                Type = type;
                Name = name;
                Description = description;
            }
        }
    }
}
