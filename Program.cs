using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace AdtModelsGraph
{
    class Program
    {
        static void Main(string[] args)
        {            
            Console.WriteLine("Hello World!");

            var currentDir = Directory.GetCurrentDirectory();
            var docPath = Path.Combine(currentDir, "Models");
            
            var models = Directory.EnumerateFiles(docPath, "*.json", SearchOption.AllDirectories);

            var sb = new StringBuilder();
            var sbProps = new StringBuilder();
            sb.AppendLine($"classDiagram");

            // Iterate through each model and paste model id and relatioinship in mermaid
            foreach (var m in models)
            {
                var myJsonString = File.ReadAllText(m);
                var model = JsonConvert.DeserializeObject<Root>(myJsonString);
                
                if(model.extends !=null)
                {
                    foreach (var e in model.extends)
                    {
                        sb.AppendLine($"    {GetName(model.Id)} <|-- {GetName(e)}");
                    }
                }
                else
                {
                    sb.AppendLine($"    class {GetName(model.Id)}{{");
                    sb.AppendLine("    }");
                }
            
                if(model.contents == null)
                    continue;

                foreach (var c in model.contents)
                {
                    var s = c.schema as string;
                    if(c.Type.Contains("Relationship") )
                    {
                        sbProps.AppendLine($"    {GetName(model.Id)} : +{c.name}()");
                    }
                    else if(s == null || s.Contains("{") || s.Contains(":")){ // Complex type
                        sbProps.AppendLine($"    {GetName(model.Id)} : +Object {c.name}");
                    }
                    else
                    {
                        sbProps.AppendLine($"    {GetName(model.Id)} : +{c.schema} {c.name}");
                    }   
                }
            }

            var mermaidPath = Path.Combine(currentDir, "mermaid.mmd");
            Console.WriteLine(mermaidPath);
            
            // output all to create a mermaid graph of the relationships
            var mermaid = sb.ToString() + sbProps.ToString();
            Console.WriteLine(mermaid);

            File.WriteAllText(mermaidPath, mermaid);

            Console.WriteLine($"Mermaid saved to {mermaidPath} ");
        }

        static string GetName(string id){
            var position = id.LastIndexOf(":");
            id = id.Substring(position+1);// removing the domain info from id
            return id.Replace(";", "");// removing the ; and keeping the version
        }

        public class Property    {
            public string name { get; set; } 
            [JsonProperty("@type")]
            public string Type { get; set; } 
            public string schema { get; set; } 
        }

        public class Content    {
            [JsonProperty("@type")]
            public string Type { get; set; } 
            public string name { get; set; } 
            public object schema { get; set; } 
            public bool? writable { get; set; } 
            public string displayName { get; set; } 
            public List<Property> properties { get; set; } 
        }

        public class Root    {
            [JsonProperty("@id")]
            public string Id { get; set; } 
            [JsonProperty("@type")]
            public string Type { get; set; } 
            public string displayName { get; set; } 
            [JsonProperty("@context")]
            public string Context { get; set; } 
            public List<Content> contents { get; set; }
            public List<string> extends{get; set;}
        }
    }
}
