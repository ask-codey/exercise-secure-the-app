import { CanActivateFn } from '@angular/router';

// Lets a page open only for a signed-in user.
//
// Requirements:
// - While someone is signed in, the page opens.
// - Otherwise, the router goes to /login instead, with the address the visitor wanted as the query
//   parameter returnUrl.
export const signedInGuard: CanActivateFn = (route, state) => {
  // TODO: let signed-in users in, and send everyone else to sign in.
  return true;
};
