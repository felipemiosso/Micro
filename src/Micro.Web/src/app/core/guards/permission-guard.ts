import { inject } from '@angular/core';
import { toObservable } from '@angular/core/rxjs-interop';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../auth/auth';
import { filter, map, take } from 'rxjs';

export const permissionGuard = (resource: string, action: string): CanActivateFn => {
  return (route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);

    return toObservable(authService.isInitialized).pipe(
      filter((initialized): initialized is boolean => !!initialized),
      take(1),
      map(() => {
        if (!authService.isAuthenticated()) {
          router.navigate(['/login']);
          return false;
        }

        if (authService.hasPermission(resource, action)) {
          return true;
        }

        router.navigate(['/']); // Redirect to home if no permission
        return false;
      })
    );
  };
};
