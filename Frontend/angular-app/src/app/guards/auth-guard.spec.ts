import { TestBed } from '@angular/core/testing';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '@auth0/auth0-angular';
import { BehaviorSubject, firstValueFrom } from 'rxjs';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import { authGuard } from './auth-guard';

describe('authGuard', () => {
  const executeGuard: CanActivateFn = (...guardParameters) =>
    TestBed.runInInjectionContext(() => authGuard(...guardParameters));

  let isAuthenticated$: BehaviorSubject<boolean>;
  let mockAuth: any;

  beforeEach(() => {
    isAuthenticated$ = new BehaviorSubject<boolean>(false);
    mockAuth = {
      isAuthenticated$: isAuthenticated$.asObservable(),
      loginWithRedirect: vi.fn(),
    };

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: mockAuth },
        { provide: Router, useValue: { navigate: vi.fn() } },
      ],
    });
  });

  it('should be created', () => {
    expect(executeGuard).toBeTruthy();
  });

  it('returns true when authenticated', async () => {
    isAuthenticated$.next(true);
    const result = await firstValueFrom(executeGuard(null as any, null as any) as any);
    expect(result).toBe(true);
  });

  it('calls loginWithRedirect and returns false when not authenticated', async () => {
    isAuthenticated$.next(false);
    const result = await firstValueFrom(executeGuard(null as any, null as any) as any);
    expect(result).toBe(false);
    expect(mockAuth.loginWithRedirect).toHaveBeenCalled();
  });
});
