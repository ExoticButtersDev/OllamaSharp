using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OllamaSharp.Responses
{
    public class GenerateResponse
    {
        public string Model { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Response { get; set; }
        public bool Done { get; set; }
        public List<int> Context { get; set; }
        public long TotalDuration { get; set; }
        public long LoadDuration { get; set; }
        public int PromptEvalCount { get; set; }
        public long PromptEvalDuration { get; set; }
        public int EvalCount { get; set; }
        public long EvalDuration { get; set; }
    }

    public class ChatResponse
    {
        public string Model { get; set; }
        public DateTime CreatedAt { get; set; }
        public Message Message { get; set; }
        public bool Done { get; set; }
        public string DoneReason { get; set; }
        public long TotalDuration { get; set; }
        public long LoadDuration { get; set; }
        public int PromptEvalCount { get; set; }
        public long PromptEvalDuration { get; set; }
        public int EvalCount { get; set; }
        public long EvalDuration { get; set; }
    }

    public class ModelOperationStatus
    {
        public string Status { get; set; }
        public string Digest { get; set; }
        public long? Total { get; set; }
        public long? Completed { get; set; }
    }

    public class ModelListResponse
    {
        public List<Model> Models { get; set; }
    }

    public class Model
    {
        public string Name { get; set; }
        public DateTime ModifiedAt { get; set; }
        public long Size { get; set; }
        public string Digest { get; set; }
        public ModelDetails Details { get; set; }
        public DateTime ExpiresAt { get; set; }
        public long SizeVram { get; set; }
    }

    public class ModelDetails
    {
        public string ParentModel { get; set; }
        public string Format { get; set; }
        public string Family { get; set; }
        public List<string> Families { get; set; }
        public string ParameterSize { get; set; }
        public string QuantizationLevel { get; set; }
    }

    public class ModelInfoResponse
    {
        public string Modelfile { get; set; }
        public string Parameters { get; set; }
        public string Template { get; set; }
        public ModelDetails Details { get; set; }
        public Dictionary<string, object> ModelInfo { get; set; }
    }

    public class EmbedResponse
    {
        public string Model { get; set; }
        public List<List<float>> Embeddings { get; set; }
        public long TotalDuration { get; set; }
        public long LoadDuration { get; set; }
        public int PromptEvalCount { get; set; }
    }

    public class VersionResponse
    {
        public string Version { get; set; }
    }
    
}
