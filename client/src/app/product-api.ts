import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { Observable } from 'rxjs';
import { Product } from './product';

// Calls the products API from Practice: Northwind Web API.
//
// Requirements:
// - getAll(categoryId) sends GET /api/products, or GET /api/products?categoryId=2 when a category is given.
// - add(product) sends POST /api/products with the product as JSON, and gives back the product the API
//   sends back.
// - delete(productId) sends DELETE /api/products/{productId}.
// - Each method returns the observable from HttpClient; it doesn't subscribe itself.
@Service()
export class ProductApi {
  private readonly http = inject(HttpClient);

  getAll(categoryId?: number): Observable<Product[]> {
    const url = categoryId === undefined ? '/api/products' : `/api/products?categoryId=${categoryId}`;
    return this.http.get<Product[]>(url);
  }

  add(product: Product): Observable<Product> {
    return this.http.post<Product>('/api/products', product);
  }

  delete(productId: number): Observable<void> {
    return this.http.delete<void>(`/api/products/${productId}`);
  }
}
