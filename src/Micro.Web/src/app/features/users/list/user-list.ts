import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { UserService, UserResponse } from '../user.service';
import { AuthService } from '../../../core/auth/auth';
import { NotificationService } from '../../../core/ui/notification.service';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../../../core/ui/confirm-dialog';
import { InviteUserDialogComponent } from '../invite/invite-user';
import { EditRolesDialogComponent } from '../edit-roles/edit-roles';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, MatButtonModule, MatTooltipModule, MatDialogModule],
  template: `
    <div class="flex flex-col gap-6">
      <div class="flex justify-between items-center">
        <div>
          <h2 class="text-2xl font-bold text-ink tracking-tight mb-1">Team Members</h2>
          <p class="text-sm text-ink-light font-medium">Manage user access and roles</p>
        </div>
        
        @if (authService.hasPermission('User', 'Invite')) {
          <button (click)="openInviteDialog()" class="btn-primary flex items-center gap-2">
            <mat-icon>person_add</mat-icon>
            Invite User
          </button>
        }
      </div>

      <div class="card-honeycomb overflow-hidden !p-0">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="bg-surface-alt border-b border-slate-200">
              <th class="px-6 py-4 text-xs font-bold text-ink uppercase tracking-widest w-1/3">User</th>
              <th class="px-6 py-4 text-xs font-bold text-ink uppercase tracking-widest w-1/3">Roles</th>
              <th class="px-6 py-4 text-xs font-bold text-ink uppercase tracking-widest w-1/4">Status</th>
              <th class="px-6 py-4 text-xs font-bold text-ink uppercase tracking-widest text-right">Actions</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100">
            @for (user of users(); track user.id) {
              <tr class="hover:bg-slate-50/50 transition-colors group">
                <td class="px-6 py-4">
                  <div class="flex flex-col">
                    <span class="font-bold text-ink">{{ user.fullName }}</span>
                    <span class="text-xs text-slate-500">{{ user.email }}</span>
                  </div>
                </td>
                <td class="px-6 py-4">
                  <div class="flex flex-wrap gap-2">
                    @for (role of user.roles; track role.id) {
                      <span class="badge-status badge-slate">{{ role.name }}</span>
                    }
                  </div>
                </td>
                <td class="px-6 py-4">
                  @if (user.isInvitePending) {
                    <div class="flex flex-col gap-1">
                      <span class="badge-status badge-amber w-fit">Invite Pending</span>
                      <span class="text-[10px] text-slate-400 font-bold uppercase tracking-wider">
                        Sent {{ user.inviteSentAt | date:'shortDate' }}
                      </span>
                    </div>
                  } @else {
                    <span class="badge-status badge-green">Active</span>
                  }
                </td>
                <td class="px-6 py-4 text-right">
                  <div class="flex items-center justify-end gap-2">
                    @if (authService.hasPermission('User', 'ManageRoles')) {
                      <button mat-icon-button (click)="openEditRolesDialog(user)" matTooltip="Edit Roles" class="text-slate-400 hover:text-ink transition-colors">
                        <mat-icon>manage_accounts</mat-icon>
                      </button>
                    }

                    @if (user.isInvitePending && authService.hasPermission('User', 'Invite')) {
                      <button mat-icon-button (click)="resendInvite(user.id)" matTooltip="Resend Invite" class="text-slate-400 hover:text-honeycomb-600 transition-colors">
                        <mat-icon>mark_email_read</mat-icon>
                      </button>
                    }
                    
                    @if (authService.hasPermission('User', 'Delete') && user.id !== authService.user()?.id) {
                      <button mat-icon-button (click)="deleteUser(user)" matTooltip="Remove User" class="!text-red-400 hover:!bg-red-50 hover:!text-red-600 transition-colors">
                        <mat-icon>close</mat-icon>
                      </button>
                    }
                  </div>
                </td>
              </tr>
            } @empty {
              <tr>
                <td colspan="4" class="px-6 py-20 text-center">
                  <div class="text-slate-400 font-medium">No team members found.</div>
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>
    </div>
  `
})
export class UserListComponent implements OnInit {
  private userService = inject(UserService);
  private dialog = inject(MatDialog);
  private notification = inject(NotificationService);
  authService = inject(AuthService);

  users = signal<UserResponse[]>([]);

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    this.userService.getUsers().subscribe({
      next: (data) => this.users.set(data),
      error: () => this.notification.error('Failed to load users.')
    });
  }

  openInviteDialog() {
    const dialogRef = this.dialog.open(InviteUserDialogComponent, {
      width: '450px',
      panelClass: '!p-0'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.userService.inviteUser(result).subscribe({
          next: () => {
            this.notification.success('Invitation sent successfully.');
            this.loadUsers();
          },
          error: (err) => {
            console.error(err);
            this.notification.error('Failed to send invitation. Email might already exist.');
          }
        });
      }
    });
  }

  openEditRolesDialog(user: UserResponse) {
    const dialogRef = this.dialog.open(EditRolesDialogComponent, {
      width: '450px',
      data: {
        userId: user.id,
        fullName: user.fullName,
        email: user.email,
        currentRoleIds: user.roles.map(r => r.id)
      }
    });

    dialogRef.afterClosed().subscribe(roleIds => {
      if (roleIds) {
        this.userService.manageRoles(user.id, roleIds).subscribe({
          next: () => {
            this.notification.success('Roles updated successfully.');
            this.loadUsers();
          },
          error: () => this.notification.error('Failed to update roles.')
        });
      }
    });
  }

  resendInvite(userId: string) {
    this.userService.resendInvite(userId).subscribe({
      next: () => this.notification.success('Invitation resent successfully.'),
      error: () => this.notification.error('Failed to resend invitation.')
    });
  }

  deleteUser(user: UserResponse) {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Remove User',
        message: `Are you sure you want to remove ${user.fullName} from the system? This action cannot be undone and will revoke any pending invitations.`,
        confirmText: 'Remove User',
        isDestructive: true
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.userService.deleteUser(user.id).subscribe({
          next: () => {
            this.notification.success('User removed successfully.');
            this.loadUsers();
          },
          error: () => this.notification.error('Failed to remove user.')
        });
      }
    });
  }
}
