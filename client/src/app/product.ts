// A product, shaped like the products API's JSON. It's finished.
export interface Product {
  productId: number;
  productName: string;
  categoryId: number | null;
  unitPrice: number | null;
  unitsInStock: number | null;
  discontinued: boolean;
}

// Northwind's categories, for the filter and the form. It's finished.
export const categories = [
  { categoryId: 1, categoryName: 'Beverages' },
  { categoryId: 2, categoryName: 'Condiments' },
  { categoryId: 3, categoryName: 'Confections' },
  { categoryId: 4, categoryName: 'Dairy Products' },
  { categoryId: 5, categoryName: 'Grains/Cereals' },
  { categoryId: 6, categoryName: 'Meat/Poultry' },
  { categoryId: 7, categoryName: 'Produce' },
  { categoryId: 8, categoryName: 'Seafood' }
];
