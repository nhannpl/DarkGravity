/**
 * TtsService - Text-to-Speech Service
 * 
 * ARCHITECTURE:
 * - Uses Angular signals for reactive state management
 * - Supports TWO voice engines:
 *   1. TtsEngine (native browser Web Speech API)
 *   2. CloudTtsEngine (Google Cloud TTS via backend API)  ← UPDATED
 * 
 * PLAYBACK FLOW:
 * 1. Text split into chunks (sentences) by TtsProcessor
 * 2. Each chunk played sequentially via playChunk()
 * 3. Engine selected based on voice.provider ('native' or 'cloud')  ← UPDATED
 * 4. Session management prevents overlapping playback
 * 
 * SESSION MANAGEMENT:
 * - currentSessionId incremented to cancel old playback
 * - Each chunk captures sessionId and checks if still valid before continuing
 * - Prevents race conditions when user changes voice/rate mid-playback
 * 
 * POTENTIAL PERFORMANCE ISSUES:
 * - High-frequency signal updates (activeCharIndex, currentChunkChar) during word highlighting
 * - Voice re-initialization during playback could cause stuttering
 * - Cloud TTS synthesis has network latency (500ms-2s per chunk)
 */

import { Injectable, signal, effect, computed, inject } from '@angular/core';
import { TtsProcessor } from './tts/tts-processor';
import { TtsVoiceManager } from './tts/tts-voice-manager';
import { TtsEngine } from './tts/tts-engine';
import { CloudTtsEngine } from './tts/cloud-tts-engine';
import { TtsVoice } from './tts/tts-types';
import { ConnectivityService } from './connectivity.service';

@Injectable({
    providedIn: 'root'
})
export class TtsService {
    // =============================================================================
    // SERVICES & DEPENDENCIES
    // =============================================================================
    
    /** ConnectivityService - Monitors online/offline status (currently not used) */
    private connectivity = inject(ConnectivityService);

    // =============================================================================
    // PUBLIC STATE SIGNALS (Reactive - UI updates automatically)
    // =============================================================================
    
    /** Text chunks (sentences) ready for TTS playback */
    chunks = signal<string[]>([]);
    
    /** Available voices (both native and Cloud if online) - populated by TtsVoiceManager */
    voices = signal<TtsVoice[]>([]);
    
    /** Currently selected voice - determines which engine (native/cloud) to use */
    selectedVoice = signal<TtsVoice | null>(null);
    
    /** Speech rate multiplier (0.5 = half speed, 2.0 = double speed) */
    rate = signal<number>(1.0); 

    /** Playback state - true when reading (includes paused state) */
    isPlaying = signal<boolean>(false);
    
    /** Pause state - true when playback is paused */
    isPaused = signal<boolean>(false);
    
    /** Cloud TTS only - true during network synthesis (before audio plays) */
    isSynthesizing = signal<boolean>(false);
    
    /** Character index for word highlighting - updated frequently during playback
     *  ⚠️ HIGH FREQUENCY UPDATE - Could cause performance issues */
    activeCharIndex = signal<number>(-1);

    /** User notification message (e.g., "Internet lost. Switching to standard voice.") */
    connectionMessage = signal<string | null>(null);

    // -----------------------------------------------------------------------------
    // Progress Tracking
    // -----------------------------------------------------------------------------
    
    /** Character count of each chunk - used for smooth progress calculation */
    private chunkLengths: number[] = [];
    
    /** Total characters in all chunks */
    totalChars = signal<number>(0);
    
    /** Characters spoken so far (completed chunks) */
    elapsedChars = signal<number>(0);
    
    /** Character position within current chunk
     *  ⚠️ HIGH FREQUENCY UPDATE - Updated during word highlighting */
    currentChunkChar = signal<number>(0); 

    /** Current chunk being played (0-indexed) */
    currentChunkIndex = signal<number>(0);
    
    /** Total number of chunks (sentences) */
    totalChunks = signal<number>(0);
    
    /** Total word count (for duration estimation) */
    totalWords = signal<number>(0);

    // =============================================================================
    // PRIVATE ENGINE STATE (Internal playback management)
    // =============================================================================
    
    /** Native TTS - Current utterance being spoken (null if using Cloud TTS) */
    private currentUtterance: SpeechSynthesisUtterance | null = null;
    
    /** Session tracking - Incremented to cancel old playback sessions
     *  Used to prevent race conditions when changing voice/rate mid-playback */
    private currentSessionId = 0;
    
    /** Voice manager - Handles voice discovery and selection */
    private voiceManager: TtsVoiceManager;

    // =============================================================================
    // COMPUTED VALUES (Auto-updated when dependencies change)
    // =============================================================================

    /** Overall progress percentage (0-100)
     *  Calculated from: (elapsed chars + current chunk position) / total chars */
    progress = computed(() => {
        if (this.totalChars() === 0) return 0;
        const currentTotal = this.elapsedChars() + this.currentChunkChar();
        return (currentTotal / this.totalChars()) * 100;
    });

    /** Estimated total duration in seconds
     *  Based on 150 words per minute baseline, adjusted by rate */
    estimatedDuration = computed(() => {
        const words = this.totalWords();
        if (words <= 0) return 0;
        const wordsPerSecond = (150 * this.rate()) / 60;
        return words / wordsPerSecond;
    });

    /** Estimated elapsed time in seconds
     *  Calculated from: total duration * (progress / 100) */
    currentTime = computed(() => {
        const duration = this.estimatedDuration();
        if (duration === 0) return 0;
        return duration * (this.progress() / 100);
    });

    // =============================================================================
    // CONSTRUCTOR - Initialization
    // =============================================================================
    
    constructor() {
        // Initialize voice manager (populates voices signal)
        this.voiceManager = new TtsVoiceManager(this.voices, this.selectedVoice);

        // Load voices at startup - assume online (connectivity monitoring disabled)
        this.voiceManager.initVoices(true);

        // BROWSER QUIRK: Chrome/Edge fire 'voiceschanged' after page load
        // Re-init voices when browser finishes loading native voices
        window.speechSynthesis.onvoiceschanged = () => {
            this.voiceManager.initVoices(true);
        };

        // =========================================================================
        // CONNECTIVITY MONITORING - DISABLED
        // =========================================================================
        // 
        // REASON: Caused stuttering during playback
        // 
        // ISSUE: Angular effect() runs on every change detection cycle.
        //        If connectivity status changes during playback:
        //        1. effect() fires
        //        2. initVoices() called
        //        3. voices.set() triggers change detection
        //        4. Causes micro-pause in audio playback
        // 
        // SOLUTION: Disabled automatic connectivity monitoring.
        //           Edge voices will still work if online, just fail gracefully if offline.
        // 
        // TO RE-ENABLE: Uncomment below (but expect potential stuttering)
        // =========================================================================
        
        // let lastOnlineStatus = this.connectivity.isOnline();
        // effect(() => {
        //     const isOnline = this.connectivity.isOnline();
        //
        //     // Only act on actual state changes
        //     if (isOnline !== lastOnlineStatus) {
        //         lastOnlineStatus = isOnline;
        //         this.voiceManager.initVoices(isOnline);
        //
        //         // If we lost connection and were using a Cloud voice, let the user know
        //         const currentVoice = this.selectedVoice();
        //         if (!isOnline && currentVoice?.provider === 'cloud') {
        //             this.connectionMessage.set('Internet lost. Switching to standard voice.');
        //             setTimeout(() => this.connectionMessage.set(null), 5000);
        //         }
        //     }
        // });

        this.loadPreferences();
        this.setupPersistence();
    }

    // =============================================================================
    // PUBLIC API - Methods called by components
    // =============================================================================

    /**
     * Pre-processes text into speakable chunks (sentences)
     * 
     * FLOW:
     * 1. TtsProcessor splits text by sentence boundaries (.!?
)
     * 2. Calculates metadata (word count, char count)
     * 3. Updates all state signals
     * 
     * NOTE: Does NOT start playback - call speak() afterwards
     */
    loadText(text: string) {
        const { chunks, metadata } = TtsProcessor.processText(text);
        this.chunks.set(chunks);
        this.totalWords.set(metadata.totalWords);
        this.totalChunks.set(chunks.length);
        this.chunkLengths = metadata.chunkLengths;
        this.totalChars.set(metadata.totalChars);

        this.resetProgress();
    }

    /**
     * Start TTS playback from the beginning
     * 
     * FLOW:
     * 1. Stop any existing playback (increments sessionId)
     * 2. Load text into chunks if not already done
     * 3. Reset progress tracking
     * 4. Start playing first chunk
     */
    speak(text: string) {
        this.stop();
        if (this.chunks().length === 0) {
            this.loadText(text);
        }

        this.resetProgress();
        this.isPlaying.set(true);
        this.playChunk(0);
    }

    /**
     * Start playback from a specific chunk (sentence)
     * 
     * USED FOR: "Click to read" feature - user clicks a sentence to start there
     * 
     * @param index - The chunk index to start from (0-based)
     */
    playFromChunk(index: number) {
        if (index < 0 || index >= this.chunks().length) return;
        this.stop();
        this.currentChunkIndex.set(index);
        this.isPlaying.set(true);
        this.playChunk(index);
    }

    /**
     * Pause playback (can be resumed later)
     * 
     * ENGINE HANDLING:
     * - Cloud TTS: Pauses the HTMLAudioElement
     * - Native TTS: Calls speechSynthesis.pause()
     */
    pause() {
        if (this.isPlaying() && !this.isPaused()) {
            const voice = this.selectedVoice();
            if (voice?.provider === 'cloud') {
                CloudTtsEngine.pause();
            } else {
                TtsEngine.pause();
            }
            this.isPaused.set(true);
        }
    }

    /**
     * Resume paused playback
     * 
     * BEHAVIOR:
     * - If audio still loaded: Resume from pause point
     * - If audio released: Re-synthesize and play current chunk
     * 
     * NOTE: Native TTS releases audio after a timeout, Cloud TTS keeps it in memory
     */
    resume() {
        if (this.isPlaying() && this.isPaused()) {
            const voice = this.selectedVoice();
            this.isPaused.set(false);

            if (voice?.provider === 'cloud') {
                // Cloud TTS: Check if audio still loaded
                if (CloudTtsEngine.isSpeaking) {
                    CloudTtsEngine.resume();
                } else {
                    // Audio was released, re-synthesize
                    this.playChunk(this.currentChunkIndex());
                }
            } else {
                // Native TTS: Check if utterance still pending
                TtsEngine.resume();
                if (!TtsEngine.isSpeaking && !TtsEngine.isPending) {
                    // Browser released the utterance, replay chunk
                    this.playChunk(this.currentChunkIndex());
                }
            }
        }
    }

    /**
     * Stop playback completely and reset to beginning
     * 
     * CRITICAL SESSION MANAGEMENT:
     * - Increments currentSessionId to invalidate all pending chunks
     * - Stops both engines (only one will be active, but safe to call both)
     * - Resets all playback state
     * 
     * PREVENTS RACE CONDITIONS:
     * Example: User stops during chunk 5's network synthesis (Cloud TTS)
     *          - currentSessionId increments from 10 to 11
     *          - When chunk 5 synthesis completes, it checks: sessionId(10) !== currentSessionId(11)
     *          - Chunk 5 silently aborts instead of playing
     */
    stop() {
        this.currentSessionId++; // ⚠️ CRITICAL: Invalidate all in-flight chunks
        TtsEngine.stop();        // Stop native browser TTS
        CloudTtsEngine.stop();    // Stop Cloud TTS (also increments its own requestId)
        this.isPlaying.set(false);
        this.isPaused.set(false);
        this.currentChunkIndex.set(0);
        this.activeCharIndex.set(-1);
    }

    /**
     * Change voice mid-playback (if playing) or just update preference
     * 
     * BEHAVIOR DURING PLAYBACK:
     * - Increments sessionId to cancel current chunk
     * - Immediately replays current chunk with new voice
     * - May cause brief pause during transition
     * 
     * POTENTIAL ISSUE:
     * If called rapidly (user spam-clicking voice dropdown), could cause stuttering
     */
    setVoice(voice: TtsVoice) {
        this.selectedVoice.set(voice);
        if (this.isPlaying() && !this.isPaused()) {
            this.currentSessionId++; // Cancel current chunk
            this.playChunk(this.currentChunkIndex()); // Replay with new voice
        }
    }

    /**
     * Change speech rate mid-playback or just update preference
     * 
     * SAME BEHAVIOR AS setVoice() - replays current chunk with new rate
     */
    setRate(newRate: number) {
        this.rate.set(newRate);
        if (this.isPlaying() && !this.isPaused()) {
            this.currentSessionId++;
            this.playChunk(this.currentChunkIndex());
        }
    }

    /**
     * Jump to a specific position in the text (progress bar seeking)
     * 
     * FLOW:
     * 1. Convert percentage to character position
     * 2. Find which chunk contains that character
     * 3. Jump to that chunk
     * 
     * @param percentage - Target position (0-100)
     * @param forcePlay - If true, start playing even if currently paused
     * 
     * NOTE: Has 200ms delay before playing to prevent rapid seeking stutters
     */
    seek(percentage: number, forcePlay = false) {
        if (this.chunks().length === 0) return;

        // Cancel current playback
        this.currentSessionId++;
        this.activeCharIndex.set(-1);
        TtsEngine.stop();
        CloudTtsEngine.stop();

        // Calculate target chunk from percentage
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

        // Resume playback or just update position
        if (forcePlay || (this.isPlaying() && !this.isPaused())) {
            this.isPaused.set(false);
            this.isPlaying.set(true);
            // Small delay to prevent stutter if user drags seek bar
            setTimeout(() => this.playChunk(targetIndex), 200);
        } else {
            // Just update progress without playing
            this.elapsedChars.set(accum - this.chunkLengths[targetIndex]);
            this.currentChunkChar.set(0);
        }
    }

    // =============================================================================
    // PRIVATE HELPER METHODS
    // =============================================================================

    private resetProgress() {
        this.elapsedChars.set(0);
        this.currentChunkChar.set(0);
    }

    private playChunk(index: number) {
        const sessionId = this.currentSessionId;
        TtsEngine.stop();
        CloudTtsEngine.stop();

        const chunks = this.chunks();
        if (index >= chunks.length) {
            this.stop();
            return;
        }

        this.updateElapsedChars(index);
        const voice = this.selectedVoice();

        if (voice?.provider === 'cloud') {
            this.playCloudChunk(sessionId, index, chunks[index], voice);
        } else {
            this.playNativeChunk(sessionId, index, chunks[index], voice);
        }
    }

    private playCloudChunk(sessionId: number, index: number, text: string, voice: TtsVoice) {
        this.isSynthesizing.set(true);
        CloudTtsEngine.speak(text, voice.voiceURI, this.rate(), (charIndex) => {
            if (this.currentSessionId !== sessionId) return;
            this.activeCharIndex.set(charIndex);
            this.currentChunkChar.set(charIndex);
        }).then(() => {
            if (this.currentSessionId !== sessionId) return;
            this.isSynthesizing.set(false);
            this.onChunkEnd(index);
        }).catch(err => {
            if (this.currentSessionId !== sessionId) return;
            this.isSynthesizing.set(false);
            console.error('Cloud TTS Error, falling back to Native', err);
            this.playNativeChunk(sessionId, index, text, null);
        });
    }

    private playNativeChunk(sessionId: number, index: number, text: string, voice: TtsVoice | null) {
        this.currentUtterance = new SpeechSynthesisUtterance(text);

        // Find actual SpeechSynthesisVoice if metadata matches
        if (voice?.provider === 'native') {
            const nativeVoice = window.speechSynthesis.getVoices().find(v => v.voiceURI === voice.voiceURI);
            if (nativeVoice) this.currentUtterance.voice = nativeVoice;
        }

        this.currentUtterance.rate = this.rate();
        this.currentUtterance.pitch = 1.0;

        this.setupUtteranceEvents(sessionId, index);
        TtsEngine.speak(this.currentUtterance);
    }

    private onChunkEnd(index: number) {
        if (this.isPlaying() && !this.isPaused()) {
            const nextIndex = index + 1;
            this.currentChunkIndex.set(nextIndex);
            this.playChunk(nextIndex);
        }
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
            this.onChunkEnd(index);
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

