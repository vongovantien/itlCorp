import { Routes, RouterModule } from "@angular/router";
import { NgModule } from "@angular/core";

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
    },
    {
        path: 'potential-customer',
        loadChildren: () => import('./potential-customer/commercial-potential-customer.module').then(m => m.CommercialPotentialCustomerModule),
        data: { name: 'Potential Customer' }
    }
];


@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class CommercialRoutingModule { }
