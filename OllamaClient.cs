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

    using OllamaSharp.Request;
    using OllamaSharp.Responses;

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

        #region Core Methods

        /// <summary>
        /// Generates a text completion for the given prompt using the specified model asynchronously
        /// </summary>
        /// <param name="request">Request parameters including model name, prompt, and generation options</param>
        /// <param name="cancellationToken">Cancellation token to abort the operation</param>
        /// <returns>GenerateResponse containing the generated text and performance metrics</returns>
        /// <exception cref="HttpRequestException">Thrown when the API request fails</exception>
        public async Task<GenerateResponse> GenerateCompletionAsync(GenerateRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/generate", request, _jsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await DeserializeResponse<GenerateResponse>(response, cancellationToken);
        }

        /// <summary>
        /// Generates a chat completion for a conversation using the specified model asynchronously
        /// </summary>
        /// <param name="request">Chat request containing message history and model parameters</param>
        /// <param name="cancellationToken">Cancellation token to abort the operation</param>
        /// <returns>ChatResponse with the assistant's reply and conversation metadata</returns>
        /// <exception cref="HttpRequestException">Thrown when the API request fails</exception>
        public async Task<ChatResponse> GenerateChatCompletionAsync(ChatRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/chat", request, _jsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await DeserializeResponse<ChatResponse>(response, cancellationToken);
        }

        /// <summary>
        /// Creates a new model from existing models, GGUF files, or safetensors directories asynchronously
        /// </summary>
        /// <param name="request">Model creation parameters including source files and configuration</param>
        /// <param name="cancellationToken">Cancellation token to abort the operation</param>
        /// <returns>List of status updates during model creation process</returns>
        /// <exception cref="HttpRequestException">Thrown when the API request fails</exception>
        public async Task<List<ModelOperationStatus>> CreateModelAsync(CreateModelRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/create", request, _jsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await ReadStreamedResponses<ModelOperationStatus>(response, cancellationToken);
        }

        /// <summary>
        /// Checks if a specific blob exists on the Ollama server asynchronously
        /// </summary>
        /// <param name="digest">SHA256 digest of the blob to check</param>
        /// <param name="cancellationToken">Cancellation token to abort the operation</param>
        /// <returns>True if the blob exists, false otherwise</returns>
        public async Task<bool> CheckBlobExistsAsync(string digest, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.SendAsync(
                new HttpRequestMessage(HttpMethod.Head, $"/api/blobs/{digest}"),
                cancellationToken
            );
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Uploads a blob file to the Ollama server asynchronously
        /// </summary>
        /// <param name="digest">Expected SHA256 digest of the blob content</param>
        /// <param name="fileStream">Stream containing the blob data</param>
        /// <param name="cancellationToken">Cancellation token to abort the operation</param>
        /// <exception cref="HttpRequestException">Thrown when the upload fails</exception>
        public async Task PushBlobAsync(string digest, Stream fileStream, CancellationToken cancellationToken = default)
        {
            var content = new StreamContent(fileStream);
            var response = await _httpClient.PostAsync($"/api/blobs/{digest}", content, cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Retrieves list of locally available models asynchronously
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to abort the operation</param>
        /// <returns>ModelListResponse containing metadata about available models</returns>
        /// <exception cref="HttpRequestException">Thrown when the API request fails</exception>
        public async Task<ModelListResponse> ListLocalModelsAsync(CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync("/api/tags", cancellationToken);
            response.EnsureSuccessStatusCode();
            return await DeserializeResponse<ModelListResponse>(response, cancellationToken);
        }

        /// <summary>
        /// Gets detailed information about a specific model asynchronously
        /// </summary>
        /// <param name="request">Model information request parameters</param>
        /// <param name="cancellationToken">Cancellation token to abort the operation</param>
        /// <returns>ModelInfoResponse with technical details and configuration</returns>
        /// <exception cref="HttpRequestException">Thrown when the API request fails</exception>
        public async Task<ModelInfoResponse> ShowModelInformationAsync(ModelInfoRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/show", request, _jsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await DeserializeResponse<ModelInfoResponse>(response, cancellationToken);
        }

        /// <summary>
        /// Creates a copy of an existing model with a new name asynchronously
        /// </summary>
        /// <param name="request">Copy operation parameters including source and destination names</param>
        /// <param name="cancellationToken">Cancellation token to abort the operation</param>
        /// <exception cref="HttpRequestException">Thrown when the copy operation fails</exception>
        public async Task CopyModelAsync(CopyModelRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/copy", request, _jsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Permanently deletes a model from the local storage asynchronously
        /// </summary>
        /// <param name="request">Delete request specifying model to remove</param>
        /// <param name="cancellationToken">Cancellation token to abort the operation</param>
        /// <exception cref="HttpRequestException">Thrown when the deletion fails</exception>
        public async Task DeleteModelAsync(DeleteModelRequest request, CancellationToken cancellationToken = default)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, "/api/delete")
            {
                Content = new StringContent(JsonSerializer.Serialize(request, _jsonOptions), Encoding.UTF8, "application/json")
            };
            var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Downloads a model from the Ollama library asynchronously
        /// </summary>
        /// <param name="request">Pull request parameters including model name</param>
        /// <param name="cancellationToken">Cancellation token to abort the operation</param>
        /// <returns>List of progress updates during the download process</returns>
        /// <exception cref="HttpRequestException">Thrown when the download fails</exception>
        public async Task<List<ModelOperationStatus>> PullModelAsync(PullModelRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/pull", request, _jsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await ReadStreamedResponses<ModelOperationStatus>(response, cancellationToken);
        }

        /// <summary>
        /// Uploads a model to a model library asynchronously
        /// </summary>
        /// <param name="request">Push request parameters including model name</param>
        /// <param name="cancellationToken">Cancellation token to abort the operation</param>
        /// <returns>List of progress updates during the upload process</returns>
        /// <exception cref="HttpRequestException">Thrown when the upload fails</exception>
        public async Task<List<ModelOperationStatus>> PushModelAsync(PushModelRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/push", request, _jsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await ReadStreamedResponses<ModelOperationStatus>(response, cancellationToken);
        }

        /// <summary>
        /// Generates vector embeddings for input text using the specified model asynchronously
        /// </summary>
        /// <param name="request">Embedding request parameters</param>
        /// <param name="cancellationToken">Cancellation token to abort the operation</param>
        /// <returns>EmbedResponse containing generated embeddings and performance data</returns>
        /// <exception cref="HttpRequestException">Thrown when the API request fails</exception>
        public async Task<EmbedResponse> GenerateEmbeddingsAsync(EmbedRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/embed", request, _jsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await DeserializeResponse<EmbedResponse>(response, cancellationToken);
        }

        /// <summary>
        /// Lists currently loaded/running models asynchronously
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to abort the operation</param>
        /// <returns>ModelListResponse with metadata about active models</returns>
        /// <exception cref="HttpRequestException">Thrown when the API request fails</exception>
        public async Task<ModelListResponse> ListRunningModelsAsync(CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync("/api/ps", cancellationToken);
            response.EnsureSuccessStatusCode();
            return await DeserializeResponse<ModelListResponse>(response, cancellationToken);
        }

        /// <summary>
        /// Retrieves the Ollama server version information asynchronously
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to abort the operation</param>
        /// <returns>VersionResponse containing the server version string</returns>
        /// <exception cref="HttpRequestException">Thrown when the API request fails</exception>
        public async Task<VersionResponse> GetVersionAsync(CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync("/api/version", cancellationToken);
            response.EnsureSuccessStatusCode();
            return await DeserializeResponse<VersionResponse>(response, cancellationToken);
        }
        #endregion

        #region Synchronous Methods

        /// <summary>
        /// Generates a text completion for the given prompt using the specified model
        /// </summary>
        /// <param name="request">Request parameters including model name, prompt, and generation options</param>
        /// <param name="cancellationToken">Cancellation token to abort the operation</param>
        /// <returns>GenerateResponse containing the generated text and performance metrics</returns>
        /// <exception cref="HttpRequestException">Thrown when the API request fails</exception>
        public GenerateResponse GenerateCompletion(GenerateRequest request, CancellationToken cancellationToken = default)
            => GenerateCompletionAsync(request, cancellationToken).GetAwaiter().GetResult();

        /// <summary>
        /// Generates a chat completion for a conversation using the specified model
        /// </summary>
        /// <param name="request">Chat request containing message history and model parameters</param>
        /// <param name="cancellationToken">Cancellation token to abort the operation</param>
        /// <returns>ChatResponse with the assistant's reply and conversation metadata</returns>
        /// <exception cref="HttpRequestException">Thrown when the API request fails</exception>
        public ChatResponse GenerateChatCompletion(ChatRequest request, CancellationToken cancellationToken = default)
            => GenerateChatCompletionAsync(request, cancellationToken).GetAwaiter().GetResult();

        /// <summary>
        /// Creates a new model from existing models, GGUF files, or safetensors directories
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public List<ModelOperationStatus> CreateModel(CreateModelRequest request, CancellationToken cancellationToken = default)
            => CreateModelAsync(request, cancellationToken).GetAwaiter().GetResult();

        /// <summary>
        /// Checks if a specific blob exists on the Ollama server
        /// </summary>
        /// <param name="digest"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public bool CheckBlobExists(string digest, CancellationToken cancellationToken = default)
            => CheckBlobExistsAsync(digest, cancellationToken).GetAwaiter().GetResult();

        /// <summary>
        /// Uploads a blob file to the Ollama server
        /// </summary>
        /// <param name="digest"></param>
        /// <param name="fileStream"></param>
        /// <param name="cancellationToken"></param>
        public void PushBlob(string digest, Stream fileStream, CancellationToken cancellationToken = default)
            => PushBlobAsync(digest, fileStream, cancellationToken).GetAwaiter().GetResult();

        /// <summary>
        /// Retrieves list of locally available models
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public ModelListResponse ListLocalModels(CancellationToken cancellationToken = default)
            => ListLocalModelsAsync(cancellationToken).GetAwaiter().GetResult();

        /// <summary>
        /// Gets detailed information about a specific model
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public ModelInfoResponse ShowModelInformation(ModelInfoRequest request, CancellationToken cancellationToken = default)
            => ShowModelInformationAsync(request, cancellationToken).GetAwaiter().GetResult();

        /// <summary>
        /// Creates a copy of an existing model with a new name
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        public void CopyModel(CopyModelRequest request, CancellationToken cancellationToken = default)
            => CopyModelAsync(request, cancellationToken).GetAwaiter().GetResult();

        /// <summary>
        /// Permanently deletes a model from the local storage
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        public void DeleteModel(DeleteModelRequest request, CancellationToken cancellationToken = default)
            => DeleteModelAsync(request, cancellationToken).GetAwaiter().GetResult();

        /// <summary>
        /// Downloads a model from the Ollama library
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public List<ModelOperationStatus> PullModel(PullModelRequest request, CancellationToken cancellationToken = default)
            => PullModelAsync(request, cancellationToken).GetAwaiter().GetResult();

        /// <summary>
        /// Uploads a model to a model library
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public List<ModelOperationStatus> PushModel(PushModelRequest request, CancellationToken cancellationToken = default)
            => PushModelAsync(request, cancellationToken).GetAwaiter().GetResult();

        /// <summary>
        /// Generates vector embeddings for input text using the specified model
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public EmbedResponse GenerateEmbeddings(EmbedRequest request, CancellationToken cancellationToken = default)
            => GenerateEmbeddingsAsync(request, cancellationToken).GetAwaiter().GetResult();

        /// <summary>
        /// Lists currently loaded/running models
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public ModelListResponse ListRunningModels(CancellationToken cancellationToken = default)
            => ListRunningModelsAsync(cancellationToken).GetAwaiter().GetResult();

        /// <summary>
        /// Retrieves the Ollama server version information
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
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