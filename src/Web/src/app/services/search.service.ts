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

  setMinScaryScore(score: number) {
    this.minScaryScore.set(score);
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
