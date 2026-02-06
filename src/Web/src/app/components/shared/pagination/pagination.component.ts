import { Component, Input, Output, EventEmitter, computed } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './pagination.component.html',
  styleUrl: './pagination.component.css'
})
export class PaginationComponent {
  @Input({ required: true }) currentPage!: number;
  @Input({ required: true }) totalPages!: number;
  @Input() maxVisiblePages = 5;
  @Input() pageSize = 50;
  @Input() pageSizeOptions = [10, 20, 50, 100, 200];

  @Output() pageChange = new EventEmitter<number>();
  @Output() pageSizeChange = new EventEmitter<number>();

  pages = computed(() => {
    const pages: (number | string)[] = [];
    const half = Math.floor(this.maxVisiblePages / 2);

    let start = Math.max(this.currentPage - half, 1);
    let end = Math.min(start + this.maxVisiblePages - 1, this.totalPages);

    if (end - start + 1 < this.maxVisiblePages) {
      start = Math.max(end - this.maxVisiblePages + 1, 1);
    }

    if (start > 1) {
      pages.push(1);
      if (start > 2) pages.push('...');
    }

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }

    if (end < this.totalPages) {
      if (end < this.totalPages - 1) pages.push('...');
      pages.push(this.totalPages);
    }

    return pages;
  });

  onPageClick(page: number | string) {
    if (typeof page === 'number' && page !== this.currentPage) {
      this.pageChange.emit(page);
    }
  }

  onPrev() {
    if (this.currentPage > 1) {
      this.pageChange.emit(this.currentPage - 1);
    }
  }

  onNext() {
    if (this.currentPage < this.totalPages) {
      this.pageChange.emit(this.currentPage + 1);
    }
  }

  onPageSizeChange(event: Event) {
    const newSize = Number((event.target as HTMLSelectElement).value);
    this.pageSizeChange.emit(newSize);
  }
}
