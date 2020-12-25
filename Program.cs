using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AdtModelsGraph
{
    class Program
    {
        static void Main(string[] args)
        {            
            Console.WriteLine("Hello World!");

            var currentDir = Directory.GetCurrentDirectory();
            var docPath = Path.Combine(currentDir, "Ontology");
            
            var models = Directory.EnumerateFiles(docPath, "*.json", SearchOption.AllDirectories);

            var sb = new StringBuilder();
            var sbProps = new StringBuilder();
            sb.AppendLine($"classDiagram");

            // Iterate through each model and paste model id and relatioinship in mermaid
            foreach (var m in models)
            {
                Console.WriteLine(m);
                var myJsonString = File.ReadAllText(m);
                var model = JsonConvert.DeserializeObject<Root>(myJsonString);
                
                if(model.extends !=null)
                {
                    if(model.extends is string){
                        sb.AppendLine($"    {GetName(model.Id)} <|-- {GetName(model.extends.ToString())}");
                    }
                    else{
                        foreach (var e in (JArray)model.extends)//(model.extends as List<string>))
                        {
                            sb.AppendLine($"    {GetName(model.Id)} <|-- {GetName(e.ToString())}");
                        }
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
                    var type = c.Type as string;
                    if(c.Type is string && c.Type.ToString().Contains("Relationship") )
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
        public class Description    {
            public string en { get; set; } 
        }

        public class DisplayName    {
            public string en { get; set; } 
        }

        public class Content    {
            [JsonProperty("@type")]
            public Object Type { get; set; } 
            public Description description { get; set; } 
            public DisplayName displayName { get; set; } 
            public string name { get; set; } 
            public string schema { get; set; } 
            public bool writable { get; set; } 
            public string target { get; set; } 
            public List<Property> properties { get; set; } 
        }

        public class Root    {
            [JsonProperty("@id")]
            public string Id { get; set; } 
            [JsonProperty("@type")]
            public string Type { get; set; } 
            public List<Content> contents { get; set; } 
            // public Description description { get; set; } 
            // public DisplayName displayName { get; set; } 
            [JsonProperty("@context")]
            public string Context { get; set; } 
            public Object extends{get; set;}
        }
    }
}
