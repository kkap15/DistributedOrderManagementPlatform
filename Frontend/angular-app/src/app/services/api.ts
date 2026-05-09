import { Injectable } from '@angular/core';
import {HttpClient} from '@angular/common/http';
import { User } from '../interfaces/User';
import { Order } from '../interfaces/Orders';

@Injectable({
  providedIn: 'root',
})
export class ApiService {

  private baseUrl = 'http://localhost:5002/gateway';

  constructor(private http: HttpClient) {
  }
  
  login() {
    return this.http.post<User>(`${this.baseUrl}/user/login`, {});
  }
  
  getUserProfile() {
    return this.http.get<Order[]>(`${this.baseUrl}/user/me`);
  }

  createOrder(userId: string) {
    const order = {
      orderNumber: 'ORD-001',
      userId: userId,
      totalAmount: Math.random(),
      status: 'Created'
    };

    return this.http.post(`${this.baseUrl}/order?userId=${userId}`, order);
  }
  
  getOrders(userId: string) {
    return this.http.get<Order[]>(`${this.baseUrl}/order?userId=${userId}`);
  }
}
