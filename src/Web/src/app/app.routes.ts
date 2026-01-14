import { Routes } from '@angular/router';
import { StoryFeedComponent } from './components/story-feed/story-feed.component';

export const routes: Routes = [
    { path: '', component: StoryFeedComponent },
    { path: '**', redirectTo: '' }
];
