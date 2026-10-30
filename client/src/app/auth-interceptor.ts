import { HttpInterceptorFn } from '@angular/common/http';

// Sees every request HttpClient sends.
//
// Requirements:
// - While someone is signed in, a request to an address starting with /api/ gets the header
//   Authorization: Bearer <their token>. Other requests, and every request while nobody is signed in,
//   go on unchanged.
// - When the API answers a request that carried the token with 401, sign the user out and go to the sign-in
//   page, with the current address as returnUrl. The error still reaches whoever sent the request.
export const authInterceptor: HttpInterceptorFn = (request, next) => {
  // TODO: add the token, and handle a 401.
  return next(request);
};
