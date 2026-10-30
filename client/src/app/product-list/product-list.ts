import { Component, inject, OnInit, signal } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Auth } from '../auth';
import { categories, Product } from '../product';
import { ProductApi } from '../product-api';

// The products page, from Exercise: an Angular Client for a Web API, with the add form moved to its own page.
//
// Requirements (Exercise: Secure the App), in product-list.html:
// - The "Add a product" link (<a id="add-link">) shows only while someone is signed in.
// - Each product's Delete button (<button class="delete">) shows only to admins.
@Component({
  imports: [CurrencyPipe, FormsModule, RouterLink],
  selector: 'app-product-list',
  templateUrl: './product-list.html',
})
export class ProductList implements OnInit {
  private readonly api = inject(ProductApi);
  protected readonly auth = inject(Auth);

  protected readonly categories = categories;
  protected readonly products = signal<Product[]>([]);
  protected readonly loading = signal(true);
  protected readonly message = signal('');
  protected readonly categoryId = signal('');

  ngOnInit() {
    this.load();
  }

  protected load() {
    this.loading.set(true);
    this.message.set('');
    const categoryId = this.categoryId() === '' ? undefined : Number(this.categoryId());
    this.api.getAll(categoryId).subscribe({
      next: products => {
        this.products.set(products);
        this.loading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.message.set(`Sorry, the products couldn't be loaded (${error.status}).`);
        this.loading.set(false);
      },
    });
  }

  protected chooseCategory(categoryId: string) {
    this.categoryId.set(categoryId);
    this.load();
  }

  protected remove(product: Product) {
    this.api.delete(product.productId).subscribe({
      next: () => this.products.update(products => products.filter(p => p.productId !== product.productId)),
      error: (error: HttpErrorResponse) => this.message.set(`Sorry, ${product.productName} couldn't be deleted (${error.status}).`),
    });
  }
}
