import { NgModule } from '@angular/core';

import { AccountReceivableTabComponent } from './account-receivable.component';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { SharedModule } from 'src/app/shared/shared.module';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { Routes, RouterModule } from '@angular/router';
import { AccountReceivableListTrialOfficialComponent } from '../components/list-trial-official/list-trial-official-account-receivable.component';
import { AccountReceivableListGuaranteedComponent } from '../components/list-guaranteed/list-guaranteed-account-receivable.component';
import { AccountReceivableListOtherComponent } from '../components/list-other/list-other-account-receivable.component';
import { AccountReceivableFormSearchComponent } from '../components/form-search/account-receivable/form-search-account-receivable.component';
import { AccountReceivableDetailComponent } from './detail/detail-account-receivable.component';
import { NgSelectModule } from '@ng-select/ng-select';


const routing: Routes = [
    {
        path: "",
        data: { name: '' },
        children: [
            {
                path: '',
                component: AccountReceivableTabComponent,
                data: { name: '' },
            },
            {
                path: ":key", component: AccountReceivableDetailComponent,
                data: { name: 'Detail' }
            },
        ]
    },

];

@NgModule({
    imports: [
        TabsModule.forRoot(),
        RouterModule.forChild(routing),
        SharedModule,
        NgSelectModule
    ],
    exports: [

    ],
    declarations: [
        AccountReceivableTabComponent,
        AccountReceivableListTrialOfficialComponent,
        AccountReceivableListGuaranteedComponent,
        AccountReceivableListOtherComponent,
        AccountReceivableFormSearchComponent,
        AccountReceivableDetailComponent
    ],
    providers: [],

})
export class AccountReceivableModule {

}