/** Payload create/update d'une catégorie (POST / PATCH /api/admin/categories). */
export interface CategoryRequest {
  slug: string;
  label: string;
  color?: string | null;
  icon?: string | null;
}
