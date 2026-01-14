import { Routes } from '@angular/router';
import { StoryFeedComponent } from './components/story-feed/story-feed.component';
import { StoryReaderComponent } from './components/story-reader/story-reader.component';

export const routes: Routes = [
    { path: '', component: StoryFeedComponent },
    { path: 'story/:id', component: StoryReaderComponent },
    { path: '**', redirectTo: '' }
];
