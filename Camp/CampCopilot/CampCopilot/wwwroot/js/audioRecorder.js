let mediaRecorder = null;
let audioChunks = [];
let silenceTimer = null;
let audioContext = null;
let dotNetHelper = null;
let silenceThreshold = -45; // Adjusted from -50 to -45 dB to be less sensitive
let isRecording = false;
let recordingStartTime = null;
let minRecordingDuration = 1000; // Minimum recording duration in milliseconds
let maxSilenceDuration = 1500; // Maximum silence duration before stopping

function log(message, isError = false) {
    const timestamp = new Date().toISOString();
    console.log(`[AudioRecorder ${timestamp}] ${message}`);
    if (isError && dotNetHelper) {
        dotNetHelper.invokeMethodAsync('OnRecordingError', message);
    }
}

function getSupportedMimeType() {
    const types = [
        'audio/webm;codecs=opus',
        'audio/webm',
        'audio/ogg;codecs=opus',
        'audio/ogg',
        'audio/mp4',
        'audio/mpeg',
        'audio/wav',
        ''  // Empty string means let the browser choose
    ];
    
    log('Checking supported MIME types...');
    for (const type of types) {
        try {
            if (type === '' || MediaRecorder.isTypeSupported(type)) {
                log(`Found supported MIME type: ${type || 'browser default'}`);
                return type;
            }
        } catch (error) {
            log(`Error checking MIME type ${type}: ${error.message}`);
        }
    }
    throw new Error('No supported audio MIME type found. Available types: ' + 
        types.filter(t => t).map(t => `${t}=${MediaRecorder.isTypeSupported(t)}`).join(', '));
}

async function convertToWav(audioBlob) {
    try {
        log('Converting audio to WAV format');
        const audioData = await audioBlob.arrayBuffer();
        const audioContext = new AudioContext();
        const audioBuffer = await audioContext.decodeAudioData(audioData);
        
        // Create WAV file
        const numberOfChannels = 1; // Mono
        const sampleRate = 16000; // Required by Whisper
        const length = audioBuffer.length;
        const wavBuffer = new ArrayBuffer(44 + length * 2);
        const view = new DataView(wavBuffer);
        
        // WAV header
        const writeString = (view, offset, string) => {
            for (let i = 0; i < string.length; i++) {
                view.setUint8(offset + i, string.charCodeAt(i));
            }
        };
        
        writeString(view, 0, 'RIFF');  // RIFF identifier
        view.setUint32(4, 36 + length * 2, true);  // file length
        writeString(view, 8, 'WAVE');  // WAVE identifier
        writeString(view, 12, 'fmt ');  // fmt chunk
        view.setUint32(16, 16, true);  // length of fmt chunk
        view.setUint16(20, 1, true);  // PCM format
        view.setUint16(22, numberOfChannels, true);  // channels
        view.setUint32(24, sampleRate, true);  // sample rate
        view.setUint32(28, sampleRate * 2, true);  // byte rate
        view.setUint16(32, numberOfChannels * 2, true);  // block align
        view.setUint16(34, 16, true);  // bits per sample
        writeString(view, 36, 'data');  // data chunk
        view.setUint32(40, length * 2, true);  // data length
        
        // Write audio data
        const samples = new Float32Array(length);
        audioBuffer.copyFromChannel(samples, 0);
        let offset = 44;
        for (let i = 0; i < length; i++) {
            const sample = Math.max(-1, Math.min(1, samples[i]));
            view.setInt16(offset, sample < 0 ? sample * 0x8000 : sample * 0x7FFF, true);
            offset += 2;
        }
        
        return new Blob([wavBuffer], { type: 'audio/wav' });
    } catch (error) {
        log('Error converting audio: ' + error.message, true);
        throw error;
    }
}

async function processAudioChunks() {
    try {
        if (audioChunks.length === 0) {
            log('No audio chunks collected');
            return;
        }
        
        log(`Processing ${audioChunks.length} audio chunks`);
        const audioBlob = new Blob(audioChunks);
        log(`Audio blob created: ${audioBlob.size} bytes, type: ${audioBlob.type}`);
        
        // Convert to WAV
        const wavBlob = await convertToWav(audioBlob);
        log(`WAV blob created: ${wavBlob.size} bytes, type: ${wavBlob.type}`);
        
        // Generate filename
        const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
        const filename = `recording-${timestamp}.wav`;
        
        // Convert to base64 for both saving and transcription
        const reader = new FileReader();
        reader.readAsDataURL(wavBlob);
        
        reader.onloadend = async () => {
            try {
                log('Audio converted to base64');
                // Get base64 data without the prefix
                const base64Data = reader.result.split(',')[1];
                
                if (dotNetHelper) {
                    // Save the file
                    log(`Saving audio file: ${filename}`);
                    await dotNetHelper.invokeMethodAsync('SaveAudioFile', base64Data, filename);
                    
                    // Send for transcription
                    log('Sending audio data for transcription');
                    await dotNetHelper.invokeMethodAsync('TranscribeAudio', base64Data);
                    log('Audio data sent successfully');
                } else {
                    log('dotNetHelper is null, cannot process audio', true);
                }
            } catch (error) {
                log('Error processing audio data: ' + error.message, true);
            }
        };

        reader.onerror = () => {
            log('Error reading audio blob: ' + reader.error, true);
        };
    } catch (error) {
        log('Error processing audio: ' + error.message, true);
    }
}

export async function startRecording(helper, silenceTimeoutMs) {
    try {
        if (!helper) {
            throw new Error('DotNet helper is null');
        }
        
        log('Starting recording setup...');
        dotNetHelper = helper;
        isRecording = true;
        audioChunks = [];
        recordingStartTime = Date.now();

        if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
            throw new Error('MediaDevices API not supported');
        }

        const stream = await navigator.mediaDevices.getUserMedia({ 
            audio: {
                channelCount: 1,
                sampleRate: 16000,
                sampleSize: 16,
                volume: 1,
                noiseSuppression: true,
                echoCancellation: true
            }
        });
        log('Microphone access granted');
        
        const mimeType = getSupportedMimeType();
        const options = {
            audioBitsPerSecond: 16000
        };
        
        if (mimeType) {
            options.mimeType = mimeType;
        }
        
        try {
            mediaRecorder = new MediaRecorder(stream, options);
            log('MediaRecorder created successfully');
        } catch (error) {
            log(`Failed to create MediaRecorder with options, trying default settings: ${error.message}`);
            mediaRecorder = new MediaRecorder(stream);
            log('MediaRecorder created with default settings');
        }
        
        audioContext = new AudioContext();
        const source = audioContext.createMediaStreamSource(stream);
        const analyser = audioContext.createAnalyser();
        analyser.fftSize = 2048;
        source.connect(analyser);
        log('Audio context and analyser set up');
        
        const bufferLength = analyser.frequencyBinCount;
        const dataArray = new Float32Array(bufferLength);
        
        let silenceStart = null;
        let consecutiveSilenceCount = 0;
        
        silenceTimer = setInterval(() => {
            if (!isRecording) return;

            try {
                const currentTime = Date.now();
                const recordingDuration = currentTime - recordingStartTime;
                
                analyser.getFloatTimeDomainData(dataArray);
                let maxVolume = -Infinity;
                for (let i = 0; i < bufferLength; i++) {
                    const volume = 20 * Math.log10(Math.abs(dataArray[i]));
                    maxVolume = Math.max(maxVolume, volume);
                }
                
                log(`Current volume: ${maxVolume.toFixed(2)} dB`);
                
                if (maxVolume < silenceThreshold) {
                    if (!silenceStart) {
                        silenceStart = currentTime;
                        log('Silence started');
                    } else {
                        const silenceDuration = currentTime - silenceStart;
                        if (recordingDuration >= minRecordingDuration && silenceDuration >= maxSilenceDuration) {
                            log(`Stopping recording after ${silenceDuration}ms of silence`);
                            if (dotNetHelper) {
                                dotNetHelper.invokeMethodAsync('OnSilenceDetected');
                            }
                            stopRecording();
                        }
                    }
                    consecutiveSilenceCount++;
                } else {
                    if (silenceStart) {
                        log('Sound detected, resetting silence timer');
                    }
                    silenceStart = null;
                    consecutiveSilenceCount = 0;
                }
            } catch (error) {
                log('Error in silence detection: ' + error.message, true);
            }
        }, 100);

        mediaRecorder.ondataavailable = event => {
            if (event.data.size > 0) {
                audioChunks.push(event.data);
                log(`Audio chunk collected: ${event.data.size} bytes`);
            }
        };

        mediaRecorder.onstop = async () => {
            try {
                log('MediaRecorder stopped, cleaning up...');
                isRecording = false;
                
                stream.getTracks().forEach(track => track.stop());
                if (silenceTimer) {
                    clearInterval(silenceTimer);
                    silenceTimer = null;
                }
                if (audioContext) {
                    await audioContext.close();
                    audioContext = null;
                }
                
                const recordingDuration = Date.now() - recordingStartTime;
                log(`Total recording duration: ${recordingDuration}ms`);
                
                if (recordingDuration < minRecordingDuration) {
                    log('Recording too short, discarding');
                    return;
                }
                
                log('Processing collected audio chunks...');
                await processAudioChunks();
            } catch (error) {
                log('Error in stop handler: ' + error.message, true);
            }
        };

        mediaRecorder.onerror = (event) => {
            log('MediaRecorder error: ' + event.error.message, true);
        };

        // Start recording
        mediaRecorder.start(1000); // Collect data every second
        log('Recording started');
    } catch (error) {
        log('Critical error in startRecording: ' + error.message, true);
        cleanup();
    }
}

export function stopRecording() {
    try {
        if (!isRecording) {
            log('Stop recording called but not recording');
            return;
        }
        
        isRecording = false;
        log('Stopping recording');
        
        if (mediaRecorder && mediaRecorder.state !== 'inactive') {
            mediaRecorder.stop();
            log('MediaRecorder stop called');
        } else {
            log('MediaRecorder not active', true);
        }
    } catch (error) {
        log('Error stopping recording: ' + error.message, true);
    }
}

function cleanup() {
    try {
        isRecording = false;
        
        if (silenceTimer) {
            clearInterval(silenceTimer);
            silenceTimer = null;
        }
        if (audioContext) {
            audioContext.close();
            audioContext = null;
        }
        if (mediaRecorder && mediaRecorder.stream) {
            mediaRecorder.stream.getTracks().forEach(track => track.stop());
        }
        log('Cleanup completed');
    } catch (error) {
        log('Error in cleanup: ' + error.message, true);
    }
} 