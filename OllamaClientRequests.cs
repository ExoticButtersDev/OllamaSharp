using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OllamaSharp.Request
{
    public class GenerateRequest
    {
        public string Model { get; set; }
        public string Prompt { get; set; }
        public string Suffix { get; set; }
        public List<string> Images { get; set; }
        public object Format { get; set; }
        public Dictionary<string, object> Options { get; set; }
        public string System { get; set; }
        public string Template { get; set; }
        public bool? Stream { get; set; }
        public bool? Raw { get; set; }
        public string KeepAlive { get; set; }
        public List<int> Context { get; set; }
    }

    public class ChatRequest
    {
        public string Model { get; set; }
        public List<Message> Messages { get; set; }
        public List<Tool> Tools { get; set; }
        public object Format { get; set; }
        public Dictionary<string, object> Options { get; set; }
        public bool? Stream { get; set; }
        public string KeepAlive { get; set; }
    }

    public class Tool
    {
        public string Type { get; set; }
        public FunctionDefinition Function { get; set; }
    }

    public class FunctionDefinition
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public object Parameters { get; set; }
    }

    public class CreateModelRequest
    {
        public string Model { get; set; }
        public string From { get; set; }
        public Dictionary<string, string> Files { get; set; }
        public Dictionary<string, string> Adapters { get; set; }
        public string Template { get; set; }
        public object License { get; set; }
        public string System { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public List<Message> Messages { get; set; }
        public bool? Stream { get; set; }
        public string Quantize { get; set; }
    }

    public class ModelInfoRequest
    {
        public string Model { get; set; }
        public bool? Verbose { get; set; }
    }

    public class CopyModelRequest
    {
        public string Source { get; set; }
        public string Destination { get; set; }
    }

    public class DeleteModelRequest
    {
        public string Model { get; set; }
    }

    public class PullModelRequest
    {
        public string Model { get; set; }
        public bool? Insecure { get; set; }
        public bool? Stream { get; set; }
    }

    public class PushModelRequest
    {
        public string Model { get; set; }
        public bool? Insecure { get; set; }
        public bool? Stream { get; set; }
    }

    public class EmbedRequest
    {
        public string Model { get; set; }
        public object Input { get; set; }
        public bool? Truncate { get; set; }
        public Dictionary<string, object> Options { get; set; }
        public string KeepAlive { get; set; }
    }

}
