import { Routes } from '@angular/router';
import { Landing } from './landing/landing';
import {EditAlbums} from './edit-albums/edit-albums';
import {AdminViewPagePreview} from './admin-view-page-preview/admin-view-page-preview';

export const routes: Routes = [
  { path: '', component: Landing },
  { path: 'edit-albums', component: EditAlbums },
  { path: 'admin-view-page-preview/:albumId/:style', component: AdminViewPagePreview },
  { path: '**', redirectTo: '' },
];
