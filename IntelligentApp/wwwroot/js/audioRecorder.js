window.audioRecorder = {
    mediaRecorder: null,
    chunks: [],
    isRecording: false,

    startRecording: async function () {
        if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
            console.log("Media Devices not supported");
            return;
        }
        let stream = await navigator.mediaDevices.getUserMedia({ audio: true });
        this.mediaRecorder = new MediaRecorder(stream);
        this.chunks = [];
        this.mediaRecorder.ondataavailable = (e) => {
            this.chunks.push(e.data);
        };
        this.mediaRecorder.start();
        this.isRecording = true;
    },

    stopRecording: async function () {
        if (this.mediaRecorder && this.isRecording) {
            return new Promise((resolve, reject) => {
                this.mediaRecorder.onstop = async (e) => {
                    let blob = new Blob(this.chunks, { type: "audio/webm" });
                    let arrayBuffer = await blob.arrayBuffer();
                    let base64 = arrayBufferToBase64(arrayBuffer);
                    resolve(base64);
                };
                this.mediaRecorder.requestData();
                this.mediaRecorder.stop();
                this.isRecording = false;
            });
        } else {
            return null;
        }
    }
};

function arrayBufferToBase64(buffer) {
    var binary = '';
    var bytes = new Uint8Array(buffer);
    var len = bytes.byteLength;
    for (var i = 0; i < len; i++) {
        binary += String.fromCharCode(bytes[i]);
    }
    return btoa(binary);
}