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
        data: { name: 'Agent', type: 'Agent' }
    },
    {
        path: 'customer', loadChildren: () => import('./customer/commercial-customer.module').then(m => m.CommercialCustomerModule),
        data: { name: 'Customer', type: 'Customer' }
    },
    {
        path: 'incoterm', loadChildren: () => import('./incoterm/commercial-incoterm.module').then(m => m.CommercialIncotermModule),
        data: { name: 'Incoterm' }
    }
];

export const routing: ModuleWithProviders = RouterModule.forChild(routes);
