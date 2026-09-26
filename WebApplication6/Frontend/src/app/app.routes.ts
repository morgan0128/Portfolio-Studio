import { Routes } from '@angular/router';
import {DeveloperLanding} from './developer-landing/developer-landing';
import {EditAlbums} from './portfolio-administrator/edit-albums/edit-albums';
import {AdminViewPagePreview} from './portfolio-administrator/admin-view-page-preview/admin-view-page-preview';


export const routes: Routes = [
  { path: '', component: DeveloperLanding },
  { path: 'edit-albums', component: EditAlbums },
  { path: 'admin-view-page-preview/:albumId/:style', component: AdminViewPagePreview },
  { path: '**', redirectTo: '' },
];
