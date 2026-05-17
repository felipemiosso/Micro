import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ApplicationCardComponent } from './application-card';
import { ApplicationStatus } from '../application.service';
import { provideRouter } from '@angular/router';

describe('ApplicationCardComponent', () => {
  let component: ApplicationCardComponent;
  let fixture: ComponentFixture<ApplicationCardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ApplicationCardComponent],
      providers: [provideRouter([])]
    }).compileComponents();

    fixture = TestBed.createComponent(ApplicationCardComponent);
    component = fixture.componentInstance;
    component.application = {
      id: '1',
      candidateName: 'Test Candidate',
      jobTitle: 'Test Job',
      status: ApplicationStatus.Applied,
      appliedAt: new Date().toISOString(),
      jobPostingId: 'job1',
      candidateEmail: 'test@example.com',
      archivalResolution: 0
    };
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render candidate name', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.candidate-name')?.textContent).toContain('Test Candidate');
  });
});
