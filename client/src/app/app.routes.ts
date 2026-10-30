import { Routes } from '@angular/router';
import { Login } from './login/login';
import { NewProduct } from './new-product/new-product';
import { ProductList } from './product-list/product-list';

// The app's pages.
//
// Requirements:
// - products/new opens only for a signed-in user. Anyone else goes to the sign-in page, with the address
//   they wanted as returnUrl: /login?returnUrl=%2Fproducts%2Fnew (signedInGuard, in signed-in-guard.ts).
export const routes: Routes = [
  { path: '', component: ProductList, title: 'Products' },
  { path: 'products/new', component: NewProduct, title: 'New product' },
  { path: 'login', component: Login, title: 'Sign in' },
  { path: '**', redirectTo: '' }
];
