import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { RoleService, AvailableAction } from '../role.service';
import { MatIconModule } from '@angular/material/icon';
import { NotificationService } from '../../../core/ui/notification.service';

@Component({
  selector: 'app-role-editor',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, MatIconModule],
  template: `
    <div class="p-8 bg-hex-pattern min-h-screen">
      <div class="max-w-3xl mx-auto">
        <div class="mb-10">
          <a routerLink="/admin/settings" [queryParams]="{tab: 'roles'}" class="text-xs font-bold text-honeycomb-600 uppercase tracking-widest flex items-center gap-1 mb-4 no-underline">
            <mat-icon class="!text-[14px] !w-[14px] !h-[14px]">arrow_back</mat-icon>
            Back to Roles
          </a>
          <h1 class="text-4xl font-bold text-ink tracking-tight">Create New Role</h1>
        </div>

        <form (ngSubmit)="save()" class="space-y-8">
          <div class="card-honeycomb">
            <div class="form-group mb-8">
              <label class="text-xs font-black uppercase tracking-widest text-slate-400 mb-2 block">Role Name</label>
              <input 
                type="text" 
                [(ngModel)]="roleName" 
                name="name" 
                placeholder="e.g. HR Manager" 
                class="w-full text-2xl font-bold border-b-2 border-slate-100 focus:border-honeycomb-500 outline-none pb-2 transition-colors">
            </div>

            <label class="text-xs font-black uppercase tracking-widest text-slate-400 mb-4 block">Assign Permissions</label>
            
            <div class="grid grid-cols-1 md:grid-cols-2 gap-x-8 gap-y-10">
              @for (resource of getResources(); track resource) {
                <div>
                  <h3 class="text-sm font-bold text-ink mb-4 flex items-center gap-2 border-l-4 border-honeycomb-400 pl-3">
                    {{ resource }}
                  </h3>
                  <div class="space-y-3">
                    @for (action of getActionsByResource(resource); track action.permission) {
                      <label class="flex items-start gap-3 cursor-pointer group">
                        <input 
                          type="checkbox" 
                          [checked]="isSelected(action.permission)"
                          (change)="togglePermission(action.permission)"
                          class="mt-1 accent-honeycomb-600">
                        <div class="min-w-0">
                          <div class="text-sm font-bold text-ink group-hover:text-honeycomb-600 transition-colors">{{ action.action }}</div>
                          <div class="text-[11px] text-slate-500 leading-tight">{{ action.description }}</div>
                        </div>
                      </label>
                    }
                  </div>
                </div>
              }
            </div>
          </div>

          <div class="flex justify-end gap-4">
            <a routerLink="/admin/settings" [queryParams]="{tab: 'roles'}" class="btn-secondary">Cancel</a>
            <button type="submit" class="btn-primary px-10" [disabled]="!roleName || selectedPermissions.length === 0">
              Save Role
            </button>
          </div>
        </form>
      </div>
    </div>
  `
})
export class RoleEditorComponent implements OnInit {
  private roleService = inject(RoleService);
  private router = inject(Router);
  private notification = inject(NotificationService);

  roleName = '';
  availableActions = signal<AvailableAction[]>([]);
  selectedPermissions: string[] = [];

  ngOnInit() {
    this.roleService.getAvailableActions().subscribe(actions => {
      this.availableActions.set(actions);
    });
  }

  getResources(): string[] {
    return Array.from(new Set(this.availableActions().map(a => a.resource))).sort();
  }

  getActionsByResource(resource: string): AvailableAction[] {
    return this.availableActions().filter(a => a.resource === resource).sort((a, b) => a.action.localeCompare(b.action));
  }

  isSelected(permission: string): boolean {
    return this.selectedPermissions.includes(permission);
  }

  togglePermission(permission: string) {
    const index = this.selectedPermissions.indexOf(permission);
    if (index > -1) {
      this.selectedPermissions.splice(index, 1);
    } else {
      this.selectedPermissions.push(permission);
    }
  }

  save() {
    if (!this.roleName || this.selectedPermissions.length === 0) return;

    this.roleService.createRole(this.roleName, this.selectedPermissions).subscribe({
      next: () => {
        this.notification.success('Role created successfully');
        this.router.navigate(['/admin/settings'], { queryParams: { tab: 'roles' } });
      },
      error: () => this.notification.error('Failed to create role')
    });
  }
}
