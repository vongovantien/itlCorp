import { NgModule } from '@angular/core';
import { Routes, RouterModule, PreloadingStrategy, PreloadAllModules } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { MasterPageComponent } from './master-page/master-page.component';
import { NotfoundPageComponent } from './notfound-page/notfound-page.component';

const routes: Routes = [
    {
        path: 'login',
        component: LoginComponent
    },
    {
        path: '',
        redirectTo: 'login', // redirect to 'login' page after login page implement 
        pathMatch: 'full'
    },    
  
    /**
     * Lazy load business modules
     */

    {
        path: 'home',
        component: MasterPageComponent,
        children: [
            {
                path: 'system',
                loadChildren: './business-modules/system/system.module#SystemModule'
            },
            {
                path: 'catalogue',
                loadChildren: './business-modules/catalogue/catalogue.module#CatalogueModule'
            },        
            {
                path:'accounting',
                loadChildren:'./business-modules/accounting/accounting.module#AccountingModule'
            },
            {
                path:'documentation',
                loadChildren:'./business-modules/documentation/documentation.module#DocumentationModule'
            },
            {
                path:'operation',
                loadChildren:'./business-modules/operation/operation.module#OperationModule'
            },
            {
                path:'report',
                loadChildren:'./business-modules/report/report.module#ReportModule'
            },
            {
                path:'support',
                loadChildren:'./business-modules/support/support.module#SupportModule'
            },
            {
                path:'tool',
                loadChildren:'./business-modules/tool-setting/tool.module#ToolModule'
            },
            {
                path:'designs-zone',
                loadChildren:'./design-modules/design-modules.module#DesignModulesModule'
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
