import {Aurelia} from 'aurelia-framework';
import {Router, RouterConfiguration} from 'aurelia-router';
import {PLATFORM} from 'aurelia-pal';

export class App {
  router: Router;

  configureRouter(config: RouterConfiguration, router: Router) {
    config.title = 'Aurelia';
    config.map([
      { route: ['', 'welcome'], name: 'welcome',      moduleId: PLATFORM.moduleName('./views/welcome/welcome'),      nav: true, title: 'Welcome' },
      { route: 'users',         name: 'users',        moduleId: PLATFORM.moduleName('./views/users/users'),        nav: true, title: 'Github Users' },
      { route: 'image-test',  name: 'image-test', moduleId: PLATFORM.moduleName('./views/image-test/image-test'), nav: true, title: 'Image Test' },
      { route: 'sub-menu',  name: 'sub-menu', moduleId: PLATFORM.moduleName('./views/sub-menu/sub-menu'), nav: true, title: 'Sub Menu' }
    ]);

    this.router = router;
  }
}
