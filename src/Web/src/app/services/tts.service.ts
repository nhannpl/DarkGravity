import { Injectable, signal } from '@angular/core';

@Injectable({
    providedIn: 'root'
})
export class TtsService {
    private synth = window.speechSynthesis;
    private utterance: SpeechSynthesisUtterance | null = null;
    private fullText: string = '';
    private currentStartIndex: number = 0;

    isPlaying = signal<boolean>(false);
    isPaused = signal<boolean>(false);
    progress = signal<number>(0); // 0 to 100

    speak(text: string, startIndex: number = 0) {
        this.stopPlayback(); // Cancel current but don't reset progress if we are seeking

        this.fullText = text;
        this.currentStartIndex = startIndex;
        const textToSpeak = text.substring(startIndex);

        if (!textToSpeak.trim()) {
            this.isPlaying.set(false);
            this.progress.set(100);
            return;
        }

        this.utterance = new SpeechSynthesisUtterance(textToSpeak);
        this.utterance.rate = 1.0;
        this.utterance.pitch = 0.9;

        const setVoice = () => {
            const voices = this.synth.getVoices();
            const preferredVoice = voices.find(v => v.name.includes('Google US English') || v.name.includes('Daniel') || v.lang === 'en-GB');
            if (preferredVoice && this.utterance) {
                this.utterance.voice = preferredVoice;
            }
        };

        if (this.synth.getVoices().length === 0) {
            this.synth.onvoiceschanged = setVoice;
        } else {
            setVoice();
        }

        this.utterance.onstart = () => {
            this.isPlaying.set(true);
            this.isPaused.set(false);
        };

        this.utterance.onend = () => {
            // Small timeout to check if we really stopped or if another one started (seeking)
            setTimeout(() => {
                if (!this.synth.speaking) {
                    this.isPlaying.set(false);
                    this.isPaused.set(false);
                    if (this.progress() > 95) this.progress.set(100);
                }
            }, 100);
        };

        this.utterance.onboundary = (event) => {
            if (event.name === 'word') {
                const charIndex = this.currentStartIndex + event.charIndex;
                const p = (charIndex / this.fullText.length) * 100;
                this.progress.set(p);
            }
        };

        this.utterance.onerror = (event) => {
            if (event.error !== 'interrupted') {
                console.error('TTS error:', event);
                this.isPlaying.set(false);
                this.isPaused.set(false);
            }
        };

        this.synth.speak(this.utterance);
    }

    private stopPlayback() {
        this.synth.cancel();
    }

    pause() {
        if (this.isPlaying() && !this.isPaused()) {
            this.synth.pause();
            this.isPaused.set(true);
        }
    }

    resume() {
        if (this.isPlaying() && this.isPaused()) {
            this.synth.resume();
            this.isPaused.set(false);
        }
    }

    stop() {
        this.stopPlayback();
        this.isPlaying.set(false);
        this.isPaused.set(false);
        this.progress.set(0);
        this.fullText = '';
        this.currentStartIndex = 0;
    }

    seek(percentage: number) {
        if (!this.fullText) return;

        const charIndex = Math.floor((percentage / 100) * this.fullText.length);

        // Attempt to find the start of a sentence or at least a word to avoid weird mid-word start
        let startIndex = charIndex;
        const lastPeriod = this.fullText.lastIndexOf('.', charIndex);
        if (lastPeriod !== -1 && charIndex - lastPeriod < 100) {
            startIndex = lastPeriod + 1;
        } else {
            const lastSpace = this.fullText.lastIndexOf(' ', charIndex);
            if (lastSpace !== -1) startIndex = lastSpace + 1;
        }

        this.progress.set(percentage);
        this.speak(this.fullText, startIndex);
    }
}
