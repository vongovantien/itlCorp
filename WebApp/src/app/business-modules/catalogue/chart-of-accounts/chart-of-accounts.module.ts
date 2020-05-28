import { NgModule } from '@angular/core';
import { SelectModule } from 'ng2-select';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { FroalaEditorModule } from 'angular-froala-wysiwyg';
import { NgxDaterangepickerMd, LocaleConfig } from 'ngx-daterangepicker-material';
import { RouterModule, Routes } from '@angular/router';
import { PaginationModule, ModalModule } from 'ngx-bootstrap';
import { SharedModule } from 'src/app/shared/shared.module';
import { CommonModule } from '@angular/common';
import { ChartOfAccountsComponent } from './chart-of-accounts.component';
import { FormSearchChartOfAccountsComponent } from './components/form-search-chart-of-accounts/form-search-chart-of-accounts.component';
import { FormCreateChartOfAccountsPopupComponent } from './components/form-create-chart-of-accounts/form-create-chart-of-accounts.popup';


const routing: Routes = [
    {
        path: '', component: ChartOfAccountsComponent
        , data: { name: '' }
    },
    {

    },
    {

    }
];


const config: LocaleConfig = {
    format: 'MM/DD/YYYY',
};

@NgModule({
    imports: [
        CommonModule,
        SharedModule,
        FormsModule,
        PaginationModule.forRoot(),
        ReactiveFormsModule,
        RouterModule.forChild(routing),
        NgxDaterangepickerMd.forRoot(config),
        FroalaEditorModule.forRoot(),
        ReactiveFormsModule,
        SelectModule,
        ModalModule.forRoot(),

    ],
    exports: [],
    declarations: [
        FormSearchChartOfAccountsComponent,
        FormCreateChartOfAccountsPopupComponent,
        ChartOfAccountsComponent
    ],
    providers: [],
})
export class ChartOfAccountsModule { }