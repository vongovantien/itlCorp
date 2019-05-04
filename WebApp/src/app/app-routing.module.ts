import { NgModule } from '@angular/core';
import { Routes, RouterModule, PreloadingStrategy, PreloadAllModules } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { MasterPageComponent } from './master-page/master-page.component';
import { NotfoundPageComponent } from './notfound-page/notfound-page.component';
import { AuthGuardService } from 'src/services-base/auth-guard.service';
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
        canActivate:[AuthGuardService],
        component: MasterPageComponent,
        children: [
            {
                path:'dashboard',
                component:DashboardComponent
            },
            {
                path: 'system',
                loadChildren: './business-modules/system/system.module#SystemModule',
                data:{
                    name:"System",
                    path:"system",
                    level:1
                }
            },
            {
                path: 'catalogue',
                loadChildren: './business-modules/catalogue/catalogue.module#CatalogueModule',
                data:{
                    name:"Catalogue",
                    path:"catalogue",
                    level:1
                }
            },        
            {
                path:'accounting',
                loadChildren:'./business-modules/accounting/accounting.module#AccountingModule',
                data:{
                    name:"Accounting",
                    path:"accounting",
                    level:1
                }
            },
            {
                path:'documentation',
                loadChildren:'./business-modules/documentation/documentation.module#DocumentationModule',
                data:{
                    name:"Documentation",
                    path:"documentation",
                    level:1
                }
            },
            {
                path:'operation',
                loadChildren:'./business-modules/operation/operation.module#OperationModule',
                data:{
                    name:"Operation",
                    path:"operation",
                    level:1
                }
            },
            {
                path:'report',
                loadChildren:'./business-modules/report/report.module#ReportModule',
                data:{
                    name:"Report",
                    path:"report",
                    level:1
                }
            },
            {
                path:'support',
                loadChildren:'./business-modules/support/support.module#SupportModule',
                data:{
                    name:"Support",
                    path:"support",
                    level:1
                }
            },
            {
                path:'tool',
                loadChildren:'./business-modules/tool-setting/tool.module#ToolModule',
                data:{
                    name:"Tool",
                    path:"tool",
                    level:1
                }
            },
            {
                path:'designs-zone',
                loadChildren:'./design-modules/design-modules.module#DesignModulesModule',
                data:{
                    name:"Design Zone",
                    path:"design-zone",
                    level:1
                }
            }
        ]

    },
    /**
     * PAGE NOT FOUND 
     */
    {
        path:'page-not-found',
        component:NotfoundPageComponent
    },
    {
        path:'**',
        redirectTo:'page-not-found',
        pathMatch:'full'
    }
];

@NgModule({
    imports: [RouterModule.forRoot(routes,{useHash:true})],
    exports: [RouterModule]
})
export class AppRoutingModule { }
