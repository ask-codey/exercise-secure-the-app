import { Component, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { categories, Product } from '../product';
import { ProductApi } from '../product-api';

// The add form, moved to its own page at /products/new. It's finished.
@Component({
  imports: [FormsModule, RouterLink],
  selector: 'app-new-product',
  template: `
    <h1 class="h4">Add a product</h1>
    <form (ngSubmit)="add()" style="max-width: 24rem">
      <input id="new-name" name="name" class="form-control mb-2" placeholder="Name" [(ngModel)]="name">
      <input id="new-price" name="price" type="number" step="0.01" class="form-control mb-2" placeholder="Price" [(ngModel)]="price">
      <select id="new-category" name="category" class="form-select mb-2" [(ngModel)]="categoryId">
        @for (category of categories; track category.categoryId) {
          <option [value]="category.categoryId">{{ category.categoryName }}</option>
        }
      </select>
      @if (message()) {
        <p id="message" class="text-danger">{{ message() }}</p>
      }
      <button id="add-product" class="btn btn-primary">Add</button>
      <a class="btn btn-link" routerLink="/">Cancel</a>
    </form>
  `,
})
export class NewProduct {
  private readonly api = inject(ProductApi);
  private readonly router = inject(Router);

  protected readonly categories = categories;
  protected readonly name = signal('');
  protected readonly price = signal<number | null>(null);
  protected readonly categoryId = signal('1');
  protected readonly message = signal('');

  protected add() {
    const name = this.name().trim();
    if (name === '') {
      this.message.set('Give the product a name.');
      return;
    }

    const product: Product = {
      productId: 0,
      productName: name,
      categoryId: Number(this.categoryId()),
      unitPrice: this.price(),
      unitsInStock: 0,
      discontinued: false,
    };
    this.api.add(product).subscribe({
      next: () => this.router.navigateByUrl('/'),
      error: (error: HttpErrorResponse) => this.message.set(`Sorry, ${name} couldn't be added (${error.status}).`),
    });
  }
}
