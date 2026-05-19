import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/auth/auth';
import { NotificationService } from '../../../core/ui/notification.service';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  templateUrl: './profile.html',
  styleUrl: './profile.css'
})
export class ProfileComponent {
  public authService = inject(AuthService);
  private notification = inject(NotificationService);
  public user = this.authService.user;

  changePassword() {
    this.notification.info('This feature is currently being integrated with our identity provider.');
  }
}
