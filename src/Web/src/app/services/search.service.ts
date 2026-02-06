import { Injectable, signal } from '@angular/core';
import { StorySortFields, SortOrders } from '../constants/story.constants';

@Injectable({
  providedIn: 'root'
})
export class SearchService {
  searchTerm = signal<string>('');
  sortBy = signal<string>(StorySortFields.Upvotes);
  sortOrder = signal<'asc' | 'desc'>(SortOrders.Descending);
  minScaryScore = signal<number>(0);
  maxScaryScore = signal<number>(10);
  platform = signal<string>('');
  page = signal<number>(1);
  pageSize = signal<number>(50);

  setSearchTerm(term: string) {
    this.searchTerm.set(term);
    this.page.set(1);
  }

  setSort(field: string, order: 'asc' | 'desc') {
    this.sortBy.set(field);
    this.sortOrder.set(order);
    this.page.set(1);
  }

  setScaryScoreRange(min: number, max: number) {
    this.minScaryScore.set(min);
    this.maxScaryScore.set(max);
    this.page.set(1);
  }

  setPlatform(platform: string) {
    this.platform.set(platform);
    this.page.set(1);
  }

  setPage(page: number) {
    this.page.set(page);
  }

  setPageSize(size: number) {
    this.pageSize.set(size);
    this.page.set(1);
  }
}
