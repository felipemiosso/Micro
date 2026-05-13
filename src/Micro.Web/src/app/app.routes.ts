import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth-guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login').then(m => m.LoginComponent)
  },
  {
    path: 'admin',
    canActivate: [authGuard],
    children: [
      {
        path: 'requisitions',
        loadComponent: () => import('./features/requisitions/list/requisition-list').then(m => m.RequisitionListComponent)
      },
      {
        path: 'requisitions/new',
        loadComponent: () => import('./features/requisitions/form/requisition-form').then(m => m.RequisitionFormComponent)
      },
      {
        path: 'requisitions/edit/:id',
        loadComponent: () => import('./features/requisitions/form/requisition-form').then(m => m.RequisitionFormComponent)
      }
    ]
  },
  {
    path: '',
    canActivate: [authGuard],
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/requisitions/list/requisition-list').then(m => m.RequisitionListComponent)
      }
    ]
  }
];
