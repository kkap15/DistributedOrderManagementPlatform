import { Injectable } from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {AuthService} from '@auth0/auth0-angular';

@Injectable({
  providedIn: 'root',
})
export class ApiService {

  private baseUrl = 'http://localhost:5002/gateway';

  constructor(private http: HttpClient, private auth: AuthService) {
  }

  createOrder() {
    const order = {
      id: 0,
      orderNumber: 'ORD-001',
      userId: 1,
      createdDate: new Date(),
      totalAmount: 100,
      status: 'Created'
    };

    return this.http.post(`${this.baseUrl}/order`, order);
  }
}
