import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { IDDefinitionComponent } from './id-definition/id-definition.component';
import { KPIComponent } from './kpi/kpi.component';
import { SupplierComponent } from './supplier/supplier.component';
import { LogViewerComponent } from './log-viewer/log-viewer.component';
import { MenuResolveGuard } from '@core';

const routes: Routes = [
    {
        path: '',
        redirectTo: 'ecus-connection',
        pathMatch: 'full'
    },
    {
        path: 'ecus-connection', loadChildren: () => import('./ecus-connection/ecus-connection.module').then(m => m.EcusConectionModule),
        data: { name: 'Ecus Connection', title: 'eFMS Ecus' },
        resolve: {
            checkMenu: MenuResolveGuard
        },
    },
    {
        path: 'tariff', loadChildren: () => import('./tariff/tariff.module').then(m => m.TariffModule),
        data: { name: 'Tariff', title: 'eFMS Tariff' },
        resolve: {
            checkMenu: MenuResolveGuard
        },
    },
    {
        path: 'exchange-rate', loadChildren: () => import('./exchange-rate/exchange-rate.module').then(m => m.ExchangeRateModule),
        data: {
            name: "Exchange Rate", title: 'eFMS Exchange Rate'
        },
        resolve: {
            checkMenu: MenuResolveGuard
        },
    },
    {
        path: 'file-management', loadChildren: () => import('./file-management/file-management.module').then(m => m.FilesManagementModule),
        data: { name: 'Files Management', title: 'eFMS File Management' },
        resolve: {
            checkMenu: MenuResolveGuard
        },
    },
    {
        path: 'other', loadChildren: () => import('./other/other.module').then(m => m.OtherModule),
        data: { name: 'Other', title: 'eFMS Other' },
        resolve: {
            checkMenu: MenuResolveGuard
        },
    },
    {
        path: 'unlock-request', loadChildren: () => import('./unlock-request/unlock-request.module').then(m => m.UnlockRequestModule),
        data: { name: 'Unlock Request', title: 'eFMS Unlock Request' },
        resolve: {
            checkMenu: MenuResolveGuard
        },
    },
    {
        path: 'log-viewer',
        component: LogViewerComponent,
        data: {
            name: "Log Viewer",
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
        path: 'link-fee', loadChildren: () => import('./link-fee/link-fee.module').then(m => m.LinkFeeModule),
        data: {
            name: "Link Fee",
            title: "eFMS Link Fee"
        },
        resolve: {
            checkMenu: MenuResolveGuard
        },
    }
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class ToolRoutingModule { }
