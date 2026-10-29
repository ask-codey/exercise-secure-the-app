import { TestBed } from '@angular/core/testing';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { appConfig } from './app.config';
import { Auth, AuthResult } from './auth';

// These tests run the app with its own app.config.ts, and a pretend API: HttpTestingController catches each
// request, and the test answers it. So they need no API running. Run them with: npm test.
// You don't need to change this file.

const ada: AuthResult = { token: 'ada.token.signature', userName: 'ada', role: 'User' };
const grace: AuthResult = { token: 'grace.token.signature', userName: 'grace', role: 'Admin' };
const chai = { productId: 1, productName: 'Chai', categoryId: 1, unitPrice: 18, unitsInStock: 39, discontinued: false };

let http: HttpTestingController;
let harness: RouterTestingHarness;
let router: Router;

async function start(signedInAs: AuthResult | null) {
  localStorage.clear();
  if (signedInAs) {
    localStorage.setItem('northwind.auth', JSON.stringify(signedInAs));
  }
  TestBed.configureTestingModule({ providers: [...appConfig.providers, provideHttpClientTesting()] });
  http = TestBed.inject(HttpTestingController);
  router = TestBed.inject(Router);
  harness = await RouterTestingHarness.create();
}

const page = () => harness.routeNativeElement as HTMLElement;

async function type(selector: string, value: string) {
  const input = page().querySelector(selector) as HTMLInputElement;
  expect(input, `an input ${selector} on the page`).toBeTruthy();
  input.value = value;
  input.dispatchEvent(new Event('input'));
  await harness.fixture.whenStable();
}

async function showProducts() {
  await harness.navigateByUrl('/');
  http.expectOne('/api/products').flush([chai]);
  await harness.fixture.whenStable();
}

afterEach(() => localStorage.clear());

describe('Auth', () => {
  it('signs in with the API, and remembers the user in a signal and in localStorage', async () => {
    await start(null);
    const auth = TestBed.inject(Auth);
    let result: AuthResult | undefined;
    auth.login({ userName: 'ada', password: 'summer2026' }).subscribe(r => (result = r));
    const request = http.expectOne('/api/auth/login');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ userName: 'ada', password: 'summer2026' });
    request.flush(ada);
    expect(result).toEqual(ada);
    expect(auth.user()).toEqual(ada);
    expect(auth.isSignedIn()).toBe(true);
    expect(auth.token).toBe('ada.token.signature');
    expect(JSON.parse(localStorage.getItem('northwind.auth') ?? 'null')).toEqual(ada);
  });

  it('reads a saved user when it starts, and forgets it on logout', async () => {
    await start(grace);
    const auth = TestBed.inject(Auth);
    expect(auth.user()?.userName).toBe('grace');
    expect(auth.isAdmin()).toBe(true);
    auth.logout();
    expect(auth.isSignedIn()).toBe(false);
    expect(auth.token).toBeNull();
    expect(localStorage.getItem('northwind.auth')).toBeNull();
  });
});

describe('Login', () => {
  it("says so when the user name and password don't match", async () => {
    await start(null);
    await harness.navigateByUrl('/login');
    await type('#user-name', 'ada');
    await type('#password', 'wrong');
    (page().querySelector('#sign-in-button') as HTMLButtonElement).click();
    await harness.fixture.whenStable();
    http.expectOne('/api/auth/login').flush("That user name and password don't match.", { status: 401, statusText: 'Unauthorized' });
    await harness.fixture.whenStable();
    expect(page().querySelector('#error')?.textContent?.trim()).toBe("That user name and password don't match.");
    expect(TestBed.inject(Auth).isSignedIn()).toBe(false);
  });

  it('goes home after signing in, when there is no returnUrl', async () => {
    await start(null);
    await harness.navigateByUrl('/login');
    await type('#user-name', 'ada');
    await type('#password', 'summer2026');
    (page().querySelector('#sign-in-button') as HTMLButtonElement).click();
    await harness.fixture.whenStable();
    http.expectOne('/api/auth/login').flush(ada);
    await harness.fixture.whenStable();
    expect(router.url).toBe('/');
    expect(TestBed.inject(Auth).user()?.userName).toBe('ada');
  });
});

describe('signedInGuard', () => {
  it('sends a visitor who is not signed in to sign in, and back to the page afterwards', async () => {
    await start(null);
    await harness.navigateByUrl('/products/new');
    expect(router.url).toBe('/login?returnUrl=%2Fproducts%2Fnew');
    await type('#user-name', 'ada');
    await type('#password', 'summer2026');
    (page().querySelector('#sign-in-button') as HTMLButtonElement).click();
    await harness.fixture.whenStable();
    http.expectOne('/api/auth/login').flush(ada);
    await harness.fixture.whenStable();
    expect(router.url).toBe('/products/new');
  });

  it('lets a signed-in user in, and not after they sign out', async () => {
    await start(ada);
    await harness.navigateByUrl('/products/new');
    expect(router.url).toBe('/products/new');
    expect(page().querySelector('#add-product')).toBeTruthy();
    TestBed.inject(Auth).logout();
    await harness.navigateByUrl('/login');
    await harness.navigateByUrl('/products/new');
    expect(router.url).toBe('/login?returnUrl=%2Fproducts%2Fnew');
  });
});

describe('authInterceptor', () => {
  it('adds the token to /api requests while someone is signed in, and only then', async () => {
    await start(ada);
    const client = TestBed.inject(HttpClient);
    client.get('/api/products').subscribe();
    expect(http.expectOne('/api/products').request.headers.get('Authorization')).toBe('Bearer ada.token.signature');
    client.get('https://example.com/prices').subscribe();
    expect(http.expectOne('https://example.com/prices').request.headers.has('Authorization')).toBe(false);
    TestBed.inject(Auth).logout();
    client.get('/api/products').subscribe();
    expect(http.expectOne('/api/products').request.headers.has('Authorization')).toBe(false);
  });

  it('signs out and goes to sign in when the API answers 401', async () => {
    await start(ada);
    await harness.navigateByUrl('/products/new');
    let status: number | undefined;
    TestBed.inject(HttpClient).post('/api/products', {}).subscribe({ error: (e: HttpErrorResponse) => (status = e.status) });
    http.expectOne('/api/products').flush(null, { status: 401, statusText: 'Unauthorized' });
    await harness.fixture.whenStable();
    expect(status).toBe(401);
    expect(TestBed.inject(Auth).isSignedIn()).toBe(false);
    expect(router.url).toBe('/login?returnUrl=%2Fproducts%2Fnew');
  });
});

describe('ProductList', () => {
  it('shows neither the Add link nor Delete buttons to someone signed out', async () => {
    await start(null);
    await showProducts();
    expect(page().querySelectorAll('tr.product').length).toBe(1);
    expect(page().querySelector('#add-link')).toBeNull();
    expect(page().querySelector('button.delete')).toBeNull();
  });

  it('shows the Add link to users, and Delete buttons only to admins', async () => {
    await start(ada);
    await showProducts();
    expect(page().querySelector('#add-link')).toBeTruthy();
    expect(page().querySelector('button.delete')).toBeNull();

    TestBed.resetTestingModule();
    await start(grace);
    await showProducts();
    expect(page().querySelector('#add-link')).toBeTruthy();
    expect(page().querySelectorAll('button.delete').length).toBe(1);
  });
});
