export class TtsEngine {
    private static synth = window.speechSynthesis;

    static stop() {
        this.synth.cancel();
    }

    static pause() {
        this.synth.pause();
    }

    static resume() {
        this.synth.resume();
    }

    static speak(utterance: SpeechSynthesisUtterance) {
        this.synth.speak(utterance);
    }

    static get isSpeaking(): boolean {
        return this.synth.speaking;
    }

    static get isPending(): boolean {
        return this.synth.pending;
    }
}
