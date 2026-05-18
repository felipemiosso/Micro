import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth-guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login').then(m => m.LoginComponent)
  },
  {
    path: 'jobs',
    loadComponent: () => import('./features/job-postings/public-board/job-board').then(m => m.JobBoardComponent)
  },
  {
    path: 'jobs/:id',
    loadComponent: () => import('./features/job-postings/public-detail/job-detail').then(m => m.JobDetailComponent)
  },
  {
    path: 'jobs/:id/apply',
    loadComponent: () => import('./features/applications/apply/apply').then(m => m.ApplyComponent)
  },
  {
    path: 'jobs/apply/success',
    loadComponent: () => import('./features/applications/success/success').then(m => m.ApplicationSuccessComponent)
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
      },
      {
        path: 'jobs',
        loadComponent: () => import('./features/job-postings/admin-list/job-posting-list').then(m => m.JobPostingListComponent)
      },
      {
        path: 'jobs/edit/:id',
        loadComponent: () => import('./features/job-postings/admin-edit/job-posting-edit').then(m => m.JobPostingEditComponent)
      },
      {
        path: 'candidates',
        loadComponent: () => import('./features/applications/list/application-list').then(m => m.ApplicationListComponent)
      },
      {
        path: 'applications',
        loadComponent: () => import('./features/applications/applications-board/applications-board').then(m => m.ApplicationsBoardComponent)
      },
      {
        path: 'applications/archived',
        loadComponent: () => import('./features/applications/archived-applications/archived-applications').then(m => m.ArchivedApplicationsComponent)
      },
      {
        path: 'candidates/:id',
        loadComponent: () => import('./features/applications/detail/application-detail').then(m => m.ApplicationDetailComponent)
      },
      {
        path: 'profile',
        loadComponent: () => import('./features/auth/profile/profile').then(m => m.ProfileComponent)
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
