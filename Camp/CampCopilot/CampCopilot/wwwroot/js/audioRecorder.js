let mediaRecorder = null;
let audioChunks = [];
let silenceTimeout = null;
let audioContext = null;
let dotNetHelper = null;
let silenceThreshold = -50; // dB
let silenceTimer = null;
let recognition = null;

function log(message) {
    console.log(`[AudioRecorder] ${message}`);
    if (dotNetHelper) {
        dotNetHelper.invokeMethodAsync('OnRecordingError', message);
    }
}

async function requestPermission() {
    try {
        // First check if permissions are already granted
        const result = await navigator.permissions.query({ name: 'microphone' });
        if (result.state === 'granted') {
            return true;
        }

        // If not granted, request permissions
        const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
        stream.getTracks().forEach(track => track.stop()); // Stop the stream since we're just checking permissions
        return true;
    } catch (error) {
        log('Error requesting microphone permission: ' + error.message);
        return false;
    }
}

function initializeSpeechRecognition() {
    try {
        const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
        if (!SpeechRecognition) {
            throw new Error('Speech recognition not supported in this browser');
        }

        recognition = new SpeechRecognition();
        recognition.continuous = true;
        recognition.interimResults = true;
        recognition.lang = 'en-US';

        recognition.onresult = (event) => {
            const transcript = Array.from(event.results)
                .map(result => result[0].transcript)
                .join(' ');
            
            if (event.results[event.results.length - 1].isFinal) {
                log('Final transcript: ' + transcript);
                if (dotNetHelper) {
                    dotNetHelper.invokeMethodAsync('OnTranscriptionReceived', transcript);
                }
            }
        };

        recognition.onerror = (event) => {
            log('Speech recognition error: ' + event.error);
        };

        return true;
    } catch (error) {
        log('Error initializing speech recognition: ' + error.message);
        return false;
    }
}

export async function startRecording(helper, silenceTimeoutMs) {
    try {
        log('Starting recording setup...');
        
        if (!helper) {
            throw new Error('DotNet helper is null');
        }
        
        dotNetHelper = helper;

        // Check if the API is available
        if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
            throw new Error('MediaDevices API not supported. Please ensure you are using a supported browser and have granted the necessary permissions.');
        }

        // Request permission first
        const hasPermission = await requestPermission();
        if (!hasPermission) {
            throw new Error('Microphone permission was denied');
        }

        // Initialize speech recognition
        if (!initializeSpeechRecognition()) {
            throw new Error('Failed to initialize speech recognition');
        }

        // Now proceed with recording
        const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
        log('Microphone access granted');
        
        mediaRecorder = new MediaRecorder(stream);
        audioChunks = [];
        
        // Set up audio analysis for silence detection
        audioContext = new AudioContext();
        log('AudioContext created: ' + audioContext.state);
        
        const source = audioContext.createMediaStreamSource(stream);
        const analyser = audioContext.createAnalyser();
        analyser.fftSize = 2048;
        source.connect(analyser);
        log('Audio analysis setup complete');
        
        const bufferLength = analyser.frequencyBinCount;
        const dataArray = new Float32Array(bufferLength);
        
        // Check for silence every 100ms
        silenceTimer = setInterval(() => {
            try {
                analyser.getFloatTimeDomainData(dataArray);
                let maxVolume = -Infinity;
                for (let i = 0; i < bufferLength; i++) {
                    const volume = 20 * Math.log10(Math.abs(dataArray[i]));
                    maxVolume = Math.max(maxVolume, volume);
                }
                
                if (maxVolume < silenceThreshold) {
                    if (!silenceTimeout) {
                        log('Silence detected, starting timeout');
                        silenceTimeout = setTimeout(() => {
                            log('Silence timeout reached');
                            dotNetHelper.invokeMethodAsync('OnSilenceDetected');
                            stopRecording();
                        }, silenceTimeoutMs);
                    }
                } else {
                    if (silenceTimeout) {
                        log('Sound detected, clearing silence timeout');
                        clearTimeout(silenceTimeout);
                        silenceTimeout = null;
                    }
                }
            } catch (error) {
                log('Error in silence detection: ' + error.message);
            }
        }, 100);

        mediaRecorder.ondataavailable = event => {
            log('Received audio chunk: ' + event.data.size + ' bytes');
            audioChunks.push(event.data);
        };

        mediaRecorder.onstop = async () => {
            try {
                log('Recording stopped, processing audio...');
                // Stop the speech recognition
                if (recognition) {
                    recognition.stop();
                }
                
                // Clean up
                log('Cleaning up resources...');
                stream.getTracks().forEach(track => track.stop());
                if (silenceTimer) {
                    clearInterval(silenceTimer);
                    silenceTimer = null;
                }
                if (silenceTimeout) {
                    clearTimeout(silenceTimeout);
                    silenceTimeout = null;
                }
                if (audioContext) {
                    await audioContext.close();
                    audioContext = null;
                }
                log('Cleanup complete');
            } catch (error) {
                log('Error in stop handler: ' + error.message);
            }
        };

        mediaRecorder.onerror = (event) => {
            log('MediaRecorder error: ' + event.error.message);
        };

        log('Starting MediaRecorder and Speech Recognition...');
        mediaRecorder.start();
        recognition.start();
        log('Recording started');
    } catch (error) {
        log('Critical error in startRecording: ' + error.message);
        cleanup();
    }
}

export function stopRecording() {
    try {
        log('Stopping recording...');
        if (mediaRecorder && mediaRecorder.state !== 'inactive') {
            mediaRecorder.stop();
            log('MediaRecorder stopped');
        } else {
            log('MediaRecorder not active');
        }
        
        if (recognition) {
            recognition.stop();
            log('Speech recognition stopped');
        }
    } catch (error) {
        log('Error stopping recording: ' + error.message);
    }
}

function cleanup() {
    try {
        if (silenceTimer) {
            clearInterval(silenceTimer);
            silenceTimer = null;
        }
        if (silenceTimeout) {
            clearTimeout(silenceTimeout);
            silenceTimeout = null;
        }
        if (audioContext) {
            audioContext.close();
            audioContext = null;
        }
        if (mediaRecorder && mediaRecorder.stream) {
            mediaRecorder.stream.getTracks().forEach(track => track.stop());
        }
        if (recognition) {
            recognition.stop();
            recognition = null;
        }
    } catch (error) {
        log('Error in cleanup: ' + error.message);
    }
} 