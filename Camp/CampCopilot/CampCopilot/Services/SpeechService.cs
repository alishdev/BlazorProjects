using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace CampCopilot.Services;

public interface ISpeechService
{
    Task<string> TranscribeAudioAsync(byte[] audioData);
}

public class SpeechService : ISpeechService
{
    private readonly ILogger<SpeechService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IAppConfig _config;
    private const string OPENAI_API_URL = "https://api.openai.com/v1/audio/transcriptions";
    
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public SpeechService(ILogger<SpeechService> logger, IHttpClientFactory httpClientFactory, IAppConfig config)
    {
        _logger = logger;
        _config = config;
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.OpenAIApiKey}");
    }

    public async Task<string> TranscribeAudioAsync(byte[] audioData)
    {
        try
        {
            _logger.LogInformation("Starting audio transcription");
            _logger.LogInformation($"Received audio data: {audioData.Length} bytes");
            
            if (audioData == null || audioData.Length == 0)
            {
                _logger.LogWarning("Received empty audio data");
                return "No audio data received";
            }

            // Verify WAV header
            if (audioData.Length >= 44)
            {
                var riffHeader = System.Text.Encoding.ASCII.GetString(audioData, 0, 4);
                var waveHeader = System.Text.Encoding.ASCII.GetString(audioData, 8, 4);
                var format = BitConverter.ToUInt16(audioData, 20);
                var channels = BitConverter.ToUInt16(audioData, 22);
                var sampleRate = BitConverter.ToUInt32(audioData, 24);
                var bitsPerSample = BitConverter.ToUInt16(audioData, 34);
                
                _logger.LogInformation($"WAV Headers - RIFF: {riffHeader}, WAVE: {waveHeader}");
                _logger.LogInformation($"Format: {format}, Channels: {channels}, Sample Rate: {sampleRate}, Bits per Sample: {bitsPerSample}");
            }
            else
            {
                _logger.LogWarning("Audio data too short to contain WAV headers");
            }

            using var content = new MultipartFormDataContent();
            
            // Add the audio file
            var audioContent = new ByteArrayContent(audioData);
            audioContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("audio/wav");
            content.Add(audioContent, "file", "audio.wav");
            
            // Add model parameter
            content.Add(new StringContent("whisper-1"), "model");
            
            // Optional parameters
            content.Add(new StringContent("en"), "language"); // Specify English
            content.Add(new StringContent("json"), "response_format"); // Get JSON response

            _logger.LogInformation("Sending request to OpenAI API");
            var response = await _httpClient.PostAsync(OPENAI_API_URL, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"OpenAI API error: {errorContent}");
                throw new Exception($"OpenAI API error: {response.StatusCode} - {errorContent}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"Received response from OpenAI: {jsonResponse}");
            
            var result = JsonSerializer.Deserialize<WhisperResponse>(jsonResponse, _jsonOptions);
            
            if (result == null)
            {
                _logger.LogWarning("Failed to deserialize OpenAI response");
                return "Failed to process transcription response";
            }
            
            _logger.LogInformation($"Transcription completed successfully: '{result.Text}'");
            return result.Text ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transcribing audio");
            throw;
        }
    }

    private class WhisperResponse
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }
} 