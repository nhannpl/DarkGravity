import { Component, OnInit, inject, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { StoryService } from '../../services/story.service';
import { SearchService } from '../../services/search.service';
import { Story } from '../../models/story.model';
import { StorySortFields, SortOrders } from '../../constants/story.constants';

@Component({
    selector: 'app-story-feed',
    standalone: true,
    imports: [CommonModule, RouterLink, FormsModule],
    templateUrl: './story-feed.component.html',
    styleUrl: './story-feed.component.css'
})
export class StoryFeedComponent implements OnInit {
    private storyService = inject(StoryService);
    public searchService = inject(SearchService);

    stories = this.storyService.stories;
    loading = this.storyService.loading;

    constructor() {
        // Automatically fetch stories when search/filter/sort parameters change
        effect(() => {
            const params = {
                searchTerm: this.searchService.searchTerm(),
                sortBy: this.searchService.sortBy(),
                sortOrder: this.searchService.sortOrder(),
                minScaryScore: this.searchService.minScaryScore(),
                page: 1, // Reset to page 1 on filter change
                pageSize: 50
            };
            this.storyService.getStories(params).subscribe();
        });
    }

    ngOnInit(): void {
        // Initial fetch is handled by effect
    }

    onSortChange(event: Event) {
        const value = (event.target as HTMLSelectElement).value;
        const [field, order] = value.split(':');
        this.searchService.setSort(field, order as 'asc' | 'desc');
    }

    onScaryScoreChange(event: Event) {
        const value = (event.target as HTMLInputElement).value;
        this.searchService.setMinScaryScore(Number(value));
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
