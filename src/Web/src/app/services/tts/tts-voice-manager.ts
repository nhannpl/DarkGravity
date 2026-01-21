import { signal, WritableSignal } from '@angular/core';

export class TtsVoiceManager {
    private synth = window.speechSynthesis;
    
    constructor(
        private voices: WritableSignal<SpeechSynthesisVoice[]>,
        private selectedVoice: WritableSignal<SpeechSynthesisVoice | null>
    ) {}

    initVoices() {
        const populate = () => {
            let availableVoices = this.synth.getVoices();
            availableVoices = availableVoices.sort((a, b) => a.name.localeCompare(b.name));
            this.voices.set(availableVoices);

            const savedUri = localStorage.getItem('tts_voice_uri');
            if (savedUri) {
                const found = availableVoices.find(v => v.voiceURI === savedUri);
                if (found) {
                    this.selectedVoice.set(found);
                    return;
                }
            }

            if (!this.selectedVoice()) {
                const preferred = availableVoices.find(v =>
                    v.name.includes('Google US English') ||
                    v.name.includes('Daniel') ||
                    (v.lang.startsWith('en') && !v.name.includes('Compact'))
                );
                if (preferred) this.selectedVoice.set(preferred);
            }
        };

        if (this.synth.getVoices().length > 0) {
            populate();
        } else {
            this.synth.onvoiceschanged = populate;
        }
    }

    saveVoicePreference(voice: SpeechSynthesisVoice | null) {
        if (voice) {
            localStorage.setItem('tts_voice_uri', voice.voiceURI);
        }
    }
}
