using System.Collections.ObjectModel;
using Plugin.Maui.Audio;

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
    private IAudioPlayer _audioPlayer;
    private bool _isRecording;
    private string _recordedFilePath;

    public MainPage(IAudioManager audioManager)
    {
        InitializeComponent();
        _messages = new ObservableCollection<ChatMessage>();
        ChatMessages.ItemsSource = _messages;
        _audioManager = audioManager;
        _isRecording = false;
    }

    private async void OnSendClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(MessageInput.Text))
            return;

        // Store the message and clear input
        string userMessage = MessageInput.Text;
        MessageInput.Text = string.Empty;

        // Add user message
        MainThread.BeginInvokeOnMainThread(() =>
        {
            _messages.Add(new ChatMessage
            {
                Sender = "User",
                Message = userMessage,
                Timestamp = DateTime.Now.ToString("HH:mm")
            });
        });

        // Get system response
        string systemResponse = await GetSystemResponse(userMessage);
        
        // Add system response
        MainThread.BeginInvokeOnMainThread(() =>
        {
            _messages.Add(new ChatMessage
            {
                Sender = "System",
                Message = systemResponse,
                Timestamp = DateTime.Now.ToString("HH:mm")
            });
        });
    }

    private async void OnMicrophoneClicked(object sender, EventArgs e)
    {
        try
        {
            var status = await Permissions.RequestAsync<Permissions.Microphone>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Permission Required", "Microphone permission is required to record audio.", "OK");
                return;
            }

            if (!_isRecording)
            {
                // Start Recording
                _recordedFilePath = Path.Combine(FileSystem.CacheDirectory, "recorded_audio.wav");
                _audioRecorder = _audioManager.CreateRecorder();
                
                await _audioRecorder.StartAsync();
                _isRecording = true;
                MicrophoneButton.Text = "⏹️"; // Stop symbol
                MicrophoneButton.BackgroundColor = Colors.Red;
            }
            else
            {
                if (_audioRecorder != null)
                {
                    // Stop Recording
                    var recordedAudio = await _audioRecorder.StopAsync();
                    _isRecording = false;
                    MicrophoneButton.Text = "🎤"; // Microphone symbol
                    MicrophoneButton.BackgroundColor = Colors.Transparent;

                    // Play the recorded audio
                    if (recordedAudio != null)
                    {
                        // Get the audio stream and play it
                        var stream = recordedAudio.GetAudioStream();
                        _audioPlayer = _audioManager.CreatePlayer(stream);
                        _audioPlayer.Play();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }

    private async Task<string> GetSystemResponse(string userMessage)
    {
        // TODO: Implement actual system response logic here
        // For now, return a simple echo
        await Task.Delay(500); // Simulate processing time
        return $"Echo: {userMessage}";
    }
}