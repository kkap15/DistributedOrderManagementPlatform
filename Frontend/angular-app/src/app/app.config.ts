import { ApplicationConfig, inject, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import {AuthService, provideAuth0} from '@auth0/auth0-angular';
import {provideHttpClient, withInterceptors} from '@angular/common/http';
import { switchMap } from "rxjs";

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(
      withInterceptors([
        (req, next) => {
          const auth = inject(AuthService);
          return auth.getAccessTokenSilently().pipe(
            switchMap(token => {
              const cloned = req.clone({
                setHeaders: {
                  Authorization: `Bearer ${token}`
                }
              });
              return next(cloned);
            })
          );
        }
      ])
    ),
    provideRouter(routes),
    provideAuth0({
      domain: "dev-8tuqxc6l48vlcq83.us.auth0.com",
      clientId: "KbWhHC9eUlezlm5paMU7SU7xdSnIziKl",
      authorizationParams: {
        audience: 'http://microservices-api',
        redirect_uri: window.location.origin,
        scope: "openid profile email read:order"
      }
    })
  ]
};
