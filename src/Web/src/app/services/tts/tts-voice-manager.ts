import { WritableSignal } from '@angular/core';
import { TtsVoice } from './tts-types';
import { CloudTtsEngine } from './cloud-tts-engine';

/**
 * TtsVoiceManager - Manages available TTS voices
 * 
 * UPDATED: Now uses Google Cloud TTS instead of Edge TTS
 */
export class TtsVoiceManager {
    private synth = window.speechSynthesis;
    private cachedCloudVoices: TtsVoice[] | null = null;
    private lastOnlineState: boolean | null = null;
    
    constructor(
        private voices: WritableSignal<TtsVoice[]>,
        private selectedVoice: WritableSignal<TtsVoice | null>
    ) {}

    async initVoices(isOnline: boolean) {
        // Avoid re-fetching if online state hasn't changed
        if (this.lastOnlineState === isOnline && this.voices().length > 0) {
            return;
        }
        this.lastOnlineState = isOnline;

        const nativeVoices: TtsVoice[] = this.synth.getVoices().map(v => ({
            name: v.name,
            voiceURI: v.voiceURI,
            lang: v.lang,
            provider: 'native'
        }));

        let cloudVoices: TtsVoice[] = [];
        if (isOnline) {
            if (this.cachedCloudVoices) {
                cloudVoices = this.cachedCloudVoices;
            } else {
                try {
                    const rawCloudVoices = await CloudTtsEngine.getVoices();
                    cloudVoices = (rawCloudVoices as any[])
                        .map(v => ({
                            name: v.name,
                            voiceURI: v.voiceId,
                            lang: v.languageCode,
                            provider: 'cloud',
                            isNeural: true
                        }));
                    this.cachedCloudVoices = cloudVoices;
                } catch (error) {
                    console.error('Failed to fetch Cloud voices', error);
                }
            }
        }

        const allVoices = [...cloudVoices, ...nativeVoices].sort((a, b) => a.name.localeCompare(b.name));
        this.voices.set(allVoices);

        this.applySavedPreference(allVoices, isOnline);
    }

    private applySavedPreference(availableVoices: TtsVoice[], isOnline: boolean) {
        const savedUri = localStorage.getItem('tts_voice_uri');

        if (savedUri) {
            const found = availableVoices.find(v => v.voiceURI === savedUri);
            if (found && !(found.provider === 'cloud' && !isOnline)) {
                if (this.selectedVoice()?.voiceURI !== found.voiceURI) {
                    this.selectedVoice.set(found);
                }
                return;
            }
        }

        const currentSelected = this.selectedVoice();
        if (!currentSelected || (currentSelected.provider === 'cloud' && !isOnline)) {
            const preferred = availableVoices.find(v =>
                v.voiceURI.includes('JennyNeural') || 
                v.name.includes('Google US English') ||
                v.name.includes('Daniel') ||
                (v.lang.startsWith('en') && !v.name.includes('Compact'))
            );
            if (preferred && preferred.voiceURI !== currentSelected?.voiceURI) {
                this.selectedVoice.set(preferred);
            }
        }
    }

    saveVoicePreference(voice: TtsVoice | null) {
        if (voice) {
            localStorage.setItem('tts_voice_uri', voice.voiceURI);
        }
    }
}
