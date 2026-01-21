export interface TtsMetadata {
    totalWords: number;
    totalChars: number;
    chunkLengths: number[];
}

export interface TtsSession {
    id: number;
    chunkIndex: number;
}

export type VoiceProvider = 'native' | 'cloud';

export interface TtsVoice {
    name: string;
    voiceURI: string;
    lang: string;
    provider: VoiceProvider;
    isNeural?: boolean;
}
