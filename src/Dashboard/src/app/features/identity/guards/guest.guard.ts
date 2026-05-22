import { inject } from '@angular/core';
import { CanMatchFn, Router } from '@angular/router';

import { IdentityAuthService } from '../services/identity-auth.service';

export const guestGuard: CanMatchFn = () => {
  const authService = inject(IdentityAuthService);

  if (!authService.isLoggedIn()) {
    return true;
  }

  return inject(Router).parseUrl('/');
};
