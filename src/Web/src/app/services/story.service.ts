import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Story } from '../models/story.model';
import { Observable, tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class StoryService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5285/api/stories';

  stories = signal<Story[]>([]);
  loading = signal<boolean>(false);

  getStories(): Observable<Story[]> {
    this.loading.set(true);
    return this.http.get<Story[]>(this.apiUrl).pipe(
      tap(data => {
        this.stories.set(data);
        this.loading.set(false);
      })
    );
  }

  getStoryById(id: number): Observable<Story> {
    return this.http.get<Story>(`${this.apiUrl}/${id}`);
  }
}
