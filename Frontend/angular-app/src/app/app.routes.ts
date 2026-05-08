import { Routes } from '@angular/router';
import { Dashboard } from './components/dashboard/dashboard';
import {authGuard} from './guards/auth-guard';
import {LoginComponent} from './pages/login/login';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full',
  },
  {
    path: 'login',
    component: LoginComponent,
  },
  {
    path: 'dashboard',
    component: Dashboard,
    canActivate: [authGuard],
  }
];
