import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class IdentityAuthService {
  readonly isLoggedIn = signal(false);

  logIn(): void {
    this.isLoggedIn.set(true);
  }

  logOut(): void {
    this.isLoggedIn.set(false);
  }

  setLoggedIn(isLoggedIn: boolean): void {
    this.isLoggedIn.set(isLoggedIn);
  }
}
