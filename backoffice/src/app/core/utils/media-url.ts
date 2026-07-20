import { environment } from '../../../environments/environment';

/**
 * Résout une URL média (image de post, avatar) pour l'affichage.
 *
 * Les fichiers sont hébergés par l'API sous `/uploads/…`. On les ré-ancre
 * toujours sur l'**origine API configurée** — même si la base contient une
 * ancienne URL absolue avec un hôte périmé (ex. une IP LAN `http://192.168.x.x:5199`
 * gravée quand un post a été créé depuis le mobile). Cela « répare » l'affichage
 * sans migration de données. Les autres URLs absolues (images externes : picsum,
 * unsplash, pravatar…) passent telles quelles.
 */
export function resolveMediaUrl(url: string | null | undefined): string {
  if (!url) return '';

  // Origine de l'API (apiUrl est « …/api » ; les uploads sont servis à la racine).
  const origin = environment.apiUrl.replace(/\/api\/?$/, '').replace(/\/+$/, '');

  // Fichier hébergé par l'API : on ne garde que le chemin à partir de /uploads/
  // et on le rattache à l'origine courante (répare les hôtes périmés).
  const uploadsIdx = url.indexOf('/uploads/');
  if (uploadsIdx >= 0) return `${origin}${url.slice(uploadsIdx)}`;

  // URL déjà absolue et externe → inchangée.
  if (/^https?:\/\//i.test(url)) return url;

  // Chemin relatif quelconque → préfixé par l'origine API.
  return url.startsWith('/') ? `${origin}${url}` : `${origin}/${url}`;
}
