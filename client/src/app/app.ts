import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { Auth } from './auth';

// The page around every page: a header with who's signed in, and the router outlet. It's finished.
@Component({
  imports: [RouterLink, RouterOutlet],
  selector: 'app-root',
  template: `
    <header class="navbar bg-dark px-3 mb-4">
      <a class="navbar-brand text-white" routerLink="/">Northwind Products</a>
      <div class="text-white">
        @if (auth.user(); as user) {
          <span id="signed-in-as" class="me-2">Signed in as {{ user.userName }}</span>
          <button id="sign-out" class="btn btn-outline-light btn-sm" (click)="signOut()">Sign out</button>
        } @else {
          <a id="sign-in" class="btn btn-outline-light btn-sm" routerLink="/login">Sign in</a>
        }
      </div>
    </header>
    <main class="container">
      <router-outlet />
    </main>
  `,
})
export class App {
  protected readonly auth = inject(Auth);
  private readonly router = inject(Router);

  protected signOut() {
    this.auth.logout();
    this.router.navigateByUrl('/');
  }
}
