import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';

bootstrapApplication(App, appConfig)
  .then(() => {
    console.log('Void initialized successfully.');
  })
  .catch((err) => {
    console.error('The Void failed to initialize:', err);
  });
