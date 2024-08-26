using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SuperGMS.Config.RemoteJsonFile
{
    internal class JsonConfigurationFileParser : object
    {
        public JsonConfigurationFileParser()
        {
        }

        public JObject jObject { get { return _jObject; } }
        private JObject _jObject;

        public IDictionary<string, string> Parse(Stream input)
        {
            return ParseStream(input);
        }

        private IDictionary<string, string> ParseStream(Stream input)
        {
            this._data.Clear();
            this._reader = new JsonTextReader(new StreamReader(input));
            this._reader.DateParseHandling = 0;
            _jObject = JObject.Load(this._reader);
            this.VisitJObject(_jObject);
            return this._data;
        }

        private void VisitJObject(JObject jObject)
        {
            foreach (JProperty jproperty in jObject.Properties())
            {
                this.EnterContext(jproperty.Name);
                this.VisitProperty(jproperty);
                this.ExitContext();
            }
        }

        private void VisitProperty(JProperty property)
        {
            this.VisitToken(property.Value);
        }

        private void VisitToken(JToken token)
        {
            switch ((int)token.Type)
            {
                case 1:
                    this.VisitJObject(Newtonsoft.Json.Linq.Extensions.Value<JObject>(token));
                    return;
                case 2:
                    this.VisitArray(Newtonsoft.Json.Linq.Extensions.Value<JArray>(token));
                    return;
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 13:
                case 14:
                    this.VisitPrimitive(Newtonsoft.Json.Linq.Extensions.Value<JValue>(token));
                    return;
            }
            throw new FormatException($"Unsupported JSON token '{this._reader.TokenType}' was found. Path '{this._reader.Path}', line {this._reader.LineNumber} position {this._reader.LinePosition}");
        }

        private void VisitArray(JArray array)
        {
            for (int i = 0; i < array.Count; i++)
            {
                this.EnterContext(i.ToString());
                this.VisitToken(array[i]);
                this.ExitContext();
            }
        }

        private void VisitPrimitive(JValue data)
        {
            string currentPath = this._currentPath;
            if (this._data.ContainsKey(currentPath))
            {
                throw new FormatException($"A duplicate key '{currentPath}' was found.");
            }
            this._data[currentPath] = data.ToString(CultureInfo.InvariantCulture);
        }

        private void EnterContext(string context)
        {
            this._context.Push(context);
            this._currentPath = ConfigurationPath.Combine(this._context.Reverse<string>());
        }

        private void ExitContext()
        {
            this._context.Pop();
            this._currentPath = ConfigurationPath.Combine(this._context.Reverse<string>());
        }

        private readonly IDictionary<string, string> _data = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private readonly Stack<string> _context = new Stack<string>();

        private string _currentPath;

        private JsonTextReader _reader;
    }
}
