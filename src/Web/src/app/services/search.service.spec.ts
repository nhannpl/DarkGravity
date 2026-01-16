import { TestBed } from '@angular/core/testing';
import { SearchService } from './search.service';
import { StorySortFields, SortOrders } from '../constants/story.constants';

describe('SearchService', () => {
  let service: SearchService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SearchService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should initialize with default values', () => {
    expect(service.searchTerm()).toBe('');
    expect(service.sortBy()).toBe(StorySortFields.Upvotes);
    expect(service.sortOrder()).toBe(SortOrders.Descending);
    expect(service.minScaryScore()).toBe(0);
  });

  it('should update search term', () => {
    const term = 'haunting';
    service.setSearchTerm(term);
    expect(service.searchTerm()).toBe(term);
  });

  it('should update sorting options', () => {
    service.setSort(StorySortFields.ScaryScore, SortOrders.Ascending);
    expect(service.sortBy()).toBe(StorySortFields.ScaryScore);
    expect(service.sortOrder()).toBe(SortOrders.Ascending);
  });

  it('should update min scary score', () => {
    const score = 75;
    service.setMinScaryScore(score);
    expect(service.minScaryScore()).toBe(score);
  });
});
