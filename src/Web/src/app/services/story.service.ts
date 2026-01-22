import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Story } from '../models/story.model';
import { PagedResult, StoryQueryParameters } from '../models/query.model';
import { Observable, tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class StoryService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5285/api/stories';

  stories = signal<Story[]>([]);
  totalCount = signal<number>(0);
  totalPages = signal<number>(0);
  loading = signal<boolean>(false);

  getStories(params?: StoryQueryParameters): Observable<PagedResult<Story>> {
    this.loading.set(true);

    let httpParams = new HttpParams();
    if (params) {
      if (params.searchTerm) httpParams = httpParams.set('searchTerm', params.searchTerm);
      if (params.minScaryScore) httpParams = httpParams.set('minScaryScore', params.minScaryScore.toString());
      if (params.platform) httpParams = httpParams.set('platform', params.platform);
      if (params.sortBy) httpParams = httpParams.set('sortBy', params.sortBy);
      if (params.sortOrder) httpParams = httpParams.set('sortOrder', params.sortOrder);
      if (params.page) httpParams = httpParams.set('page', params.page.toString());
      if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
    }

    return this.http.get<PagedResult<Story>>(this.apiUrl, { params: httpParams }).pipe(
      tap(result => {
        this.stories.set(result.items);
        this.totalCount.set(result.totalCount);
        const total = result.totalPages || Math.ceil(result.totalCount / (params?.pageSize || 50));
        this.totalPages.set(total);
        this.loading.set(false);
      })
    );
  }

  getStoryById(id: string): Observable<Story> {
    return this.http.get<Story>(`${this.apiUrl}/${id}`);
  }
}
