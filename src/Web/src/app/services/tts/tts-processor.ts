import { TtsMetadata } from './tts-types';

export class TtsProcessor {
    /**
     * Splits text into manageable chunks (sentences) and calculates metadata.
     */
    static processText(text: string): { chunks: string[], metadata: TtsMetadata } {
        // Chunking by sentences roughly
        // Split by period, question mark, exclamation point, but keep them.
        // Also handle newlines as distinct breaks.
        const rawChunks = text.match(/[^.!?\n]+[.!?\n]+|[^.!?\n]+$/g) || [text];
        const chunks = rawChunks.filter(c => c.trim().length > 0);

        const words = text.split(/\s+/).filter(w => w.length > 0).length;
        const chunkLengths = chunks.map(c => c.length);
        const totalChars = chunkLengths.reduce((a, b) => a + b, 0);

        return {
            chunks,
            metadata: {
                totalWords: words,
                totalChars,
                chunkLengths
            }
        };
    }
}
