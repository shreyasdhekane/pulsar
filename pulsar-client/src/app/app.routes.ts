import { Routes } from '@angular/router';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { EndpointDetailComponent } from './components/endpoint-detail/endpoint-detail.component';

export const routes: Routes = [
  { path: '', component: DashboardComponent },
  { path: 'endpoint/:id', component: EndpointDetailComponent },
];
