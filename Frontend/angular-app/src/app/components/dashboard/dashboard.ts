import {ChangeDetectorRef, Component} from '@angular/core';
import {ApiService} from '../../services/api';
import {AuthService} from '@auth0/auth0-angular';
import { finalize } from "rxjs";
import { NgZone } from '@angular/core';
import {CommonModule} from '@angular/common';
import { User } from '../../interfaces/User';
import { Order } from '../../interfaces/Orders';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule],
  standalone: true,
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css'],
})
export class Dashboard {
  loading = false;
  resultMessage = '';
  errorMessage = '';
  currentUser: User | null = null;
  orders: Order[] = [];

  constructor(public api: ApiService, private authService: AuthService, private zone: NgZone, private cd: ChangeDetectorRef) {}

  ngOnInit() {
    this.api.login().subscribe({
      next: (user) => {
        this.currentUser = user;
      }
    });
  }

  createOrder() {
    if (!this.currentUser) {
      this.errorMessage = "User not logged in"
      return;
    }
    this.loading = true;

    this.resultMessage = '';
    this.errorMessage = '';

    this.api.createOrder(this.currentUser.id).pipe(
      finalize(() => {
        this.loading = false;
        this.cd.detectChanges();
      })
    ).subscribe({
      next: (result: any) => {
        this.resultMessage = `Order created — Transaction: ${result?.transactionId}, Status: ${result?.status}`;
        //this.getOrders();
      },
      error: () => {
        this.errorMessage = "Order failed";
      }
    });
  }
  
  getOrders() {
    if (!this.currentUser) {
      this.errorMessage = "User not logged in";
      return;
    }
    this.loading = true;
    this.resultMessage = '';
    this.errorMessage = '';

    this.api.getOrders(this.currentUser.id).pipe(
      finalize(() => {
        this.loading = false;
        this.cd.detectChanges();
      })
    ).subscribe({
      next: (result: any) => {
        if (Array.isArray(result)) {
          this.orders = result;
        } else {
          this.orders = [];
          this.resultMessage = result?.message ?? 'No orders found';
        }
      },
      error: () => {
        this.errorMessage = "Failed to fetch orders";
      }
    });
  }

  logout() {
    this.authService.logout({
      logoutParams: {
        returnTo: window.location.origin
      }
    });
  }
}
