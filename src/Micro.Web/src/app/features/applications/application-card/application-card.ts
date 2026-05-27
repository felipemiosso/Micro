import { Component, Input } from '@angular/core';
import { Application } from '../application.service';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-application-card',
  standalone: true,
  imports: [CommonModule, RouterLink, MatIconModule],
  templateUrl: './application-card.html',
})
export class ApplicationCardComponent {
  @Input({ required: true }) application!: Application;
}
