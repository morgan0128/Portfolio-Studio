import { Routes } from '@angular/router';
import { Landing } from './landing/landing';
import {AdminViewPagePreview} from './components/page-level-components/admin-view-page-preview/admin-view-page-preview';
import {EditAlbums} from './components/page-level-components/edit-albums/edit-albums';

export const routes: Routes = [
  { path: '', component: Landing },
  { path: 'edit-albums', component: EditAlbums },
  { path: 'admin-view-page-preview/:albumId/:style', component: AdminViewPagePreview },
  { path: '**', redirectTo: '' },
];
