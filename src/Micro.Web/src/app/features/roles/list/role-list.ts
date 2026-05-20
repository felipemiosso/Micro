import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { RoleService, Role } from '../role.service';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
  selector: 'app-role-list',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, MatTooltipModule],
  template: `
    <div class="flex flex-col gap-6">
      <div class="flex justify-between items-center">
        <div>
          <h2 class="text-2xl font-bold text-ink tracking-tight mb-1">Role Management</h2>
          <p class="text-sm text-ink-light font-medium">Define roles and assign system-wide permissions.</p>
        </div>
        <a routerLink="/roles/new" class="btn-primary flex items-center gap-2">
          <mat-icon>add</mat-icon>
          Create New Role
        </a>
      </div>

      <div class="card-honeycomb !p-0 overflow-hidden">
        <table class="w-full text-left border-collapse role-table">
          <thead>
            <tr class="bg-surface-alt border-b border-slate-100">
              <th class="px-6 py-4 text-xs font-bold text-ink uppercase tracking-widest">Role Name</th>
              <th class="px-6 py-4 text-xs font-bold text-ink uppercase tracking-widest">Permissions</th>
              <th class="px-6 py-4 text-xs font-bold text-ink uppercase tracking-widest text-right">Actions</th>
            </tr>
          </thead>
          <tbody>
            @for (role of roles(); track role.id) {
              <tr class="border-b border-slate-50 hover:bg-slate-50/50 transition-colors">
                <td class="px-6 py-4 font-bold text-ink">{{ role.name }}</td>
                <td class="px-6 py-4">
                  <div class="flex flex-wrap gap-1">
                    @for (p of role.permissions; track p) {
                      <span class="text-[10px] bg-slate-100 text-slate-600 px-2 py-0.5 rounded font-bold uppercase">{{ p }}</span>
                    }
                  </div>
                </td>
                <td class="px-6 py-4 text-right">
                  <button mat-icon-button color="primary" matTooltip="Edit Role" disabled>
                    <mat-icon>edit</mat-icon>
                  </button>
                </td>
              </tr>
            } @empty {
              <tr>
                <td colspan="3" class="px-6 py-20 text-center text-slate-400 font-medium">
                  No roles defined yet. Start by creating one.
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>
    </div>
  `
})
export class RoleListComponent implements OnInit {
  private roleService = inject(RoleService);
  roles = signal<Role[]>([]);

  ngOnInit() {
    this.roleService.getRoles().subscribe(data => this.roles.set(data));
  }
}
