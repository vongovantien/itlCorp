import { Routes, RouterModule } from "@angular/router";
import { ModuleWithProviders } from "@angular/core";

const routes: Routes = [
    {
        path: '',
        redirectTo: 'agent',
        pathMatch: 'full'
    },
    {
        path: 'agent', loadChildren: () => import('./agent/commercial-agent.module').then(m => m.CommercialAgentModule),
        data: { name: 'Agent' }
    },
    {
        path: 'customer', loadChildren: () => import('./customer/commercial-customer.module').then(m => m.CommercialCustomerModule),
        data: { name: 'Customer' }
    },
];

export const routing: ModuleWithProviders = RouterModule.forChild(routes);
