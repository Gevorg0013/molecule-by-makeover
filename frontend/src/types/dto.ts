import type { DiscountType, LanguageCode, OrderStatus, PaymentStatus } from './enums'

export interface PaginatedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface ApiError {
  status: number
  title: string
  detail: string | null
  errors?: Record<string, string[]>
}

// ---------- Auth ----------

export interface UserProfileDto {
  id: string
  email: string
  firstName: string
  lastName: string
  phoneNumber: string | null
  roles: string[]
}

export interface AuthResponse {
  accessToken: string
  expiresAt: string
  user: UserProfileDto
}

export interface MeResponse {
  userId: string
  email: string
  isAdmin: boolean
}

export interface RegisterRequest {
  email: string
  password: string
  firstName: string
  lastName: string
  phoneNumber?: string
  preferredLanguageCode?: LanguageCode
}

export interface LoginRequest {
  email: string
  password: string
}

// ---------- Catalog ----------

export interface CategoryDto {
  id: string
  parentCategoryId: string | null
  name: string
  slug: string
  description: string | null
  imageUrl: string | null
  sortOrder: number
  metaTitle: string | null
  metaDescription: string | null
  children: CategoryDto[]
}

export interface ProductImageDto {
  id: string
  url: string
  altText: string | null
  sortOrder: number
  isPrimary: boolean
}

export interface ProductListItemDto {
  id: string
  sku: string
  name: string
  slug: string
  shortDescription: string | null
  price: number
  finalPrice: number
  discountType: DiscountType
  discountValue: number | null
  mainImageUrl: string | null
  stock: number
  brand: string | null
  isFeatured: boolean
  isBestSeller: boolean
  categoryId: string
  categoryName: string
  tags: string[]
  averageRating: number
  reviewCount: number
}

export interface ProductDetailDto {
  id: string
  sku: string
  name: string
  slug: string
  shortDescription: string | null
  description: string | null
  ingredients: string | null
  usageInstructions: string | null
  benefits: string | null
  price: number
  finalPrice: number
  discountType: DiscountType
  discountValue: number | null
  mainImageUrl: string | null
  stock: number
  brand: string | null
  categoryId: string
  categoryName: string
  tags: string[]
  images: ProductImageDto[]
  metaTitle: string | null
  metaDescription: string | null
  averageRating: number
  reviewCount: number
}

export interface ProductQueryParams {
  page?: number
  pageSize?: number
  categorySlug?: string
  minPrice?: number
  maxPrice?: number
  brand?: string
  tag?: string
  search?: string
  featuredOnly?: boolean
  bestSellerOnly?: boolean
  sort?: number
}

// ---------- Reviews ----------

export interface ReviewDto {
  id: string
  productId: string
  userId: string
  reviewerName: string
  rating: number
  comment: string | null
  isApproved: boolean
  createdAt: string
}

export interface CreateReviewRequest {
  rating: number
  comment?: string
}

// ---------- Content ----------

export interface BlogPostListItemDto {
  id: string
  title: string
  slug: string
  excerpt: string | null
  coverImageUrl: string | null
  publishedAt: string | null
}

export interface BlogPostDetailDto {
  id: string
  title: string
  slug: string
  excerpt: string | null
  content: string
  coverImageUrl: string | null
  metaTitle: string | null
  metaDescription: string | null
  publishedAt: string | null
}

export interface PageDto {
  key: string
  title: string
  slug: string
  content: string
  metaTitle: string | null
  metaDescription: string | null
}

// ---------- Marketing ----------

export interface BannerDto {
  id: string
  imageUrl: string
  linkUrl: string | null
  title: string | null
  subtitle: string | null
  ctaText: string | null
}

// ---------- Cart ----------

export interface CartItemDto {
  id: string
  productId: string
  productName: string
  slug: string
  imageUrl: string | null
  unitPrice: number
  quantity: number
  lineTotal: number
  availableStock: number
}

export interface CartDto {
  id: string
  items: CartItemDto[]
  subTotal: number
  discountTotal: number
  grandTotal: number
  couponCode: string | null
  currency: string
}

// ---------- Checkout / Orders ----------

export interface Address {
  fullName: string
  phone: string
  country: string
  city: string
  addressLine1: string
  addressLine2?: string | null
  postalCode?: string | null
}

export interface CheckoutRequest {
  shippingAddress: Address
  paymentProviderKey: string
}

export interface CheckoutResultDto {
  orderNumber: string
  clientSecret: string | null
  redirectUrl: string | null
  grandTotal: number
  currency: string
}

export interface OrderSummaryDto {
  id: string
  orderNumber: string
  status: OrderStatus
  paymentStatus: PaymentStatus
  grandTotal: number
  currency: string
  placedAt: string
}

export interface OrderItemDto {
  id: string
  productId: string | null
  productName: string
  sku: string
  unitPrice: number
  quantity: number
  lineTotal: number
}

export interface OrderDetailDto {
  id: string
  orderNumber: string
  status: OrderStatus
  paymentStatus: PaymentStatus
  subTotal: number
  discountTotal: number
  shippingTotal: number
  grandTotal: number
  currency: string
  shippingAddress: Address
  paymentProvider: string
  placedAt: string
  items: OrderItemDto[]
}

// ---------- Wishlist ----------

export interface WishlistItemDto {
  productId: string
  name: string
  slug: string
  imageUrl: string | null
  price: number
  finalPrice: number
  inStock: boolean
  addedAt: string
}

export interface WishlistDto {
  items: WishlistItemDto[]
}

// ---------- Admin: shared translation shapes ----------

export interface TranslationInputBase {
  languageCode: LanguageCode
}

export interface ProductTranslationDto {
  id: string
  languageCode: LanguageCode
  name: string
  slug: string
  shortDescription: string | null
  description: string | null
  ingredients: string | null
  usageInstructions: string | null
  benefits: string | null
  metaTitle: string | null
  metaDescription: string | null
}

export interface ProductTranslationInput {
  languageCode: LanguageCode
  name: string
  slug: string
  shortDescription?: string | null
  description?: string | null
  ingredients?: string | null
  usageInstructions?: string | null
  benefits?: string | null
  metaTitle?: string | null
  metaDescription?: string | null
}

export interface ProductAdminDto {
  id: string
  sku: string
  categoryId: string
  price: number
  discountType: DiscountType
  discountValue: number | null
  stock: number
  mainImageUrl: string | null
  brand: string | null
  isFeatured: boolean
  isBestSeller: boolean
  isActive: boolean
  tags: string[]
  images: ProductImageDto[]
  translations: ProductTranslationDto[]
}

export interface ProductUpsertRequest {
  sku: string
  categoryId: string
  price: number
  discountType: DiscountType
  discountValue?: number | null
  stock: number
  mainImageUrl?: string | null
  brand?: string | null
  isFeatured: boolean
  isBestSeller: boolean
  isActive: boolean
  tags: string[]
  translations: ProductTranslationInput[]
}

export interface CategoryTranslationDto {
  id: string
  languageCode: LanguageCode
  name: string
  slug: string
  description: string | null
  metaTitle: string | null
  metaDescription: string | null
}

export interface CategoryTranslationInput {
  languageCode: LanguageCode
  name: string
  slug: string
  description?: string | null
  metaTitle?: string | null
  metaDescription?: string | null
}

export interface CategoryAdminDto {
  id: string
  parentCategoryId: string | null
  imageUrl: string | null
  sortOrder: number
  isActive: boolean
  translations: CategoryTranslationDto[]
}

export interface CategoryUpsertRequest {
  parentCategoryId?: string | null
  imageUrl?: string | null
  sortOrder: number
  isActive: boolean
  translations: CategoryTranslationInput[]
}

export interface CouponDto {
  id: string
  code: string
  discountType: DiscountType
  discountValue: number
  minOrderAmount: number | null
  maxUses: number | null
  usesCount: number
  startsAt: string | null
  expiresAt: string | null
  isActive: boolean
}

export interface CouponUpsertRequest {
  code: string
  discountType: DiscountType
  discountValue: number
  minOrderAmount?: number | null
  maxUses?: number | null
  startsAt?: string | null
  expiresAt?: string | null
  isActive: boolean
}

export interface CustomerSummaryDto {
  id: string
  email: string
  firstName: string
  lastName: string
  isActive: boolean
  createdAt: string
  orderCount: number
}

export interface CustomerDetailDto {
  id: string
  email: string
  firstName: string
  lastName: string
  phoneNumber: string | null
  isActive: boolean
  createdAt: string
}

export interface UpdateOrderStatusRequest {
  status: OrderStatus
}

export interface BannerTranslationDto {
  id: string
  languageCode: LanguageCode
  title: string | null
  subtitle: string | null
  ctaText: string | null
}

export interface BannerTranslationInput {
  languageCode: LanguageCode
  title?: string | null
  subtitle?: string | null
  ctaText?: string | null
}

export interface BannerAdminDto {
  id: string
  imageUrl: string
  linkUrl: string | null
  sortOrder: number
  isActive: boolean
  startsAt: string | null
  endsAt: string | null
  translations: BannerTranslationDto[]
}

export interface BannerUpsertRequest {
  imageUrl: string
  linkUrl?: string | null
  sortOrder: number
  isActive: boolean
  startsAt?: string | null
  endsAt?: string | null
  translations: BannerTranslationInput[]
}

export interface BlogTranslationDto {
  id: string
  languageCode: LanguageCode
  title: string
  slug: string
  excerpt: string | null
  content: string
  metaTitle: string | null
  metaDescription: string | null
}

export interface BlogTranslationInput {
  languageCode: LanguageCode
  title: string
  slug: string
  excerpt?: string | null
  content: string
  metaTitle?: string | null
  metaDescription?: string | null
}

export interface BlogPostAdminDto {
  id: string
  coverImageUrl: string | null
  isPublished: boolean
  publishedAt: string | null
  translations: BlogTranslationDto[]
}

export interface BlogPostUpsertRequest {
  coverImageUrl?: string | null
  isPublished: boolean
  translations: BlogTranslationInput[]
}

export interface PageTranslationDto {
  id: string
  languageCode: LanguageCode
  title: string
  slug: string
  content: string
  metaTitle: string | null
  metaDescription: string | null
}

export interface PageTranslationInput {
  languageCode: LanguageCode
  title: string
  slug: string
  content: string
  metaTitle?: string | null
  metaDescription?: string | null
}

export interface PageAdminDto {
  id: string
  key: string
  isPublished: boolean
  translations: PageTranslationDto[]
}

export interface PageUpsertRequest {
  key: string
  isPublished: boolean
  translations: PageTranslationInput[]
}

export interface GalleryImageDto {
  id: string
  url: string
  altText: string | null
  fileName: string
  sizeBytes: number
  mimeType: string
  createdAt: string
}

export interface SettingDto {
  key: string
  value: string | null
  group: string
}

export interface UpdateSettingRequest {
  value?: string | null
}

export interface LanguageDto {
  id: number
  code: string
  name: string
  isDefault: boolean
  isActive: boolean
}

export interface LanguageUpsertRequest {
  code: string
  name: string
  isDefault: boolean
  isActive: boolean
}

export interface TopProductDto {
  productId: string
  name: string
  unitsSold: number
  revenue: number
}

export interface DashboardStatsDto {
  totalRevenue: number
  totalOrders: number
  pendingOrders: number
  totalCustomers: number
  totalProducts: number
  lowStockProducts: number
  topProducts: TopProductDto[]
}
