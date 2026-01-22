import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PaginationComponent } from './pagination.component';
import { describe, it, expect, beforeEach, vi } from 'vitest';

describe('PaginationComponent', () => {
  let component: PaginationComponent;
  let fixture: ComponentFixture<PaginationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PaginationComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(PaginationComponent);
    component = fixture.componentInstance;
    
    // Set required inputs
    fixture.componentRef.setInput('currentPage', 1);
    fixture.componentRef.setInput('totalPages', 10);
    
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should calculate pages correctly for page 1', () => {
    const pages = component.pages();
    // Expected: [1, 2, 3, 4, 5, '...', 10]
    expect(pages).toContain(1);
    expect(pages).toContain('...');
    expect(pages).toContain(10);
  });

  it('should emit pageChange when onPageClick is called', () => {
    const spy = vi.spyOn(component.pageChange, 'emit');
    component.onPageClick(2);
    expect(spy).toHaveBeenCalledWith(2);
  });

  it('should not emit pageChange when current page is clicked', () => {
    const spy = vi.spyOn(component.pageChange, 'emit');
    component.onPageClick(1);
    expect(spy).not.toHaveBeenCalled();
  });

  it('should emit next page on onNext', () => {
    const spy = vi.spyOn(component.pageChange, 'emit');
    component.onNext();
    expect(spy).toHaveBeenCalledWith(2);
  });

  it('should emit prev page on onPrev, if not on first page', () => {
    fixture.componentRef.setInput('currentPage', 2);
    fixture.detectChanges();
    
    const spy = vi.spyOn(component.pageChange, 'emit');
    component.onPrev();
    expect(spy).toHaveBeenCalledWith(1);
  });
});
