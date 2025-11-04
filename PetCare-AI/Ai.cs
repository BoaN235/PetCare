using LLama;
using LLama.Abstractions;
using LLama.Common;
using LLama.Sampling;
using System.Diagnostics;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;


namespace PetCare_AI
{
    public class AiModel : IDisposable
    {
        const string modelPath = "qwen1_5-0_5b-chat-q8_0.gguf"; // change it to your own model path.
        private ChatSession _session;
        private InferenceParams _inferenceParams;
        private ChatHistory _chatHistory;
        public ChatHistory chatHistory => _chatHistory;
        private LLamaWeights _model;
        private LLamaContext _context;

        static async Task<string> CopyModelToAppFolder()
        {
            var filePath = Path.Combine(FileSystem.AppDataDirectory, modelPath);
            if (File.Exists(filePath))
            {
                return filePath; // File already exists, no need to copy
            }
            using var stream = await FileSystem.OpenAppPackageFileAsync(modelPath);
            using var fileStream = File.Create(filePath);
            await stream.CopyToAsync(fileStream);
            await fileStream.FlushAsync();

            return filePath;
        }

        public async void InitModel()
        {
            var appDirectoryModelPath = await CopyModelToAppFolder();

            if (!File.Exists(appDirectoryModelPath))
            {
                throw new FileNotFoundException($"Model file not found at {appDirectoryModelPath}");
            }




            var parameters = new ModelParams(appDirectoryModelPath)
            {
                ContextSize = 1024, // The longest length of chat as memory.
                GpuLayerCount = 5 // How many layers to offload to GPU. Please adjust it according to your GPU memory.
            };
            this._model = LLamaWeights.LoadFromFile(parameters);
            this._context = _model.CreateContext(parameters);
            var executor = new InteractiveExecutor(_context);

            // Add chat histories as prompt to tell AI how to act.
            this._chatHistory = new ChatHistory();
            _chatHistory.AddMessage(AuthorRole.System, "Transcript of a dialog, where the User interacts with an Assistant named Bob. Bob is helpful, kind, honest, good at writing, and never fails to answer the User's requests immediately and with precision. if bob cannot answer or is confused at any point fail with a friendly error message.");
            _chatHistory.AddMessage(AuthorRole.User, "Hello, Bob.");
            _chatHistory.AddMessage(AuthorRole.Assistant, "Hello. How may I help you today?");

            this._session = new(executor, this._chatHistory);

            this._inferenceParams = new InferenceParams()
            {
                AntiPrompts = new List<string> { "User:" }, // Stop generation once antiprompts appear., "\n", "   " 

                SamplingPipeline = new DefaultSamplingPipeline()
                {
                    Temperature = 0.7f
                }

            };

            await RunModel("Hi Bob, can you introduce yourself?");
            await RunModel("Can you tell me a joke?");

        }
        async public Task<bool> RunModel(string userinput)
        {
            string userInput = userinput ?? "";
            Debug.Write(userinput);
            // Collect the assistant's full response
            string response = "";

            await foreach (var text
                in this._session.ChatAsync(
                new ChatHistory.Message(AuthorRole.User, userInput),
                this._inferenceParams))
            {
                response += text;  // build up response
                Debug.Write(text); // optional: live output
            }
            return true;
        }

        public void Dispose()
        {
            this._model.Dispose();
            this._context.Dispose();
        }

    }

}
