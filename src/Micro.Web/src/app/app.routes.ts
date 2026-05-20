import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth-guard';
import { publicGuard } from './core/guards/public-guard';
import { permissionGuard } from './core/guards/permission-guard';
import { roleGuard } from './core/guards/role-guard';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [publicGuard],
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
    path: '',
    canActivate: [authGuard],
    children: [
      {
        path: '',
        redirectTo: 'requisitions',
        pathMatch: 'full'
      },
      {
        path: 'requisitions',
        canActivate: [permissionGuard('Requisition', 'View')],
        loadComponent: () => import('./features/requisitions/list/requisition-list').then(m => m.RequisitionListComponent)
      },
      {
        path: 'requisitions/new',
        canActivate: [permissionGuard('Requisition', 'Create')],
        loadComponent: () => import('./features/requisitions/form/requisition-form').then(m => m.RequisitionFormComponent)
      },
      {
        path: 'requisitions/edit/:id',
        canActivate: [permissionGuard('Requisition', 'Edit')],
        loadComponent: () => import('./features/requisitions/form/requisition-form').then(m => m.RequisitionFormComponent)
      },
      {
        path: 'job-management',
        canActivate: [permissionGuard('JobPosting', 'View')],
        loadComponent: () => import('./features/job-postings/admin-list/job-posting-list').then(m => m.JobPostingListComponent)
      },
      {
        path: 'job-management/edit/:id',
        canActivate: [permissionGuard('JobPosting', 'Edit')],
        loadComponent: () => import('./features/job-postings/admin-edit/job-posting-edit').then(m => m.JobPostingEditComponent)
      },
      {
        path: 'candidates',
        canActivate: [permissionGuard('Application', 'View')],
        loadComponent: () => import('./features/applications/list/application-list').then(m => m.ApplicationListComponent)
      },
      {
        path: 'candidates/:id',
        canActivate: [permissionGuard('Application', 'View')],
        loadComponent: () => import('./features/applications/detail/application-detail').then(m => m.ApplicationDetailComponent)
      },
      {
        path: 'applications',
        canActivate: [permissionGuard('Application', 'View')],
        loadComponent: () => import('./features/applications/applications-board/applications-board').then(m => m.ApplicationsBoardComponent)
      },
      {
        path: 'applications/archived',
        canActivate: [permissionGuard('Application', 'View')],
        loadComponent: () => import('./features/applications/archived-applications/archived-applications').then(m => m.ArchivedApplicationsComponent)
      },
      {
        path: 'profile',
        loadComponent: () => import('./features/auth/profile/profile').then(m => m.ProfileComponent)
      },
      {
        path: 'roles',
        canActivate: [permissionGuard('Role', 'View')],
        loadComponent: () => import('./features/roles/list/role-list').then(m => m.RoleListComponent)
      },
      {
        path: 'roles/new',
        canActivate: [permissionGuard('Role', 'Create')],
        loadComponent: () => import('./features/roles/form/role-editor').then(m => m.RoleEditorComponent)
      },
      {
        path: 'users',
        canActivate: [permissionGuard('User', 'View')],
        loadComponent: () => import('./features/users/list/user-list').then(m => m.UserListComponent)
      },
      {
        path: 'admin/settings',
        canActivate: [roleGuard('Admin')],
        loadComponent: () => import('./features/admin/admin-settings').then(m => m.AdminSettingsComponent)
      }
    ]
  }
];
