import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { RoleService, Role } from '../../roles/role.service';

export interface EditRolesData {
  userId: string;
  fullName: string;
  email: string;
  currentRoleIds: string[];
}

@Component({
  selector: 'app-edit-roles-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule],
  template: `
    <div class="p-6 max-w-md w-full">
      <div class="flex items-center gap-3 mb-6">
        <div class="w-10 h-10 bg-slate-100 text-slate-600 rounded-full flex items-center justify-center shrink-0">
          <mat-icon>manage_accounts</mat-icon>
        </div>
        <div>
          <h2 class="text-xl font-bold text-ink m-0">Edit Roles</h2>
          <p class="text-xs text-slate-400 font-medium">{{ data.fullName }} ({{ data.email }})</p>
        </div>
      </div>

      <div class="flex flex-col gap-2">
        <label class="text-xs font-bold text-ink uppercase tracking-wider mb-1">Assign Roles</label>
        
        @if (loadingRoles()) {
          <div class="text-sm text-slate-400">Loading roles...</div>
        } @else {
          <div class="max-h-48 overflow-y-auto border border-slate-200 rounded-xl divide-y divide-slate-100">
            @for (role of availableRoles(); track role.id) {
              <label class="flex items-center gap-3 p-3 hover:bg-slate-50 cursor-pointer">
                <input type="checkbox" 
                       class="w-4 h-4 text-honeycomb-500 border-slate-300 rounded focus:ring-honeycomb-500 cursor-pointer"
                       [checked]="isRoleSelected(role.id)"
                       (change)="toggleRole(role.id)">
                <div class="flex flex-col">
                  <span class="text-sm font-bold text-ink">{{ role.name }}</span>
                </div>
              </label>
            }
          </div>
        }
      </div>

      <div class="flex justify-end gap-3 mt-8">
        <button mat-button (click)="dialogRef.close()" class="!rounded-lg !px-6 !font-bold !text-slate-400">Cancel</button>
        <button (click)="onSubmit()" [disabled]="selectedRoles().length === 0" class="btn-primary !rounded-lg !px-8 !font-bold !shadow-none disabled:opacity-50">Save Changes</button>
      </div>
    </div>
  `
})
export class EditRolesDialogComponent implements OnInit {
  dialogRef = inject(MatDialogRef<EditRolesDialogComponent>);
  data = inject<EditRolesData>(MAT_DIALOG_DATA);
  private roleService = inject(RoleService);

  availableRoles = signal<Role[]>([]);
  loadingRoles = signal(true);
  selectedRoles = signal<string[]>([]);

  ngOnInit() {
    this.selectedRoles.set([...this.data.currentRoleIds]);
    
    this.roleService.getRoles().subscribe({
      next: (roles) => {
        this.availableRoles.set(roles);
        this.loadingRoles.set(false);
      },
      error: () => this.loadingRoles.set(false)
    });
  }

  isRoleSelected(roleId: string): boolean {
    return this.selectedRoles().includes(roleId);
  }

  toggleRole(roleId: string) {
    const current = this.selectedRoles();
    if (current.includes(roleId)) {
      this.selectedRoles.set(current.filter(id => id !== roleId));
    } else {
      this.selectedRoles.set([...current, roleId]);
    }
  }

  onSubmit() {
    if (this.selectedRoles().length > 0) {
      this.dialogRef.close(this.selectedRoles());
    }
  }
}
