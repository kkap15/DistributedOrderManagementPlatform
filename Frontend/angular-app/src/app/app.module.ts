import {AuthModule} from '@auth0/auth0-angular';

AuthModule.forRoot({
  domain: "dev-8tuqxc6l48vlcq83.us.auth0.com",
  clientId: 'KbWhHC9eUlezlm5paMU7SU7xdSnIziKl',
  authorizationParams: {
    redirect_uri: window.location.origin,
    audience: 'http://microservices-api',
    scope: 'openid profile email read:order'
  }
})
