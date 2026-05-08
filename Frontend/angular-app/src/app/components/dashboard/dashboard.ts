import {ChangeDetectorRef, Component} from '@angular/core';
import {ApiService} from '../../services/api';
import {AuthService} from '@auth0/auth0-angular';
import { finalize } from "rxjs";
import { NgZone } from '@angular/core';
import {CommonModule} from '@angular/common';

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

  constructor(public api: ApiService, private authService: AuthService, private zone: NgZone, private cd: ChangeDetectorRef) {}


  createOrder() {
    this.loading = true;

    this.api.createOrder().pipe(
      finalize(() => {
        this.loading = false;
        this.cd.detectChanges();
      })
    ).subscribe({
      next: (result: any) => {
        this.resultMessage = result?.message || "Unknown response";
      },
      error: () => {
        this.errorMessage = "Order failed";
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
