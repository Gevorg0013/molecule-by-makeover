export const OrderStatus = {
  Pending: 0,
  Processing: 1,
  Shipped: 2,
  Delivered: 3,
  Completed: 4,
  Cancelled: 5,
} as const
export type OrderStatus = (typeof OrderStatus)[keyof typeof OrderStatus]

export const orderStatusLabels: Record<OrderStatus, string> = {
  [OrderStatus.Pending]: 'Pending',
  [OrderStatus.Processing]: 'Processing',
  [OrderStatus.Shipped]: 'Shipped',
  [OrderStatus.Delivered]: 'Delivered',
  [OrderStatus.Completed]: 'Completed',
  [OrderStatus.Cancelled]: 'Cancelled',
}

/** Allowed forward transitions the API itself does not enforce. */
export const orderStatusTransitions: Record<OrderStatus, OrderStatus[]> = {
  [OrderStatus.Pending]: [OrderStatus.Processing, OrderStatus.Cancelled],
  [OrderStatus.Processing]: [OrderStatus.Shipped, OrderStatus.Cancelled],
  [OrderStatus.Shipped]: [OrderStatus.Delivered, OrderStatus.Cancelled],
  [OrderStatus.Delivered]: [OrderStatus.Completed],
  [OrderStatus.Completed]: [],
  [OrderStatus.Cancelled]: [],
}

export const PaymentStatus = {
  Pending: 0,
  Paid: 1,
  Failed: 2,
  Refunded: 3,
} as const
export type PaymentStatus = (typeof PaymentStatus)[keyof typeof PaymentStatus]

export const paymentStatusLabels: Record<PaymentStatus, string> = {
  [PaymentStatus.Pending]: 'Pending',
  [PaymentStatus.Paid]: 'Paid',
  [PaymentStatus.Failed]: 'Failed',
  [PaymentStatus.Refunded]: 'Refunded',
}

export const DiscountType = {
  None: 0,
  Percent: 1,
  Fixed: 2,
} as const
export type DiscountType = (typeof DiscountType)[keyof typeof DiscountType]

export const discountTypeLabels: Record<DiscountType, string> = {
  [DiscountType.None]: 'None',
  [DiscountType.Percent]: 'Percent',
  [DiscountType.Fixed]: 'Fixed',
}

export const ProductSort = {
  Newest: 0,
  PriceAsc: 1,
  PriceDesc: 2,
  BestSelling: 3,
} as const
export type ProductSort = (typeof ProductSort)[keyof typeof ProductSort]

export const productSortLabels: Record<ProductSort, string> = {
  [ProductSort.Newest]: 'Newest',
  [ProductSort.PriceAsc]: 'Price: Low to High',
  [ProductSort.PriceDesc]: 'Price: High to Low',
  [ProductSort.BestSelling]: 'Best Selling',
}

export const LANGUAGES = ['en', 'hy', 'ru'] as const
export type LanguageCode = (typeof LANGUAGES)[number]

export const RoleNames = {
  Admin: 'Admin',
  Customer: 'Customer',
} as const
