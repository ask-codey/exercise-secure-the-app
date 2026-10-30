import { computed, Service, signal } from '@angular/core';
import { EMPTY, Observable } from 'rxjs';

export interface Credentials {
  userName: string;
  password: string;
}

export interface AuthResult {
  token: string;
  userName: string;
  role: string;
}

// Signs users in and out, and remembers who's signed in.
//
// Requirements:
// - login(credentials) sends POST /api/auth/login with the credentials, and, when it works, saves the
//   AuthResult the API sends back: in the user signal, and in localStorage under the key 'northwind.auth',
//   as JSON. It returns the observable, so the sign-in page can subscribe.
// - When the service starts, it reads a saved AuthResult from localStorage, so a reload keeps you signed in.
// - user is the signed-in AuthResult, or null. isSignedIn and isAdmin (role "Admin") follow it.
// - token is the signed-in user's token, or null.
// - logout() forgets the user: from the signal and from localStorage.
const storageKey = 'northwind.auth';

@Service()
export class Auth {
  private readonly current = signal<AuthResult | null>(null);

  readonly user = this.current.asReadonly();
  readonly isSignedIn = computed(() => this.current() !== null);
  readonly isAdmin = computed(() => this.current()?.role === 'Admin');

  get token(): string | null {
    return this.current()?.token ?? null;
  }

  login(credentials: Credentials): Observable<AuthResult> {
    // TODO: send the credentials, and save the result.
    return EMPTY;
  }

  logout() {
    // TODO: forget the user.
  }
}
