namespace OllamaSharp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading;
    using System.Threading.Tasks;

    public class OllamaClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            Converters = { new JsonStringEnumConverter() },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public OllamaClient(string baseAddress = "http://localhost:11434")
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(baseAddress) };
        }

        #region Response DTOs
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

        public class Message
        {
            public string Role { get; set; }
            public string Content { get; set; }
            public List<string> Images { get; set; }
            public List<ToolCall> ToolCalls { get; set; }
        }

        public class ToolCall
        {
            public FunctionCall Function { get; set; }
        }

        public class FunctionCall
        {
            public string Name { get; set; }
            public Dictionary<string, object> Arguments { get; set; }
        }
        #endregion

        #region Request DTOs
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
        #endregion

        #region Core Methods
        public async Task<GenerateResponse> GenerateCompletionAsync(GenerateRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/generate", request, _jsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await DeserializeResponse<GenerateResponse>(response, cancellationToken);
        }

        public async Task<ChatResponse> GenerateChatCompletionAsync(ChatRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/chat", request, _jsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await DeserializeResponse<ChatResponse>(response, cancellationToken);
        }

        public async Task<List<ModelOperationStatus>> CreateModelAsync(CreateModelRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/create", request, _jsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await ReadStreamedResponses<ModelOperationStatus>(response, cancellationToken);
        }

        public async Task<bool> CheckBlobExistsAsync(string digest, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.SendAsync(
                new HttpRequestMessage(HttpMethod.Head, $"/api/blobs/{digest}"),
                cancellationToken
            );
            return response.IsSuccessStatusCode;
        }

        public async Task PushBlobAsync(string digest, Stream fileStream, CancellationToken cancellationToken = default)
        {
            var content = new StreamContent(fileStream);
            var response = await _httpClient.PostAsync($"/api/blobs/{digest}", content, cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        public async Task<ModelListResponse> ListLocalModelsAsync(CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync("/api/tags", cancellationToken);
            response.EnsureSuccessStatusCode();
            return await DeserializeResponse<ModelListResponse>(response, cancellationToken);
        }

        public async Task<ModelInfoResponse> ShowModelInformationAsync(ModelInfoRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/show", request, _jsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await DeserializeResponse<ModelInfoResponse>(response, cancellationToken);
        }

        public async Task CopyModelAsync(CopyModelRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/copy", request, _jsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteModelAsync(DeleteModelRequest request, CancellationToken cancellationToken = default)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, "/api/delete")
            {
                Content = new StringContent(JsonSerializer.Serialize(request, _jsonOptions), Encoding.UTF8, "application/json")
            };
            var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<ModelOperationStatus>> PullModelAsync(PullModelRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/pull", request, _jsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await ReadStreamedResponses<ModelOperationStatus>(response, cancellationToken);
        }

        public async Task<List<ModelOperationStatus>> PushModelAsync(PushModelRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/push", request, _jsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await ReadStreamedResponses<ModelOperationStatus>(response, cancellationToken);
        }

        public async Task<EmbedResponse> GenerateEmbeddingsAsync(EmbedRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/embed", request, _jsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await DeserializeResponse<EmbedResponse>(response, cancellationToken);
        }

        public async Task<ModelListResponse> ListRunningModelsAsync(CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync("/api/ps", cancellationToken);
            response.EnsureSuccessStatusCode();
            return await DeserializeResponse<ModelListResponse>(response, cancellationToken);
        }

        public async Task<VersionResponse> GetVersionAsync(CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync("/api/version", cancellationToken);
            response.EnsureSuccessStatusCode();
            return await DeserializeResponse<VersionResponse>(response, cancellationToken);
        }
        #endregion

        #region Synchronous Methods
        public GenerateResponse GenerateCompletion(GenerateRequest request, CancellationToken cancellationToken = default)
            => GenerateCompletionAsync(request, cancellationToken).GetAwaiter().GetResult();

        public ChatResponse GenerateChatCompletion(ChatRequest request, CancellationToken cancellationToken = default)
            => GenerateChatCompletionAsync(request, cancellationToken).GetAwaiter().GetResult();

        public List<ModelOperationStatus> CreateModel(CreateModelRequest request, CancellationToken cancellationToken = default)
            => CreateModelAsync(request, cancellationToken).GetAwaiter().GetResult();

        public bool CheckBlobExists(string digest, CancellationToken cancellationToken = default)
            => CheckBlobExistsAsync(digest, cancellationToken).GetAwaiter().GetResult();

        public void PushBlob(string digest, Stream fileStream, CancellationToken cancellationToken = default)
            => PushBlobAsync(digest, fileStream, cancellationToken).GetAwaiter().GetResult();

        public ModelListResponse ListLocalModels(CancellationToken cancellationToken = default)
            => ListLocalModelsAsync(cancellationToken).GetAwaiter().GetResult();

        public ModelInfoResponse ShowModelInformation(ModelInfoRequest request, CancellationToken cancellationToken = default)
            => ShowModelInformationAsync(request, cancellationToken).GetAwaiter().GetResult();

        public void CopyModel(CopyModelRequest request, CancellationToken cancellationToken = default)
            => CopyModelAsync(request, cancellationToken).GetAwaiter().GetResult();

        public void DeleteModel(DeleteModelRequest request, CancellationToken cancellationToken = default)
            => DeleteModelAsync(request, cancellationToken).GetAwaiter().GetResult();

        public List<ModelOperationStatus> PullModel(PullModelRequest request, CancellationToken cancellationToken = default)
            => PullModelAsync(request, cancellationToken).GetAwaiter().GetResult();

        public List<ModelOperationStatus> PushModel(PushModelRequest request, CancellationToken cancellationToken = default)
            => PushModelAsync(request, cancellationToken).GetAwaiter().GetResult();

        public EmbedResponse GenerateEmbeddings(EmbedRequest request, CancellationToken cancellationToken = default)
            => GenerateEmbeddingsAsync(request, cancellationToken).GetAwaiter().GetResult();

        public ModelListResponse ListRunningModels(CancellationToken cancellationToken = default)
            => ListRunningModelsAsync(cancellationToken).GetAwaiter().GetResult();

        public VersionResponse GetVersion(CancellationToken cancellationToken = default)
            => GetVersionAsync(cancellationToken).GetAwaiter().GetResult();
        #endregion

        #region Helpers
        private async Task<T> DeserializeResponse<T>(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return await JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions, cancellationToken);
        }

        private async Task<List<T>> ReadStreamedResponses<T>(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            var results = new List<T>();
            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync(cancellationToken));

            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(cancellationToken);
                if (!string.IsNullOrEmpty(line))
                {
                    var item = JsonSerializer.Deserialize<T>(line, _jsonOptions);
                    results.Add(item);
                }
            }
            return results;
        }
        #endregion
    }
}