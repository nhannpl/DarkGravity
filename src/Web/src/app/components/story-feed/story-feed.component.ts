import { Component, OnInit, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { StoryService } from '../../services/story.service';
import { SearchService } from '../../services/search.service';
import { Story } from '../../models/story.model';

@Component({
    selector: 'app-story-feed',
    standalone: true,
    imports: [CommonModule, RouterLink],
    templateUrl: './story-feed.component.html',
    styleUrl: './story-feed.component.css'
})
export class StoryFeedComponent implements OnInit {
    private storyService = inject(StoryService);
    private searchService = inject(SearchService);

    loading = this.storyService.loading;

    // Filtered stories using computed signals
    filteredStories = computed(() => {
        const stories = this.storyService.stories();
        const query = this.searchService.query().toLowerCase();

        if (!query) return stories;

        return stories.filter(s =>
            s.title.toLowerCase().includes(query) ||
            s.bodyText.toLowerCase().includes(query)
        );
    });

    ngOnInit(): void {
        this.storyService.getStories().subscribe();
    }

    getScaryLevel(score: number): string {
        if (score >= 8) return 'ELDRIITCH';
        if (score >= 5) return 'UNSETTLING';
        return 'EERIE';
    }
}
