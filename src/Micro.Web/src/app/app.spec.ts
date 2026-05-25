import { TestBed } from '@angular/core/testing';
import { App } from './app';
import { AuthService } from './core/auth/auth';
import { provideRouter } from '@angular/router';
import { signal } from '@angular/core';
import { vi } from 'vitest';

describe('App', () => {
  let authServiceMock: any;

  beforeEach(async () => {
    authServiceMock = {
      isAuthenticated: vi.fn().mockReturnValue(true),
      user: signal({
        id: '1',
        email: 'admin@microats.com',
        fullName: 'System Admin',
        photoUrl: null
      }),
      hasPermission: vi.fn().mockReturnValue(true),
      hasRole: vi.fn().mockReturnValue(true),
      logout: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        { provide: AuthService, useValue: authServiceMock },
        provideRouter([])
      ]
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should render brand name', async () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.brand')?.textContent).toContain('Micro ATS');
  });
});
