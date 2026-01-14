import { Component, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SearchService } from './services/search.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('dark-gravity-web');
  private searchService = inject(SearchService);

  onSearch(event: Event) {
    const query = (event.target as HTMLInputElement).value;
    this.searchService.setQuery(query);
  }
}
