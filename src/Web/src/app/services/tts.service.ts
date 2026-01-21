import { Injectable, signal, effect, computed } from '@angular/core';
import { TtsProcessor } from './tts/tts-processor';
import { TtsVoiceManager } from './tts/tts-voice-manager';
import { TtsEngine } from './tts/tts-engine';

@Injectable({
    providedIn: 'root'
})
export class TtsService {
    // --- State Signals ---
    chunks = signal<string[]>([]);
    voices = signal<SpeechSynthesisVoice[]>([]);
    selectedVoice = signal<SpeechSynthesisVoice | null>(null);
    rate = signal<number>(1.0); 

    isPlaying = signal<boolean>(false);
    isPaused = signal<boolean>(false);
    activeCharIndex = signal<number>(-1);

    // Smooth Progress Tracking
    private chunkLengths: number[] = [];
    totalChars = signal<number>(0);
    elapsedChars = signal<number>(0);
    currentChunkChar = signal<number>(0); 

    // Progress
    currentChunkIndex = signal<number>(0);
    totalChunks = signal<number>(0);
    totalWords = signal<number>(0);

    // Private Engine State
    private currentUtterance: SpeechSynthesisUtterance | null = null;
    private currentSessionId = 0;
    private voiceManager: TtsVoiceManager;

    // --- Computed State ---

    progress = computed(() => {
        if (this.totalChars() === 0) return 0;
        const currentTotal = this.elapsedChars() + this.currentChunkChar();
        return (currentTotal / this.totalChars()) * 100;
    });

    estimatedDuration = computed(() => {
        const words = this.totalWords();
        if (words <= 0) return 0;
        const wordsPerSecond = (150 * this.rate()) / 60;
        return words / wordsPerSecond;
    });

    currentTime = computed(() => {
        const duration = this.estimatedDuration();
        if (duration === 0) return 0;
        return duration * (this.progress() / 100);
    });

    constructor() {
        this.voiceManager = new TtsVoiceManager(this.voices, this.selectedVoice);
        this.voiceManager.initVoices();

        this.loadPreferences();
        this.setupPersistence();
    }

    // --- Public API ---

    loadText(text: string) {
        const { chunks, metadata } = TtsProcessor.processText(text);
        this.chunks.set(chunks);
        this.totalWords.set(metadata.totalWords);
        this.totalChunks.set(chunks.length);
        this.chunkLengths = metadata.chunkLengths;
        this.totalChars.set(metadata.totalChars);

        this.resetProgress();
    }

    speak(text: string) {
        this.stop();
        if (this.chunks().length === 0) {
            this.loadText(text);
        }

        this.resetProgress();
        this.isPlaying.set(true);
        this.playChunk(0);
    }

    playFromChunk(index: number) {
        if (index < 0 || index >= this.chunks().length) return;
        this.stop();
        this.currentChunkIndex.set(index);
        this.isPlaying.set(true);
        this.playChunk(index);
    }

    pause() {
        if (this.isPlaying() && !this.isPaused()) {
            TtsEngine.pause();
            this.isPaused.set(true);
        }
    }

    resume() {
        if (this.isPlaying() && this.isPaused()) {
            TtsEngine.resume();
            this.isPaused.set(false);
            if (!TtsEngine.isSpeaking && !TtsEngine.isPending) {
                this.playChunk(this.currentChunkIndex());
            }
        }
    }

    stop() {
        this.currentSessionId++;
        TtsEngine.stop();
        this.isPlaying.set(false);
        this.isPaused.set(false);
        this.currentChunkIndex.set(0);
        this.activeCharIndex.set(-1);
    }

    seek(percentage: number, forcePlay = false) {
        if (this.chunks().length === 0) return;

        this.currentSessionId++;
        this.activeCharIndex.set(-1);
        TtsEngine.stop();

        const targetChar = Math.floor((percentage / 100) * this.totalChars());
        let accum = 0;
        let targetIndex = 0;

        for (let i = 0; i < this.chunkLengths.length; i++) {
            accum += this.chunkLengths[i];
            if (accum >= targetChar) {
                targetIndex = i;
                break;
            }
        }

        this.currentChunkIndex.set(targetIndex);

        if (forcePlay || (this.isPlaying() && !this.isPaused())) {
            this.isPaused.set(false);
            this.isPlaying.set(true);
            setTimeout(() => this.playChunk(targetIndex), 200);
        } else {
            this.elapsedChars.set(accum - this.chunkLengths[targetIndex]);
            this.currentChunkChar.set(0);
        }
    }

    setVoice(voice: SpeechSynthesisVoice) {
        this.selectedVoice.set(voice);
        if (this.isPlaying() && !this.isPaused()) {
            this.playChunk(this.currentChunkIndex());
        }
    }

    setRate(newRate: number) {
        this.rate.set(newRate);
        if (this.isPlaying() && !this.isPaused()) {
            this.playChunk(this.currentChunkIndex());
        }
    }

    // --- Private Helpers ---

    private resetProgress() {
        this.elapsedChars.set(0);
        this.currentChunkChar.set(0);
    }

    private playChunk(index: number) {
        const sessionId = this.currentSessionId;
        TtsEngine.stop();

        const chunks = this.chunks();
        if (index >= chunks.length) {
            this.stop();
            return;
        }

        this.updateElapsedChars(index);

        this.currentUtterance = new SpeechSynthesisUtterance(chunks[index]);
        const voice = this.selectedVoice();
        if (voice) this.currentUtterance.voice = voice;
        this.currentUtterance.rate = this.rate();
        this.currentUtterance.pitch = 1.0;

        this.setupUtteranceEvents(sessionId, index);
        TtsEngine.speak(this.currentUtterance);
    }

    private updateElapsedChars(index: number) {
        let elapsed = 0;
        for (let i = 0; i < index; i++) {
            elapsed += this.chunkLengths[i] || 0;
        }
        this.elapsedChars.set(elapsed);
        this.currentChunkChar.set(0);
    }

    private setupUtteranceEvents(sessionId: number, index: number) {
        if (!this.currentUtterance) return;

        this.currentUtterance.onboundary = (event) => {
            if (event.name === 'word') {
                this.activeCharIndex.set(event.charIndex);
                this.currentChunkChar.set(event.charIndex);
            }
        };

        this.currentUtterance.onend = () => {
            if (this.currentSessionId !== sessionId) return;
            if (this.isPlaying() && !this.isPaused()) {
                const nextIndex = index + 1;
                this.currentChunkIndex.set(nextIndex);
                this.playChunk(nextIndex);
            }
        };

        this.currentUtterance.onerror = (e) => {
            if (this.currentSessionId !== sessionId) return;
            console.error('TTS Chunk Error', e);
            if (e.error !== 'interrupted' && e.error !== 'canceled') {
                const nextIndex = index + 1;
                this.currentChunkIndex.set(nextIndex);
                setTimeout(() => this.playChunk(nextIndex), 100);
            }
        };
    }

    private loadPreferences() {
        const savedRate = localStorage.getItem('tts_rate');
        if (savedRate) this.rate.set(parseFloat(savedRate));
    }

    private setupPersistence() {
        effect(() => {
            localStorage.setItem('tts_rate', this.rate().toString());
            this.voiceManager.saveVoicePreference(this.selectedVoice());
        });
    }
}

