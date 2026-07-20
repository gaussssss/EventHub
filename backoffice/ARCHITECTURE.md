# EventHub, Back Office · Architecture & conventions

Doc de référence pour l'implémentation du back-office Angular. Elle **transpose
les patterns du projet `odtrCampagn.Client`** (Angular 20, standalone, état par
`signal` + `inject`, Clean Architecture par feature) au contexte EventHub.

---

## 1. Stack

| Élément            | Choix                                                        |
|--------------------|-------------------------------------------------------------|
| Framework          | **Angular 20.2**, **standalone** (pas de NgModule)          |
| Détection          | Zone.js + `provideZoneChangeDetection({ eventCoalescing })` |
| État               | **`signal` maison** (pas de NgRx/store externe)             |
| HTTP               | `HttpClient` + `withInterceptors([...])`                    |
| Routing            | `provideRouter` + **lazy `loadComponent`**                  |
| Styles             | **Tailwind v4** + **daisyUI** (via `@tailwindcss/postcss`)  |
| API                | EventHub Web API, base `http://localhost:5199/api`         |
| Lint/format        | Prettier (`printWidth: 100`, `singleQuote: true`)           |

---

## 2. Arborescence (par feature)

```
src/app/
  app.ts                      # shell : <router-outlet />
  app.config.ts               # providers (router, http+interceptors, zone)
  app.routes.ts               # routes lazy + guard + layout à enfants

  core/                       # transverse, singletons
    guards/          auth.ts          # CanActivateFn
    interceptors/    auth.ts          # HttpInterceptorFn (Bearer)
    services/        <infra globale>  # ex. auth/session

  shared/                     # réutilisable inter-features
    components/      layout/  navbar/  toast/  toast-list/
    models/          enums/   interfaces/
    services/infrastructure/states/    toastStates.ts  modalStates.ts

  features/<feature>/
    models/                          <x>Dto.ts   create<X>Request.ts
    services/
      application/                   loadXs.ts  createX.ts  ... (1 use-case = 1 fichier)
      infrastructure/
        repository/  <x>.ts          # appels HTTP (Observable)
        states/      <x>States.ts    # store signal
    components/      <x>-list/  <x>-detail/  modals/<...>/
```

**Nommage des fichiers** : kebab pour les composants (`campaign-list.ts/.html/.scss`),
camelCase pour use-cases/états/modèles (`loadCampaigns.ts`, `campaignStates.ts`,
`campaignDto.ts`). Classes en PascalCase (`CampaignList`, `LoadCampaigns`,
`CampaignStates`, `CampaignService`).

---

## 3. Couches d'une feature

### 3.1 Modèle (`models/`)
Interfaces plates, une par contrat. `xDto.ts` = forme renvoyée par l'API ;
`createXRequest.ts` = payload d'écriture.

```ts
export interface CampaignDto { id: string; name: string; /* ... */ }
export interface CreateCampaignRequest { name: string; subject: string; }
```

### 3.2 Repository (`infrastructure/repository/<x>.ts`)
Fin wrapper HTTP. **Injectable root**, `HttpClient` par constructeur, renvoie des
`Observable`, URL dérivée de `environment.apiUrl`. Aucune logique métier.

```ts
@Injectable({ providedIn: 'root' })
export class CategoryService {
  private apiUrl = `${environment.apiUrl}/admin/categories`;
  constructor(private http: HttpClient) {}
  getAll(): Observable<CategoryDto[]> { return this.http.get<CategoryDto[]>(this.apiUrl); }
  create(req: CreateCategoryRequest): Observable<string> { return this.http.post<string>(this.apiUrl, req); }
}
```

### 3.3 État (`infrastructure/states/<x>States.ts`)
Store **signal** pur. Signals privés exposés en `asReadonly()`, mutations via
méthodes `setX` / `reset`. **Aucune dépendance** (ni HTTP ni autre service).

```ts
@Injectable({ providedIn: 'root' })
export class CategoryStates {
  private _items = signal<CategoryDto[]>([]);
  readonly items = this._items.asReadonly();
  setItems(v: CategoryDto[]): void { this._items.set(v); }
  reset(): void { this._items.set([]); }
}
```

### 3.4 Use-case / application (`application/<verbe><X>.ts`)
**1 cas d'usage = 1 classe = 1 fichier**, nommé par le verbe (`LoadCategories`,
`CreateCategory`, `DeleteCategory`). Injectable root, dépendances via **`inject()`**.
Porte les signals **locaux d'UI** (`isLoading`, `isCreating`, le formulaire courant),
et une méthode **`handler(...)`** qui orchestre : flag loading → `subscribe` au repo
→ pousse dans les `States` → `toast` en cas d'erreur → (option) `callback` + rechargement.

```ts
@Injectable({ providedIn: 'root' })
export class LoadCategories {
  public isLoading = signal(false);
  private repo = inject(CategoryService);
  private states = inject(CategoryStates);
  private toasts = inject(ToastStates);

  handler(): void {
    this.isLoading.set(true);
    this.repo.getAll().subscribe({
      next: (items) => { this.isLoading.set(false); this.states.setItems(items); },
      error: () => {
        this.isLoading.set(false);
        this.toasts.addToast({ message: 'Erreur de chargement', messageType: ToastType.error, id: 0 });
      },
    });
  }
}
```

> **Séparation nette** : le **use-case** connaît repo + états + toasts ; l'**état**
> ne connaît rien ; le **repo** ne connaît que HTTP ; le **composant** ne connaît
> que les use-cases et les états (jamais le repo directement).

### 3.5 Composant (`components/<x>-list/`)
**Standalone**, dépendances par `inject()` (states + use-cases + modalStates),
`ngOnInit()` déclenche le `handler()`. Le template lit directement les **signals**
`states.items()` / `useCase.isLoading()` avec le **control flow** `@if` / `@for`
(`track`). Pas de `.scss` sauf besoin réel, tout en classes Tailwind/daisyUI.

```ts
@Component({
  selector: 'app-category-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './category-list.html',
})
export class CategoryList implements OnInit {
  states = inject(CategoryStates);
  load = inject(LoadCategories);
  ngOnInit(): void { this.load.handler(); }
}
```

---

## 4. Shared

### 4.1 Toasts, `toastStates.ts` + `toast` / `toast-list`
Store signal `IToast[]`, `addToast` (id auto-incrémenté), `removeToast`.
`IToast { message; messageType; id }`, `ToastType { success|warning|info|error }`.
Rendu global via `<app-toast-list>` posé dans le `layout`.

### 4.2 Modales, `modalStates.ts` (typé)
Registre de modales ouvertes via une **`Map<ModalId, data>`** avec un type
**`ModalData`** qui mappe chaque id de modale à la forme de ses données →
`open(id, data)`, `getData(id)`, `isOpen(id)`, `close(id)` **type-safe**.

```ts
export type ModalData = {
  'create-category': { name: string };
  'confirm-delete': { id: string; label: string };
};
```

### 4.3 Layout & navbar
`layout` = `min-h-screen` + `<app-navbar>` + `<main><router-outlet/><app-toast-list/></main>`.
`navbar` = liens de nav + utilisateur courant + `signOut()`.

---

## 5. Auth, guard, interceptor

- **Interceptor** `core/interceptors/auth.ts` (`HttpInterceptorFn`) : `inject()` le
  service de session, ajoute `Authorization: Bearer <token>` si présent, sinon
  laisse passer. Branché dans `app.config.ts` via `withInterceptors([auth])`.
- **Guard** `core/guards/auth.ts` (`CanActivateFn`) : vérifie la session ; sinon
  `router.navigate(['/login'])` + toast, `return false`.
- **Service de session** (`core/services/...`) : détient le token + l'état
  utilisateur (dans la référence : Supabase ; **ici → voir §7 décision auth**).

---

## 6. Routing (`app.routes.ts`)

- Tout en **lazy `loadComponent`** (`.then(m => m.X)`).
- Routes publiques (`login`, `auth/callback`) hors layout.
- Une route `''` **protégée par `canActivate: [auth]`** qui charge le `Layout`
  et déclare les pages en **`children`** (rendues dans le `<router-outlet>` du layout).
- `**` → redirect vers la home.

---

## 7. Adaptations EventHub (vs référence odtrCampagn)

| Sujet        | odtrCampagn            | **EventHub back-office**                                   |
|--------------|------------------------|-----------------------------------------------------------|
| API base     | `localhost:5998/api`   | **`localhost:5199/api`**                                   |
| Auth         | Supabase (Google OAuth)| **Microsoft Entra** (voir décision ci-dessous)            |
| Rôles        |,                      | **`organizer` / `moderator` / `admin`** (routes `/admin/*`) |
| Domaine      | campagnes/contacts     | activités, modération, utilisateurs, catégories, dashboard |

### Surface API admin déjà disponible (à consommer)
- `GET  /api/admin/dashboard/overview`, *admin, moderator*
- `GET  /api/admin/exports/registrations.csv`
- `PUT/POST /api/admin/activities/{id}` (+ `publish|cancel|feature`), `GET .../registrations`, *organizer, admin*
- `GET  /api/admin/reports`, `POST /api/admin/posts|comments/{id}/hide`, *moderator, admin*
- `PATCH/DELETE /api/admin/categories/{id}`, *admin*
- `PATCH/DELETE /api/admin/organizers/{id}`, *admin*
- `PATCH /api/admin/users/{id}`, `POST /api/admin/users/{id}/hearts`, *admin*
- `GET/PATCH /api/admin/settings/gamification`, *admin*
- `POST /api/admin/notifications/broadcast`, *admin*

> Côté API : CORS policy `Default` (origines via `Cors:AllowedOrigins`, sinon
> `AllowAnyOrigin` en l'absence de config), auth JWT Entra **ou** schéma dev
> `X-User-Id` / `X-User-Roles` selon la config (voir décision auth).

---

## 8. Règles d'or

1. Le **composant** n'appelle jamais le repo : il passe par un **use-case**.
2. L'**état** est passif (signals + setters), zéro dépendance.
3. Un **use-case = une intention** (`Load…`, `Create…`, `Delete…`), signals d'UI locaux.
4. **Un fichier par cas d'usage / état / modèle.**
5. Templates : `@if/@for` + lecture directe des signals ; UI en Tailwind/daisyUI.
6. Toute erreur réseau se traduit par un **toast** (jamais d'exception silencieuse).
