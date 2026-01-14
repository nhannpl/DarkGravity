import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { StoryService } from '../../services/story.service';
import { Story } from '../../models/story.model';
import { MarkdownPipe } from '../../pipes/markdown.pipe';

@Component({
  selector: 'app-story-reader',
  standalone: true,
  imports: [CommonModule, RouterLink, MarkdownPipe],
  templateUrl: './story-reader.component.html',
  styleUrl: './story-reader.component.css'
})
export class StoryReaderComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private storyService = inject(StoryService);
  private sanitizer = inject(DomSanitizer);

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
}
