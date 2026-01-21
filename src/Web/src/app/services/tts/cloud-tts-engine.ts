/**
 * CloudTtsEngine - Google Cloud Text-to-Speech via Backend API
 * 
 * PURPOSE: High-quality neural TTS voices via Google Cloud Platform
 * 
 * ARCHITECTURE:
 * - Calls backend API (/api/tts/synthesize) instead of Google directly
 * - Backend holds the API key (secure)
 * - Returns MP3 audio blob for playback
 * 
 * KEY FEATURES:
 * - Session-aware request management (prevents overlapping audio)
 * - Word boundary approximation for UI highlighting
 * - Automatic cleanup of audio resources
 * 
 * RESUME VALUE:
 * - Shows integration with GCP
 * - Demonstrates secure API design (no key exposure)
 * - Full-stack implementation
 */

export class CloudTtsEngine {
    // Audio element for playback
    private static audio: HTMLAudioElement | null = null;

    // State tracking
    private static isCurrentlySpeaking = false;
    private static isCurrentlyPaused = false;

    // REQUEST CANCELLATION: Incremented on each new speak() call to invalidate old requests
    // This prevents race conditions where a slow network request finishes AFTER a newer one
    private static currentRequestId = 0;

    // Backend API base URL
    private static apiBase = 'http://localhost:5285/api/tts';

    /**
     * Synthesizes text to speech using Google Cloud TTS via backend
     * 
     * FLOW:
     * 1. Increment requestId to invalidate any pending requests
     * 2. Stop any currently playing audio
     * 3. Call backend API to synthesize audio (network call - can be slow!)
     * 4. Check if this request is still valid (user might have changed voice/stopped)
     * 5. Play the audio and approximate word boundaries for UI highlighting
     * 
     * @param text - Text to speak
     * @param voiceId - Google Cloud voice ID (e.g., 'en-US-Neural2-C')
     * @param rate - Speech rate multiplier (1.0 = normal, 1.5 = 50% faster)
     * @param onWordBoundary - Optional callback for word-by-word highlighting
     */
    static async speak(
        text: string,
        voiceId: string,
        rate: number,
        onWordBoundary?: (charIndex: number) => void
    ): Promise<void> {
        // CRITICAL: Stop and invalidate previous requests BEFORE capturing our new requestId
        this.stop();
        const requestId = ++this.currentRequestId;

        try {
            this.isCurrentlySpeaking = true;
            this.isCurrentlyPaused = false;

            // DEBUG: Log synthesis request
            console.log('[CloudTTS] Synthesizing - Voice:', voiceId, 'Rate:', rate, 'Text:', text.substring(0, 50) + '...');

            // Call backend API to synthesize
            const response = await fetch(`${this.apiBase}/synthesize`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    text: text,
                    voiceId: voiceId,
                    rate: rate,
                    languageCode: 'en-US'
                })
            });

            if (!response.ok) {
                const errorText = await response.text();
                console.error('[CloudTTS] Backend error:', response.status, errorText);
                throw new Error(`TTS synthesis failed: ${response.status} ${response.statusText}`);
            }

            // NETWORK CALL COMPLETE: Get audio blob
            const blob = await response.blob();
            console.log('[CloudTTS] Received audio blob, size:', blob.size, 'type:', blob.type);

            // RACE CONDITION CHECK: Did the user change voice or stop while we were synthesizing?
            // If so, abandon this request silently
            if (requestId !== this.currentRequestId) {
                console.warn('[CloudTTS] Request outdated (requestId mismatch), aborting');
                return; // Outdated request, do nothing
            }

            const url = URL.createObjectURL(blob);
            this.audio = new Audio(url);

            return new Promise((resolve, reject) => {
                if (!this.audio || requestId !== this.currentRequestId) {
                    URL.revokeObjectURL(url);
                    console.warn('[CloudTTS] Audio element or request invalidated before play');
                    return resolve();
                }

                const audio = this.audio;

                audio.onended = () => {
                    this.isCurrentlySpeaking = false;
                    URL.revokeObjectURL(url);
                    console.log('[CloudTTS] Playback completed');
                    resolve();
                };

                audio.onerror = (e) => {
                    this.isCurrentlySpeaking = false;
                    URL.revokeObjectURL(url);
                    console.error('[CloudTTS] HTMLAudioElement error:', e);
                    reject(e);
                };

                // WORD BOUNDARY APPROXIMATION:
                // Google Cloud TTS doesn't provide word timing data in the response,
                // so we estimate them based on:
                // - Audio playback time
                // - Assumed reading rate (150 words per minute base)
                // - User's rate multiplier
                const words = text.split(/\s+/);
                const avgMsPerWord = (60000 / (150 * rate)); // milliseconds per word

                let currentWordIndex = 0;
                let charAccumulator = 0; // Track character position for highlighting

                // Poll every 50ms to update word position
                const interval = setInterval(() => {
                    // Stop if request outdated, audio stopped, or finished
                    if (requestId !== this.currentRequestId || !audio || audio.paused || !this.isCurrentlySpeaking) {
                        clearInterval(interval);
                        return;
                    }

                    // Calculate expected word index based on playback time
                    const currentTimeMs = audio.currentTime * 1000;
                    const expectedWordIndex = Math.floor(currentTimeMs / avgMsPerWord);

                    // Fire word boundary events for any words we've passed
                    if (expectedWordIndex > currentWordIndex && currentWordIndex < words.length) {
                        for (let i = currentWordIndex; i < expectedWordIndex && i < words.length; i++) {
                            if (onWordBoundary) onWordBoundary(charAccumulator);
                            charAccumulator += words[i].length + 1; // +1 for space
                        }
                        currentWordIndex = expectedWordIndex;
                    }

                    if (currentWordIndex >= words.length) clearInterval(interval);
                }, 50);

                console.log('[CloudTTS] Starting playback...');
                audio.play().then(() => {
                    console.log('[CloudTTS] Playback started successfully');
                }).catch(err => {
                    console.error('[CloudTTS] Playback failed/blocked:', err);
                    reject(err);
                });
            });
        } catch (error) {
            this.isCurrentlySpeaking = false;
            console.error('[CloudTTS] Synthesis failed:', error);
            throw error;
        }
    }

    /**
     * Stops playback and cleans up resources
     * 
     * CRITICAL: Also increments requestId to cancel any in-flight network requests
     */
    static stop() {
        this.currentRequestId++; // Invalidate pending synthesis requests
        if (this.audio) {
            this.audio.pause();
            this.audio.onended = null; // Prevent memory leaks
            this.audio.onerror = null;
            this.audio = null;
        }
        this.isCurrentlySpeaking = false;
        this.isCurrentlyPaused = false;
    }

    /**
     * Pause playback
     */
    static pause() {
        if (this.audio && !this.audio.paused) {
            this.audio.pause();
            this.isCurrentlyPaused = true;
        }
    }

    /**
     * Resume paused playback
     */
    static resume() {
        if (this.audio && this.audio.paused) {
            this.audio.play();
            this.isCurrentlyPaused = false;
        }
    }

    /**
     * Check if currently speaking
     */
    static get isSpeaking(): boolean {
        return this.isCurrentlySpeaking;
    }

    /**
     * Check if currently paused
     */
    static get isPaused(): boolean {
        return this.isCurrentlyPaused;
    }

    /**
     * Fetches the list of available Google Cloud TTS voices from backend
     */
    static async getVoices(): Promise<any[]> {
        try {
            console.log('[CloudTTS] Fetching voices from backend...');
            const response = await fetch(`${this.apiBase}/voices`);

            if (!response.ok) {
                throw new Error(`Failed to fetch voices: ${response.statusText}`);
            }

            const voices = await response.json();
            console.log('[CloudTTS] Fetched', voices.length, 'voices');
            return voices;
        } catch (error) {
            console.error('[CloudTTS] Failed to fetch voices:', error);
            return []; // Return empty array on error (will fall back to native voices)
        }
    }
}
