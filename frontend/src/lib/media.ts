const API_ORIGIN = new URL(import.meta.env.VITE_API_BASE_URL || 'http://localhost:5116/api/v1').origin

/**
 * Backend-uploaded media (banners, product/gallery images) comes back as a path relative to the
 * API host (e.g. "/uploads/gallery/x.jpg"), not the frontend origin. Resolve it against the API
 * so it loads correctly regardless of which port/host is serving the SPA.
 */
export function resolveMediaUrl(url: string | null | undefined): string | null {
  if (!url) return null
  if (/^https?:\/\//i.test(url)) return url
  return `${API_ORIGIN}${url.startsWith('/') ? '' : '/'}${url}`
}
