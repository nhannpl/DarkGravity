export interface TtsMetadata {
    totalWords: number;
    totalChars: number;
    chunkLengths: number[];
}

export interface TtsSession {
    id: number;
    chunkIndex: number;
}
