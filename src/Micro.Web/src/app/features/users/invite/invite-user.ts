import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { RoleService, Role } from '../../roles/role.service';

@Component({
  selector: 'app-invite-user-dialog',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatDialogModule, MatButtonModule, MatIconModule],
  template: `
    <div class="p-6 max-w-md w-full">
      <div class="flex items-center gap-3 mb-6">
        <div class="w-10 h-10 bg-honeycomb-100 text-honeycomb-600 rounded-full flex items-center justify-center shrink-0">
          <mat-icon>person_add</mat-icon>
        </div>
        <h2 class="text-xl font-bold text-ink m-0">Invite Team Member</h2>
      </div>

      <form [formGroup]="inviteForm" (ngSubmit)="onSubmit()" class="flex flex-col gap-4">
        <div class="flex flex-col gap-2">
          <label class="text-xs font-bold text-ink uppercase tracking-wider">Full Name</label>
          <input type="text" formControlName="fullName" class="input-honeycomb" placeholder="Jane Doe">
        </div>

        <div class="flex flex-col gap-2">
          <label class="text-xs font-bold text-ink uppercase tracking-wider">Email Address</label>
          <input type="email" formControlName="email" class="input-honeycomb" placeholder="jane@company.com">
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

        <div class="flex justify-end gap-3 mt-4">
          <button mat-button type="button" (click)="dialogRef.close()" class="!rounded-lg !px-6 !font-bold !text-slate-400">Cancel</button>
          <button type="submit" [disabled]="inviteForm.invalid || selectedRoles().length === 0" class="btn-primary !rounded-lg !px-8 !font-bold !shadow-none disabled:opacity-50">Send Invite</button>
        </div>
      </form>
    </div>
  `
})
export class InviteUserDialogComponent implements OnInit {
  dialogRef = inject(MatDialogRef<InviteUserDialogComponent>);
  private fb = inject(FormBuilder);
  private roleService = inject(RoleService);

  inviteForm: FormGroup;
  availableRoles = signal<Role[]>([]);
  loadingRoles = signal(true);
  selectedRoles = signal<string[]>([]);

  constructor() {
    this.inviteForm = this.fb.group({
      fullName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]]
    });
  }

  ngOnInit() {
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
    if (this.inviteForm.valid && this.selectedRoles().length > 0) {
      this.dialogRef.close({
        ...this.inviteForm.value,
        roleIds: this.selectedRoles()
      });
    }
  }
}
