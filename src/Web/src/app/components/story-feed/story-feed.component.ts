import { Component, OnInit, inject, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { StoryService } from '../../services/story.service';
import { SearchService } from '../../services/search.service';
import { Story } from '../../models/story.model';
import { StorySortFields, SortOrders } from '../../constants/story.constants';
import { PaginationComponent } from '../shared/pagination/pagination.component';

@Component({
    selector: 'app-story-feed',
    standalone: true,
    imports: [CommonModule, RouterLink, FormsModule, PaginationComponent],
    templateUrl: './story-feed.component.html',
    styleUrl: './story-feed.component.css'
})
export class StoryFeedComponent implements OnInit {
    private storyService = inject(StoryService);
    public searchService = inject(SearchService);
    private fetchSubscription?: Subscription;

    stories = this.storyService.stories;
    loading = this.storyService.loading;
    totalPages = this.storyService.totalPages;
    totalCount = this.storyService.totalCount;

    constructor() {
        // Automatically fetch stories when search/filter/sort parameters change
        effect(() => {
            const params = {
                searchTerm: this.searchService.searchTerm(),
                sortBy: this.searchService.sortBy(),
                sortOrder: this.searchService.sortOrder(),
                minScaryScore: this.searchService.minScaryScore(),
                maxScaryScore: this.searchService.maxScaryScore(),
                platform: this.searchService.platform(),
                page: this.searchService.page(),
                pageSize: this.searchService.pageSize()
            };

            // Cancel any pending request
            if (this.fetchSubscription) {
                this.fetchSubscription.unsubscribe();
            }

            this.fetchSubscription = this.storyService.getStories(params).subscribe();
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

    updateScaryScore(min: number | null, max: number | null) {
        const currentMin = this.searchService.minScaryScore();
        const currentMax = this.searchService.maxScaryScore();

        let newMin = min !== null ? min : currentMin;
        let newMax = max !== null ? max : currentMax;

        // Ensure min <= max
        if (newMin > newMax) {
            if (min !== null) newMax = newMin; // if moving min past max, push max
            else newMin = newMax; // if moving max past min, push min (shouldn't happen with correct UI constraints but good safety)
        }

        this.searchService.setScaryScoreRange(newMin, newMax);
    }

    onMinScaryScoreChange(event: Event) {
        const value = Number((event.target as HTMLInputElement).value);
        this.updateScaryScore(value, null);
    }

    onMaxScaryScoreChange(event: Event) {
        const value = Number((event.target as HTMLInputElement).value);
        this.updateScaryScore(null, value);
    }

    onPlatformChange(event: Event) {
        const value = (event.target as HTMLSelectElement).value;
        this.searchService.setPlatform(value);
    }

    onPageChange(page: number) {
        this.searchService.setPage(page);
        // Scroll to top when page changes
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }

    onPageSizeChange(size: number) {
        this.searchService.setPageSize(size);
        window.scrollTo({ top: 0, behavior: 'smooth' });
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

    getTrackGradient(): string {
        const min = this.searchService.minScaryScore();
        const max = this.searchService.maxScaryScore();
        const minPercent = (min / 10) * 100;
        const maxPercent = (max / 10) * 100;

        return `linear-gradient(to right, 
            rgba(255, 255, 255, 0.1) 0%, 
            rgba(255, 255, 255, 0.1) ${minPercent}%, 
            rgba(124, 77, 255, 0.8) ${minPercent}%, 
            rgba(124, 77, 255, 0.8) ${maxPercent}%, 
            rgba(255, 255, 255, 0.1) ${maxPercent}%, 
            rgba(255, 255, 255, 0.1) 100%)`;
    }
}
