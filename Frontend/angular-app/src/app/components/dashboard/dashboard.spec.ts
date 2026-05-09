import { TestBed } from '@angular/core/testing';
import { AuthService } from '@auth0/auth0-angular';
import { of, throwError } from 'rxjs';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import { Dashboard } from './dashboard';
import { ApiService } from '../../services/api';
import { User } from '../../interfaces/User';
import { Order } from '../../interfaces/Orders';

const mockUser: User = { id: 'user-1', auth0id: 'auth0|123', name: 'Test User', email: 'test@example.com', createdAt: '2026-01-01' };
const mockOrder: Order = { id: 'ord-1', orderNumber: 'ORD-001', totalAmount: 99, status: 'Completed', createdAt: '2026-01-01' };

describe('Dashboard', () => {
  let mockApi: any;
  let mockAuth: any;

  beforeEach(async () => {
    mockApi = {
      login: vi.fn().mockReturnValue(of(mockUser)),
      createOrder: vi.fn().mockReturnValue(of({ transactionId: 'tx-1', status: 'Success' })),
      getOrders: vi.fn().mockReturnValue(of([mockOrder])),
    };
    mockAuth = { logout: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [Dashboard],
      providers: [
        { provide: ApiService, useValue: mockApi },
        { provide: AuthService, useValue: mockAuth },
      ],
    }).compileComponents();
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(Dashboard);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('ngOnInit calls api.login() and sets currentUser', async () => {
    const fixture = TestBed.createComponent(Dashboard);
    fixture.componentInstance.ngOnInit();
    await fixture.whenStable();
    expect(mockApi.login).toHaveBeenCalled();
    expect(fixture.componentInstance.currentUser).toEqual(mockUser);
  });

  it('createOrder - shows error if user not logged in', async () => {
    const fixture = TestBed.createComponent(Dashboard);
    fixture.componentInstance.currentUser = null;
    fixture.componentInstance.createOrder();
    expect(fixture.componentInstance.errorMessage).toBe('User not logged in');
    expect(mockApi.createOrder).not.toHaveBeenCalled();
  });

  it('createOrder - sets resultMessage on success', async () => {
    const fixture = TestBed.createComponent(Dashboard);
    fixture.componentInstance.currentUser = mockUser;
    fixture.componentInstance.createOrder();
    await fixture.whenStable();
    expect(fixture.componentInstance.resultMessage).toContain('tx-1');
    expect(fixture.componentInstance.resultMessage).toContain('Success');
  });

  it('createOrder - sets errorMessage on failure', async () => {
    mockApi.createOrder.mockReturnValue(throwError(() => new Error('network error')));
    const fixture = TestBed.createComponent(Dashboard);
    fixture.componentInstance.currentUser = mockUser;
    fixture.componentInstance.createOrder();
    await fixture.whenStable();
    expect(fixture.componentInstance.errorMessage).toBe('Order failed');
  });

  it('getOrders - array response populates orders', async () => {
    const fixture = TestBed.createComponent(Dashboard);
    fixture.componentInstance.currentUser = mockUser;
    fixture.componentInstance.getOrders();
    await fixture.whenStable();
    expect(fixture.componentInstance.orders).toEqual([mockOrder]);
  });

  it('getOrders - message response clears orders and sets resultMessage', async () => {
    mockApi.getOrders.mockReturnValue(of({ message: 'No orders found' }));
    const fixture = TestBed.createComponent(Dashboard);
    fixture.componentInstance.currentUser = mockUser;
    fixture.componentInstance.getOrders();
    await fixture.whenStable();
    expect(fixture.componentInstance.orders).toEqual([]);
    expect(fixture.componentInstance.resultMessage).toBe('No orders found');
  });

  it('logout calls authService.logout', () => {
    const fixture = TestBed.createComponent(Dashboard);
    fixture.componentInstance.logout();
    expect(mockAuth.logout).toHaveBeenCalledWith({ logoutParams: { returnTo: window.location.origin } });
  });
});
