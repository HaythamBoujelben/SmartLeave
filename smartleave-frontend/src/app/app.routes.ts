import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { authGuard } from './core/services/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  {
    path: 'my-requests',
    loadComponent: () => import('./features/leave-requests/my-requests/my-requests.component')
      .then(m => m.MyRequestsComponent),
    canActivate: [authGuard]
  },
  {
    path: 'pending-requests',
    loadComponent: () => import('./features/leave-requests/pending-requests/pending-requests.component')
      .then(m => m.PendingRequestsComponent),
    canActivate: [authGuard]
  },
  { path: '**', redirectTo: 'login' }
];
