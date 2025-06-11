using System.Collections.ObjectModel;
using Plugin.Maui.Audio;
using System.Timers;
using OpenAI;
using System.IO;
using OpenAI.Files;

namespace CampCopilotApp;

public class ChatMessage
{
    public string Sender { get; set; }
    public string Message { get; set; }
    public string Timestamp { get; set; }
}

public partial class MainPage : ContentPage
{
    private ObservableCollection<ChatMessage> _messages;
    private readonly IAudioManager _audioManager;
    private IAudioRecorder _audioRecorder;
    private bool _isRecording;
    private string _recordedFilePath;
    private System.Timers.Timer _silenceTimer;
    private const int SILENCE_THRESHOLD = 5000; // 5 seconds in milliseconds
    private OpenAIClient _openAI;
    private string _currentRecognizedText = "";
    private const string API_KEY = "your-api-key-here";

    public MainPage()
    {
        InitializeComponent();
        InitializeOpenAI();
        InitializeAudioManager();
        InitializeMessages();
    }

    private void InitializeOpenAI()
    {
        var options = new OpenAIClientOptions { ApiKey = API_KEY };
        _openAI = new OpenAIClient(options);
    }

    private void InitializeAudioManager()
    {
        _audioManager = AudioManager.Current;
        _silenceTimer = new System.Timers.Timer(SILENCE_THRESHOLD);
        _silenceTimer.Elapsed += OnSilenceTimerElapsed;
        _silenceTimer.AutoReset = false;
    }

    private void InitializeMessages()
    {
        _messages = new ObservableCollection<ChatMessage>();
        MessagesCollection.ItemsSource = _messages;
    }

    private async void OnMicrophoneClicked(object sender, EventArgs e)
    {
        if (!_isRecording)
        {
            await StartRecording();
        }
        else
        {
            await StopRecording();
        }
    }

    private async Task StartRecording()
    {
        try
        {
            _audioRecorder = await _audioManager.CreateRecorderAsync(new AudioRecorderOptions
            {
                FilePath = Path.Combine(FileSystem.CacheDirectory, "recording.wav"),
                AudioFormat = AudioFormat.Wav
            });

            await _audioRecorder.StartAsync();
            _isRecording = true;
            _silenceTimer.Start();
            MicrophoneButton.Text = "⏹️";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to start recording: {ex.Message}", "OK");
        }
    }

    private async Task StopRecording()
    {
        if (_audioRecorder == null) return;

        try
        {
            _silenceTimer.Stop();
            _recordedFilePath = _audioRecorder.FilePath;
            await _audioRecorder.StopAsync();
            _isRecording = false;
            MicrophoneButton.Text = "🎤";

            if (File.Exists(_recordedFilePath))
            {
                var fileInfo = new FileInfo(_recordedFilePath);
                var fileStream = File.OpenRead(_recordedFilePath);
                var fileContent = new FileContent("audio/wav", fileStream, fileInfo.Name);
                var transcription = await _openAI.CreateTranscriptionAsync(fileContent, "whisper-1");

                if (!string.IsNullOrEmpty(transcription))
                {
                    MessageInput.Text = transcription.Trim();
                    await OnSendClicked(null, null);
                }

                fileStream.Dispose();
                File.Delete(_recordedFilePath);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to stop recording: {ex.Message}", "OK");
        }
        finally
        {
            _audioRecorder = null;
        }
    }

    private async void OnSilenceTimerElapsed(object sender, ElapsedEventArgs e)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (_isRecording)
            {
                await StopRecording();
            }
        });
    }

    private async void OnSendClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(MessageInput.Text)) return;

        _messages.Add(new ChatMessage
        {
            Sender = "User",
            Message = MessageInput.Text,
            Timestamp = DateTime.Now.ToString("HH:mm")
        });

        MessageInput.Text = string.Empty;
        await ScrollToLastMessage();
    }

    private async Task ScrollToLastMessage()
    {
        if (_messages.Count > 0)
        {
            await MessagesCollection.ScrollTo(_messages.Count - 1);
        }
    }
}