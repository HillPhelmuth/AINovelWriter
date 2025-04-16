let audioElement = null;
let base64Total = "";
export async function downloadFile(filename, data) {
	const response = new Response(data);
	const blob = await response.blob();

	const url = window.URL.createObjectURL(blob);
	const link = document.createElement("a");
	link.href = url;
	link.download = filename;

	document.body.appendChild(link);
	link.click();
	document.body.removeChild(link);
};
export function init(audioElementId, data) {
	console.log(`init audio element ${audioElementId}`);
	audioElement = document.getElementById(audioElementId);
	audioElement.src = data;
	
}

export function play(elementId) {
	audioElement = document.getElementById(elementId);
	if (audioElement) {
		audioElement.play();
	}
}

export function pause(elementId) {
	audioElement = document.getElementById(elementId);
	if (audioElement) {
		audioElement.pause();
	}
}

export function changeProgress(value, elementId) {
	audioElement = document.getElementById(elementId);
	if (audioElement) {
		audioElement.currentTime = (value / 100) * audioElement.duration;
	}
}

export function getProgress(elementId) {
	audioElement = document.getElementById(elementId);
	if (audioElement) {
		return (audioElement.currentTime / audioElement.duration) * 100;
	}
	return 0.0;
}

export function getCurrentTime(elementId) {
	audioElement = document.getElementById(elementId);
	if (audioElement) {
		const currentTime = audioElement.currentTime;
		console.log(`current time: ${currentTime}`);
		return currentTime;
	}
	return 0;
}

export function getDuration(elementId) {
	audioElement = document.getElementById(elementId);
	if (audioElement) {
		const duration = audioElement.duration;
		console.log(`duration: ${duration}`);
		return duration;
	}
	return 0;
}
let audioContext = null;
let audioQueue = [];
let isPlaying = false;

export function initialize() {
    if (!audioContext) {
        audioContext = new (window.AudioContext || window.webkitAudioContext)();
    }
}

export function playBase64Audio(base64Audio, elementId) {
    // Streaming playback via <audio> element using MediaSource
    const audioElement = document.getElementById(elementId);
    if (!audioElement) {
        console.error(`Audio element with id ${elementId} not found.`);
        return;
    }

    // Initialize MediaSource if not already attached
    if (!audioElement.mediaSource) {
        const mediaSource = new MediaSource();
        audioElement.src = URL.createObjectURL(mediaSource);
        audioElement.mediaSource = mediaSource;
        audioElement.sourceBuffer = null;
        mediaSource.addEventListener('sourceopen', () => {
            // You may need to change the mimeType to match your audio encoding
            const mimeType = 'audio/mpeg'; // PCM WAV
            try {
                audioElement.sourceBuffer = mediaSource.addSourceBuffer(mimeType);
                audioElement.sourceBuffer.mode = 'sequence';
            } catch (e) {
                console.error('Error creating SourceBuffer:', e);
            }
        });
    }

    // Wait for sourceBuffer to be ready
    const appendChunk = () => {
        if (!audioElement.sourceBuffer) {
            setTimeout(appendChunk, 50);
            return;
        }
        // Convert base64 to Uint8Array
        const binary = atob(base64Audio);
        const len = binary.length;
        const bytes = new Uint8Array(len);
        for (let i = 0; i < len; i++) {
            bytes[i] = binary.charCodeAt(i);
        }
        // Append chunk to SourceBuffer
        if (!audioElement.sourceBuffer.updating) {
            try {
                audioElement.sourceBuffer.appendBuffer(bytes);
            } catch (e) {
                console.error('Error appending buffer:', e);
            }
        } else {
            audioElement.sourceBuffer.addEventListener('updateend', function handler() {
                audioElement.sourceBuffer.removeEventListener('updateend', handler);
                appendChunk();
            });
        }
    };
    appendChunk();
}

export function clearAudio() {
    audioQueue = [];
    isPlaying = false;
}