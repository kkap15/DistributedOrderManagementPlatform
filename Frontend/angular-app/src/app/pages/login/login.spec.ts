import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { AuthService } from '@auth0/auth0-angular';
import { of, Subject } from 'rxjs';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import { LoginComponent } from './login';

describe('LoginComponent', () => {
  let authIsAuthenticated$: Subject<boolean>;
  let mockAuth: any;
  let mockRouter: any;

  beforeEach(async () => {
    authIsAuthenticated$ = new Subject<boolean>();
    mockAuth = {
      isAuthenticated$: authIsAuthenticated$.asObservable(),
      loginWithPopup: vi.fn().mockReturnValue(of(undefined)),
    };
    mockRouter = { navigate: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        { provide: AuthService, useValue: mockAuth },
        { provide: Router, useValue: mockRouter },
      ],
    }).compileComponents();
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(LoginComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should navigate to /dashboard when already authenticated', () => {
    TestBed.createComponent(LoginComponent);
    authIsAuthenticated$.next(true);
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/dashboard']);
  });

  it('should not navigate when not authenticated', () => {
    TestBed.createComponent(LoginComponent);
    authIsAuthenticated$.next(false);
    expect(mockRouter.navigate).not.toHaveBeenCalled();
  });

  it('login() calls loginWithPopup', () => {
    const fixture = TestBed.createComponent(LoginComponent);
    fixture.componentInstance.login();
    expect(mockAuth.loginWithPopup).toHaveBeenCalled();
  });

  it('login() navigates to /dashboard on success', async () => {
    const fixture = TestBed.createComponent(LoginComponent);
    fixture.componentInstance.login();
    await fixture.whenStable();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/dashboard']);
  });
});
