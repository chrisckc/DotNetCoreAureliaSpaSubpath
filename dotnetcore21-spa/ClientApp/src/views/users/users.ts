import {lazy} from 'aurelia-framework';
import {HttpClient} from 'aurelia-fetch-client';

// polyfill fetch client conditionally
const fetchPolyfill = !self.fetch
  ? import('isomorphic-fetch' /* webpackChunkName: 'fetch' */)
  : Promise.resolve(self.fetch);

interface IUser {
  avatar_url: string;
  login: string;
  html_url: string;
}

export class Users {
  heading: string = 'Github Users';
  users: Array<IUser> = [];
  http: HttpClient;
  /**
   * ref element on the binding-context
   */
  image: HTMLImageElement;

  constructor(@lazy(HttpClient) private getHttpClient: () => HttpClient) {}

  async activate(): Promise<void> {
    // ensure fetch is polyfilled before we create the http client
    await fetchPolyfill;
    const http = this.http = this.getHttpClient();

    http.configure(config => {
      config
        .useStandardConfiguration()
        .withBaseUrl('https://api.github.com/');
    });

    const response = await http.fetch('users');
    this.users = await response.json();
  }
}
