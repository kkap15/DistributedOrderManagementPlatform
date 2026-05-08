import {HttpInterceptorFn, provideHttpClient, withInterceptors} from '@angular/common/http';
import {authHttpInterceptorFn, AuthService} from '@auth0/auth0-angular';
import {inject} from '@angular/core';
import {switchMap} from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService)

  provideHttpClient(withInterceptors([authHttpInterceptorFn]))
  return auth.getAccessTokenSilently().pipe(
    switchMap(accessToken => {
      const cloned = req.clone({
        setHeaders: {
          Authorization: `Bearer ${accessToken}`
        }
      });
      return next(cloned);
    })
  );
};
