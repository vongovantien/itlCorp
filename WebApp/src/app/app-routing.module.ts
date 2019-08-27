import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { MasterPageComponent } from './master-page/master-page.component';
import { NotfoundPageComponent } from './notfound-page/notfound-page.component';
import { AuthGuardService } from 'src/app/shared/services/auth-guard.service';
import { DashboardComponent } from './dashboard/dashboard.component';

const routes: Routes = [
    {
        path: 'login',
        component: LoginComponent
    },

    {
        path: '',
        redirectTo: 'login',
        pathMatch: 'full'
    },

    /**
     * Lazy load business modules
     */

    {
        path: 'home',
        canActivate: [AuthGuardService],
        component: MasterPageComponent,
        children: [
            {
                path: 'dashboard',
                component: DashboardComponent
            },
            {
                path: 'system',
                loadChildren: () => import('./business-modules/system/system.module').then(m => m.SystemModule),
                data: {
                    name: "System",
                    path: "system",
                    level: 1
                }
            },
            {
                path: 'catalogue',
                loadChildren: () => import('./business-modules/catalogue/catalogue.module').then(m => m.CatalogueModule),
                data: {
                    name: "Catalogue",
                    path: "catalogue",
                    level: 1
                }
            },
            {
                path: 'accounting',
                loadChildren: () => import('./business-modules/accounting/accounting.module').then(m => m.AccountingModule),
                data: {
                    name: "Accounting",
                    path: "accounting",
                    level: 1
                }
            },
            {
                path: 'documentation',
                loadChildren: () => import('./business-modules/documentation/documentation.module').then(m => m.DocumentationModule),
                data: {
                    name: "Documentation",
                    path: "documentation",
                    level: 1
                }
            },
            {
                path: 'operation',
                loadChildren: () => import('./business-modules/operation/operation.module').then(m => m.OperationModule),
                data: {
                    name: "Operation",
                    path: "operation",
                    level: 1
                }
            },
            {
                path: 'report',
                loadChildren: () => import('./business-modules/report/report.module').then(m => m.ReportModule),
                data: {
                    name: "Report",
                    path: "report",
                    level: 1
                }
            },
            {
                path: 'support',
                loadChildren: () => import('./business-modules/support/support.module').then(m => m.SupportModule),
                data: {
                    name: "Support",
                    path: "support",
                    level: 1
                }
            },
            {
                path: 'tool',
                loadChildren: () => import('./business-modules/tool-setting/tool.module').then(m => m.ToolModule),
                data: {
                    name: "Tool",
                    path: "tool",
                    level: 1
                }
            },
            {
                path: 'designs-zone',
                loadChildren: () => import('./design-modules/design-modules.module').then(m => m.DesignModulesModule),
                data: {
                    name: "Design Zone",
                    path: "design-zone",
                    level: 1
                }
            }
        ]

    },
    /**
     * PAGE NOT FOUND 
     */
    {
        path: 'page-not-found',
        component: NotfoundPageComponent
    },
    {
        path: '**',
        redirectTo: 'page-not-found',
        pathMatch: 'full'
    }
];

@NgModule({
    imports: [RouterModule.forRoot(routes, { useHash: true })],
    exports: [RouterModule]
})
export class AppRoutingModule { }
