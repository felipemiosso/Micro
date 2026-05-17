import { Component, Input } from '@angular/core';
import { Application } from '../application.service';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-application-card',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './application-card.html',
  styleUrl: './application-card.css',
})
export class ApplicationCardComponent {
  @Input({ required: true }) application!: Application;
}
