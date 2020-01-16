import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { EcusConnectionComponent } from './ecus-connection/ecus-connection.component';
import { IDDefinitionComponent } from './id-definition/id-definition.component';
import { KPIComponent } from './kpi/kpi.component';
import { SupplierComponent } from './supplier/supplier.component';
import { TariffComponent } from './tariff/tariff.component';
import { ExchangeRateComponent } from './exchange-rate/exchange-rate.component';
import { LogViewerComponent } from './log-viewer/log-viewer.component';

const routes: Routes = [
    {
        path: '',
        redirectTo: 'ecus-connection',
        pathMatch: 'full'
    },
    {
        path: 'ecus-connection',
        component: EcusConnectionComponent,
        data: {
            name: "Ecus Connection",
            level: 2
        }
    },
    {
        path: 'id-definition',
        component: IDDefinitionComponent,
        data: {
            name: "ID Definition",
            level: 2
        }
    },
    {
        path: 'kpi',
        component: KPIComponent,
        data: {
            name: "KPI",
            level: 2
        }
    },
    {
        path: 'supplier',
        component: SupplierComponent,
        data: {
            name: "Supplier",
            level: 2
        }
    },
    {
        path: 'tariff', loadChildren: () => import('./tariff/tariff.module').then(m => m.TariffModule),
        data: { name: 'Tariff' }
    },
    {
        path: 'exchange-rate',
        component: ExchangeRateComponent,
        data: {
            name: "Exchange Rate",
            level: 2
        }
    },
    {
        path: 'log-viewer',
        component: LogViewerComponent,
        data: {
            name: "Log Viewer",
            level: 2
        }
    },
    { path: 'unlock', loadChildren: () => import('./unlock/unlock.module').then(m => m.UnlockModule), data: { name: 'Unlock' } }
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class ToolRoutingModule { }
