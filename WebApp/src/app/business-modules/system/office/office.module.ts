import { NgModule } from '@angular/core';
import { OfficeComponent } from './office.component';
import { CommonModule } from '@angular/common';
import { SharedModule } from 'src/app/shared/shared.module';
import { SelectModule } from 'ng2-select';
import { FormsModule } from '@angular/forms';
import { NgProgressModule } from '@ngx-progressbar/core';
import { Routes, RouterModule } from '@angular/router';
import { OfficeAddNewComponent } from './addnew/office.addnew.component';
import { OfficeDetailsComponent } from './details/office-details.component';
import { OfficeFormSearchComponent } from './components/form-search-office/form-search-office.component';


const routing: Routes = [

    { path: '', component: OfficeComponent, data: { name: "Office", level: 2 } },
    { path: 'addnew', component: OfficeAddNewComponent, data: { name: "Addnew Office", level: 3 } },
    { path: ':id', component: OfficeDetailsComponent, data: { name: "Edit Office", level: 3 } },

];

@NgModule({
    imports: [
        CommonModule,
        SharedModule,
        SelectModule,
        FormsModule,
        NgProgressModule,
        RouterModule.forChild(routing)
    ],
    exports: [],
    declarations: [OfficeComponent, OfficeAddNewComponent, OfficeDetailsComponent, OfficeFormSearchComponent],
    providers: [],
})
export class OfficeModule { }
