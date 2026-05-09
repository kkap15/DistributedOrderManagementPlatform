import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { describe, it, expect, beforeEach, afterEach } from 'vitest';
import { ApiService } from './api';

describe('ApiService', () => {
  let service: ApiService;
  let http: HttpTestingController;

  const base = 'http://localhost:5002/gateway';

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ApiService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(ApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('login() POSTs to /gateway/user/login', () => {
    service.login().subscribe();
    const req = http.expectOne(`${base}/user/login`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('getOrders() GETs with userId query param', () => {
    service.getOrders('user-123').subscribe();
    const req = http.expectOne(`${base}/order?userId=user-123`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('createOrder() POSTs to /gateway/order with userId', () => {
    service.createOrder('user-123').subscribe();
    const req = http.expectOne(`${base}/order?userId=user-123`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body.userId).toBe('user-123');
    req.flush({});
  });
});
