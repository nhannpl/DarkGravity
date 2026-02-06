import { Component, OnInit, OnDestroy, inject, signal, computed, effect, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { StoryService } from '../../services/story.service';
import { TtsService } from '../../services/tts.service';
import { Story } from '../../models/story.model';
import { MarkdownPipe } from '../../pipes/markdown.pipe';

@Component({
  selector: 'app-story-reader',
  standalone: true,
  imports: [CommonModule, RouterLink, MarkdownPipe],
  templateUrl: './story-reader.component.html',
  styleUrls: [
    './story-reader.component.css',
    './styles/tts/tts-animations.css',
    './styles/tts/tts-controls.css',
    './styles/tts/tts-seeker.css',
    './styles/tts/tts-settings.css',
    './styles/tts/tts-highlight.css',
    './styles/tts/tts-floating.css',
    './story-reader-analysis.css'
  ]
})
export class StoryReaderComponent implements OnInit, OnDestroy {
  private route = inject(ActivatedRoute);
  private storyService = inject(StoryService);
  private sanitizer = inject(DomSanitizer);
  private el = inject(ElementRef);
  public tts = inject(TtsService);

  constructor() {
    effect(() => {
      const index = this.tts.currentChunkIndex();
      // Only auto-scroll if playing to avoid annoying jumps when just exploring
      if (this.tts.isPlaying() && !this.tts.isPaused()) {
        setTimeout(() => this.scrollToActiveChunk(), 100);
      }
    });
  }

  private scrollToActiveChunk() {
    const activeEl = this.el.nativeElement.querySelector('.active-chunk');
    if (!activeEl) return;

    const rect = activeEl.getBoundingClientRect();
    const isInViewport = (
      rect.top >= 100 && // Add some buffer for header
      rect.bottom <= (window.innerHeight || document.documentElement.clientHeight)
    );

    if (!isInViewport) {
      activeEl.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
  }

  story = signal<Story | null>(null);
  loading = signal<boolean>(true);
  isTranscriptOpen = signal<boolean>(false);

  videoEmbedUrl = computed(() => {
    const s = this.story();
    if (s && (s.url.includes('youtube.com') || s.url.includes('youtu.be'))) {
      const videoId = s.id.startsWith('yt_') ? s.id.replace('yt_', '') : this.extractYouTubeId(s.url);
      if (videoId) {
        return this.sanitizer.bypassSecurityTrustResourceUrl(`https://www.youtube.com/embed/${videoId}?autoplay=0&rel=0&modestbranding=1`);
      }
    }
    return null;
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.storyService.getStoryById(id).subscribe({
        next: (data) => {
          this.story.set(data);
          this.loading.set(false);
          // Auto-open if it's a Reddit story (no video), close if it's YouTube
          const isYoutube = data.url.includes('youtube.com') || data.url.includes('youtu.be');
          this.isTranscriptOpen.set(!isYoutube);

          // Pre-load text into TTS for chunking display
          this.tts.loadText(data.bodyText);
        },
        error: (err) => {
          console.error('Failed to transmit story:', err);
          this.loading.set(false);
        }
      });
    } else {
      this.loading.set(false);
    }
  }

  getHighlightedChunk(chunk: string, index: number): string {
    const isCurrent = index === this.tts.currentChunkIndex();
    const charIndex = this.tts.activeCharIndex();

    if (!isCurrent || charIndex === -1) {
      return chunk;
    }

    // Simple word finding logic starting at charIndex
    const before = chunk.substring(0, charIndex);
    const remainder = chunk.substring(charIndex);
    const wordMatch = remainder.match(/^(\S+)(.*)/s); // Word then rest

    if (wordMatch) {
      const word = wordMatch[1];
      const after = wordMatch[2];
      return `${before}<span class="word-highlight">${word}</span>${after}`;
    }

    return chunk;
  }

  toggleTranscript() {
    this.isTranscriptOpen.update(v => !v);
  }

  private extractYouTubeId(url: string): string | null {
    const regExp = /^.*(youtu.be\/|v\/|u\/\w\/|embed\/|watch\?v=|\&v=)([^#\&\?]*).*/;
    const match = url.match(regExp);
    return (match && match[2].length === 11) ? match[2] : null;
  }

  getScaryLevel(score: number): string {
    if (score >= 8) return 'ELDRIITCH';
    if (score >= 5) return 'UNSETTLING';
    return 'EERIE';
  }

  getStorySource(url: string): string {
    if (url.includes('youtube.com') || url.includes('youtu.be')) return 'YOUTUBE';
    if (url.includes('reddit.com')) return 'REDDIT';
    return 'VOID';
  }

  readStory() {
    const s = this.story();
    if (s) {
      this.tts.speak(s.bodyText);
    }
  }

  pauseStory() {
    this.tts.pause();
  }

  resumeStory() {
    this.tts.resume();
  }

  stopStory() {
    this.tts.stop();
  }

  private wasPlayingBeforeDrag = false;

  onSeekStart() {
    this.wasPlayingBeforeDrag = !this.tts.isPaused() && this.tts.isPlaying();
    if (this.wasPlayingBeforeDrag) {
      this.tts.pause();
    }
  }

  onSeek(event: Event) {
    const target = event.target as HTMLInputElement;
    const value = parseFloat(target.value);

    // Pass the state to seek, let the service handle the resume/delay logic
    this.tts.seek(value, this.wasPlayingBeforeDrag);
  }

  onVoiceChange(event: Event) {
    const select = event.target as HTMLSelectElement;
    const voiceUri = select.value;
    const voice = this.tts.voices().find(v => v.voiceURI === voiceUri);
    if (voice) {
      this.tts.setVoice(voice);
    }
  }

  skipToChunk(index: number) {
    this.tts.playFromChunk(index);
  }

  onRateChange(event: Event) {
    const input = event.target as HTMLInputElement;
    let rate = parseFloat(input.value);
    // Clamp value
    if (rate < 0.5) rate = 0.5;
    if (rate > 2.0) rate = 2.0;

    this.tts.setRate(rate);
  }

  formatTime(seconds: number): string {
    if (!seconds || isNaN(seconds)) return '00:00';
    const mins = Math.floor(seconds / 60);
    const secs = Math.floor(seconds % 60);
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  }

  ngOnDestroy(): void {
    this.tts.stop();
  }
}
