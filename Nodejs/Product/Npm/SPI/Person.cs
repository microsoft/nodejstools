using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Npm.SPI{
    internal class Person : IPerson{
        private static readonly Regex RegexPerson = new Regex(
            "^"
            + "(?<name>[^<]+)"
            + "(<(?<email>[^>]+)>)?"
            + "(\\s*\\((?<url>[^\\)])\\))?"
            + "$",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public Person(dynamic source){
            var tok = source as JToken;
            switch (tok.Type){
                case JTokenType.Object:
                    Name = source.name;
                    Email = source.email;
                    Url = source.url;
                    break;

                default:
                    InitFromString(source.ToString());
                    break;
            }
        }

        public Person(string source){
            InitFromString(source);
        }

        private void InitFromString(string source){
            var matches = RegexPerson.Matches(source);
            if (matches.Count != 1){
                Name = source;
            } else{
                var match = matches[0];
                var group = match.Groups["name"];
                if (group.Success){
                    Name = group.Value;
                }

                group = match.Groups["email"];
                if (group.Success){
                    Email = group.Value;
                }

                group = match.Groups["url"];
                if (group.Success){
                    Url = group.Value;
                }
            }
        }

        public string Name { get; private set; }
        public string Email { get; private set; }
        public string Url { get; private set; }
    }
}