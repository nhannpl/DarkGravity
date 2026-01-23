import { Component, inject, signal } from '@angular/core';
import { RouterOutlet, Router, NavigationEnd } from '@angular/router';
import { SearchService } from './services/search.service';
import { filter } from 'rxjs';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('dark-gravity-web');
  protected readonly isMenuOpen = signal(false);
  private searchService = inject(SearchService);
  private router = inject(Router);

  constructor() {
    // Scroll to top on navigation and close menu
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe(() => {
      window.scrollTo(0, 0);
      this.isMenuOpen.set(false);
    });
  }

  toggleMenu() {
    this.isMenuOpen.update(open => !open);
  }

  onSearch(event: Event) {
    const query = (event.target as HTMLInputElement).value;
    this.searchService.setSearchTerm(query);
  }
}
