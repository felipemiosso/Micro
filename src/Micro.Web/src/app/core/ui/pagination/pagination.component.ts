import { Component, input, output, computed } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="flex items-center justify-between border-t border-slate-200 bg-white px-4 py-3 sm:px-6 mt-4">
      <div class="flex flex-1 justify-between sm:hidden">
        <button 
          (click)="prevPage()" 
          [disabled]="page() === 1"
          class="relative inline-flex items-center rounded-md border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-50 disabled:opacity-50">
          Previous
        </button>
        <button 
          (click)="nextPage()" 
          [disabled]="page() === totalPages()"
          class="relative ml-3 inline-flex items-center rounded-md border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-50 disabled:opacity-50">
          Next
        </button>
      </div>
      <div class="hidden sm:flex sm:flex-1 sm:items-center sm:justify-between">
        <div>
          <p class="text-sm text-slate-700">
            Showing
            <span class="font-medium">{{ startIndex() }}</span>
            to
            <span class="font-medium">{{ endIndex() }}</span>
            of
            <span class="font-medium">{{ totalCount() }}</span>
            results
          </p>
        </div>
        <div class="flex items-center gap-4">
          <!-- Page Size Selector -->
          <div class="flex items-center gap-2">
            <span class="text-sm text-slate-600">Rows per page:</span>
            <select 
              [value]="pageSize()" 
              (change)="onPageSizeChange($event)"
              class="rounded-md border border-slate-300 py-1.5 px-2 text-sm font-medium text-slate-700 shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500 bg-white">
              @for (size of pageSizeOptions(); track size) {
                <option [value]="size">{{ size }}</option>
              }
            </select>
          </div>

          <!-- Page Navigation -->
          <nav class="isolate inline-flex -space-x-px rounded-md shadow-sm" aria-label="Pagination">
            <button 
              (click)="prevPage()" 
              [disabled]="page() === 1"
              class="relative inline-flex items-center rounded-l-md px-2 py-2 text-slate-400 ring-1 ring-inset ring-slate-300 hover:bg-slate-50 focus:z-20 focus:outline-offset-0 disabled:opacity-50">
              <span class="sr-only">Previous</span>
              <svg class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
                <path fill-rule="evenodd" d="M12.79 5.23a.75.75 0 01-.02 1.06L8.832 10l3.938 3.71a.75.75 0 11-1.04 1.08l-4.5-4.25a.75.75 0 010-1.08l4.5-4.25a.75.75 0 011.06.02z" clip-rule="evenodd" />
              </svg>
            </button>
            
            @for (p of pages(); track p) {
              <button 
                (click)="goToPage(p)"
                [class.bg-indigo-600]="p === page()"
                [class.text-white]="p === page()"
                [class.ring-1]="true"
                [class.ring-inset]="true"
                [class.ring-slate-300]="p !== page()"
                [class.text-slate-900]="p !== page()"
                [class.hover:bg-slate-50]="p !== page()"
                class="relative inline-flex items-center px-4 py-2 text-sm font-semibold focus:z-20 focus:outline-offset-0">
                {{ p }}
              </button>
            }

            <button 
              (click)="nextPage()" 
              [disabled]="page() === totalPages()"
              class="relative inline-flex items-center rounded-r-md px-2 py-2 text-slate-400 ring-1 ring-inset ring-slate-300 hover:bg-slate-50 focus:z-20 focus:outline-offset-0 disabled:opacity-50">
              <span class="sr-only">Next</span>
              <svg class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
                <path fill-rule="evenodd" d="M7.21 14.77a.75.75 0 01.02-1.06L11.168 10 7.23 6.29a.75.75 0 111.04-1.08l4.5 4.25a.75.75 0 010 1.08l-4.5 4.25a.75.75 0 01-1.06-.02z" clip-rule="evenodd" />
              </svg>
            </button>
          </nav>
        </div>
      </div>
    </div>
  `
})
export class PaginationComponent {
  page = input.required<number>();
  pageSize = input.required<number>();
  totalCount = input.required<number>();
  totalPages = input.required<number>();
  pageSizeOptions = input<number[]>([10, 20, 50, 100]);

  pageChange = output<number>();
  pageSizeChange = output<number>();

  startIndex = computed(() => {
    if (this.totalCount() === 0) return 0;
    return (this.page() - 1) * this.pageSize() + 1;
  });

  endIndex = computed(() => {
    return Math.min(this.page() * this.pageSize(), this.totalCount());
  });

  pages = computed(() => {
    const list: number[] = [];
    for (let i = 1; i <= this.totalPages(); i++) {
      list.push(i);
    }
    return list;
  });

  prevPage() {
    if (this.page() > 1) {
      this.pageChange.emit(this.page() - 1);
    }
  }

  nextPage() {
    if (this.page() < this.totalPages()) {
      this.pageChange.emit(this.page() + 1);
    }
  }

  goToPage(p: number) {
    if (p !== this.page()) {
      this.pageChange.emit(p);
    }
  }

  onPageSizeChange(event: Event) {
    const target = event.target as HTMLSelectElement;
    this.pageSizeChange.emit(Number(target.value));
  }
}
