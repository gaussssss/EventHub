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

  /** Ouvre la fenêtre de connexion Microsoft (popup). */
  async login(): Promise<void> {
    if (!this.msal) {
      throw new Error("Microsoft Entra n'est pas configuré (clientId SPA manquant).");
    }
    const result = await this.msal.loginPopup({ scopes: this.loginScopes });
    this.msal.setActiveAccount(result.account);
    this._account.set(result.account);
  }

  /** Déconnexion (popup) + purge de la session locale. */
  async logout(): Promise<void> {
    if (!this.msal) {
      this._account.set(null);
      return;
    }
    const account = this.msal.getActiveAccount() ?? undefined;
    await this.msal.logoutPopup({ account });
    this._account.set(null);
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
      if (err instanceof InteractionRequiredAuthError) {
        const res = await this.msal.acquireTokenPopup({ scopes });
        this.msal.setActiveAccount(res.account);
        this._account.set(res.account);
        return res.accessToken;
      }
      return null;
    }
  }
}
