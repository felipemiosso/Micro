import { inject } from '@angular/core';
import { toObservable } from '@angular/core/rxjs-interop';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../auth/auth';
import { filter, map, take } from 'rxjs';

export const publicGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return toObservable(authService.isInitialized).pipe(
    filter((initialized): initialized is boolean => !!initialized),
    take(1),
    map(() => {
      if (authService.isAuthenticated()) {
        router.navigate(['/dashboard']);
        return false;
      }
      return true;
    })
  );
};
