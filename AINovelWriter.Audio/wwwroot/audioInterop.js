
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