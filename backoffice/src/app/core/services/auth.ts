import { computed, Injectable, signal } from '@angular/core';
import {
  AccountInfo,
  InteractionRequiredAuthError,
  PublicClientApplication,
} from '@azure/msal-browser';
import { environment } from '../../../environments/environment';

/**
 * Service d'authentification **Microsoft Entra** (SPA / MSAL, Option B).
 *
 * Enveloppe `@azure/msal-browser` dans une API à **signals** — même rôle que le
 * service `Supabase` de la référence : détient le compte courant, expose
 * `login()`, `logout()`, `getToken()`, et l'état `isAuthenticated`. L'interceptor
 * y prend le Bearer, le guard y lit la session.
 *
 * L'app obtient un **access token** pour la scope de l'API EventHub et l'envoie
 * en `Authorization: Bearer` ; le backend valide le jeton et provisionne l'user.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private msal: PublicClientApplication | null = null;

  private readonly _account = signal<AccountInfo | null>(null);
  readonly account = this._account.asReadonly();
  readonly isAuthenticated = computed(() => this._account() !== null);

  /** Vrai quand tenant + clientId SPA sont renseignés (app registration faite). */
  get isConfigured(): boolean {
    return !!environment.entra.tenantId && !!environment.entra.clientId;
  }

  get displayName(): string {
    return this._account()?.name ?? this._account()?.username ?? '';
  }

  get username(): string {
    return this._account()?.username ?? '';
  }

  private get loginScopes(): string[] {
    return ['openid', 'profile', 'email', 'offline_access', environment.entra.apiScope].filter(
      Boolean,
    );
  }

  /**
   * Initialise MSAL et restaure la session en cache (à appeler une fois au
   * démarrage, avant le routing — cf. `provideAppInitializer` dans app.config).
   */
  async initialize(): Promise<void> {
    if (!this.isConfigured) return;

    this.msal = new PublicClientApplication({
      auth: {
        clientId: environment.entra.clientId,
        authority: `https://login.microsoftonline.com/${environment.entra.tenantId}`,
        redirectUri: environment.entra.redirectUri,
      },
      cache: { cacheLocation: 'localStorage' },
    });

    await this.msal.initialize();
    const redirect = await this.msal.handleRedirectPromise();
    if (redirect?.account) this.msal.setActiveAccount(redirect.account);

    const account = this.msal.getActiveAccount() ?? this.msal.getAllAccounts()[0] ?? null;
    if (account) this.msal.setActiveAccount(account);
    this._account.set(account);
  }

  /**
   * Lance la connexion Microsoft par **redirection pleine page** (et non popup).
   * La page quitte vers Microsoft puis revient sur `redirectUri` ; au retour,
   * `initialize()` → `handleRedirectPromise()` récupère le compte. La redirection
   * évite les blocages de popup (Safari/ITP, extensions, bloqueurs) qui laissaient
   * la fenêtre sur `about:blank`. La promesse ne se résout pas ici : la page se
   * décharge pendant la navigation.
   */
  async login(): Promise<void> {
    if (!this.msal) {
      throw new Error("Microsoft Entra n'est pas configuré (clientId SPA manquant).");
    }
    await this.msal.loginRedirect({ scopes: this.loginScopes });
  }

  /** Déconnexion par redirection + purge de la session locale. */
  async logout(): Promise<void> {
    if (!this.msal) {
      this._account.set(null);
      return;
    }
    const account = this.msal.getActiveAccount() ?? undefined;
    await this.msal.logoutRedirect({ account });
  }

  /**
   * Renvoie un access token frais pour la scope de l'API (silencieux, avec repli
   * interactif si l'utilisateur doit ré-consentir). `null` si non connecté /
   * non configuré. Appelé par l'interceptor HTTP.
   */
  async getToken(): Promise<string | null> {
    if (!this.msal) return null;
    const account = this.msal.getActiveAccount();
    if (!account) return null;

    const scopes = [environment.entra.apiScope];
    try {
      const res = await this.msal.acquireTokenSilent({ scopes, account });
      return res.accessToken;
    } catch (err) {
      // Consentement/interaction requis : on repasse par une redirection (pas de
      // popup) pour rester cohérent avec le flux de connexion.
      if (err instanceof InteractionRequiredAuthError) {
        await this.msal.acquireTokenRedirect({ scopes, account });
      }
      return null;
    }
  }
}
