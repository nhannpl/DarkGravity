export interface StoryQueryParameters {
  searchTerm?: string;
  minScaryScore?: number;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
  platform?: string;
  page?: number;
  pageSize?: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
