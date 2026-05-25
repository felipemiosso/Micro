import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ApplicationsBoardComponent } from './applications-board';
import { ApplicationService, ApplicationStatus, ArchivalResolution } from '../application.service';
import { JobPostingService } from '../../job-postings/job-posting.service';
import { AuthService } from '../../../core/auth/auth';
import { of } from 'rxjs';
import { provideRouter } from '@angular/router';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { vi } from 'vitest';
import { signal } from '@angular/core';

describe('ApplicationsBoardComponent', () => {
  let component: ApplicationsBoardComponent;
  let fixture: ComponentFixture<ApplicationsBoardComponent>;
  let applicationService: any;
  let jobService: any;

  beforeEach(async () => {
    const applicationServiceSpy = {
      getApplications: vi.fn(),
      updateStatus: vi.fn()
    };
    const jobServiceSpy = {
      getAllJobs: vi.fn()
    };
    const authServiceSpy = {
      isAuthenticated: vi.fn().mockReturnValue(true),
      user: signal({
        id: '1',
        email: 'admin@microats.com',
        fullName: 'System Admin',
        photoUrl: null
      }),
      hasPermission: vi.fn().mockReturnValue(true),
      hasRole: vi.fn().mockReturnValue(true)
    };

    await TestBed.configureTestingModule({
      imports: [ApplicationsBoardComponent, HttpClientTestingModule],
      providers: [
        { provide: ApplicationService, useValue: applicationServiceSpy },
        { provide: JobPostingService, useValue: jobServiceSpy },
        { provide: AuthService, useValue: authServiceSpy },
        provideRouter([])
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ApplicationsBoardComponent);
    component = fixture.componentInstance;
    applicationService = TestBed.inject(ApplicationService);
    jobService = TestBed.inject(JobPostingService);

    applicationService.getApplications.mockReturnValue(of([]));
    jobService.getAllJobs.mockReturnValue(of([]));
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load applications on init', () => {
    const mockApps = [
      { 
        id: '1', 
        candidateName: 'John', 
        status: ApplicationStatus.Applied, 
        appliedAt: new Date().toISOString(),
        jobTitle: 'Job',
        jobPostingId: 'j1',
        candidateEmail: 'j@ex.com',
        archivalResolution: ArchivalResolution.None
      }
    ];
    applicationService.getApplications.mockReturnValue(of(mockApps));
    
    component.ngOnInit();
    
    expect(component.applied().length).toBe(1);
    expect(component.applied()[0].candidateName).toBe('John');
  });

  it('should only allow sequential moves', () => {
    // @ts-ignore - access private method for test
    expect(component.isMoveAllowed(ApplicationStatus.Applied, ApplicationStatus.Interview)).toBe(true);
    // @ts-ignore
    expect(component.isMoveAllowed(ApplicationStatus.Interview, ApplicationStatus.Offer)).toBe(true);
    // @ts-ignore
    expect(component.isMoveAllowed(ApplicationStatus.Applied, ApplicationStatus.Offer)).toBe(false);
  });
});
