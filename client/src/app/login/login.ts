import { Component } from '@angular/core';

// The sign-in page, at /login.
//
// Requirements:
// - A form with <input id="user-name">, <input id="password" type="password"> and
//   <button id="sign-in-button">. Submitting it signs in with Auth's login.
// - When signing in works, go to the address in the query parameter returnUrl, or to / when there's none.
// - When the API answers 401, <p id="error"> says "That user name and password don't match.".
//   Any other failure says "Sorry, signing in failed (500)." with the status.
@Component({
  imports: [],
  selector: 'app-login',
  templateUrl: './login.html',
})
export class Login {
  // TODO: the page's inputs, signals and signIn method.
}
