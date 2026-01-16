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

  setSearchTerm(term: string) {
    this.searchTerm.set(term);
  }

  setSort(field: string, order: 'asc' | 'desc') {
    this.sortBy.set(field);
    this.sortOrder.set(order);
  }

  setMinScaryScore(score: number) {
    this.minScaryScore.set(score);
  }
}
