import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { StoryService } from '../../services/story.service';
import { Story } from '../../models/story.model';

@Component({
  selector: 'app-story-reader',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './story-reader.component.html',
  styleUrl: './story-reader.component.css'
})
export class StoryReaderComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private storyService = inject(StoryService);

  story = signal<Story | null>(null);
  loading = signal<boolean>(true);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.storyService.getStoryById(id).subscribe({
        next: (data) => {
          this.story.set(data);
          this.loading.set(false);
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

  getScaryLevel(score: number): string {
    if (score >= 8) return 'ELDRIITCH';
    if (score >= 5) return 'UNSETTLING';
    return 'EERIE';
  }
}
