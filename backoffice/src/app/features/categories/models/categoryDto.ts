/** Catégorie d'activités (GET /api/admin/categories). */
export interface CategoryDto {
  id: string;
  slug: string;
  label: string;
  color?: string | null;
  icon?: string | null;
}
