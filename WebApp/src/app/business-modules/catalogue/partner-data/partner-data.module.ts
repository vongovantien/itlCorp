import { NgModule } from '@angular/core';

import { RouterModule, Routes } from '@angular/router';
import { NgSelectModule } from '@ng-select/ng-select';
import { NgProgressModule } from '@ngx-progressbar/core';
import { ModalModule } from 'ngx-bootstrap/modal';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { SharedModule } from 'src/app/shared/shared.module';
import { ShareModulesModule } from '../../share-modules/share-modules.module';
import { AddPartnerDataComponent } from './add/add-partner.component';
import { FormAddPartnerComponent } from './components/form-add-partner/form-add-partner.component';
import { PartnerOtherChargePopupComponent } from './components/other-charge/partner-other-charge.popup';
import { PartnerListComponent } from './components/partner-list/partner-list.component';
import { SalemanPopupComponent } from './components/saleman-popup.component';
import { UserCreatePopupComponent } from './components/user-create-popup/user-create-popup.component';
import { PartnerDetailComponent } from './detail/detail-partner.component';
import { PartnerDataImportComponent } from './import/partner-data-import.component';
import { PartnerComponent } from './partner.component';
import { ShareCommercialModule } from '../../commercial/share-commercial.module';

const routing: Routes = [
    {
        path: '', data: { name: "", title: 'eFMS Partner' },
        children: [
            {
                path: '', component: PartnerComponent

            },
            {
                path: 'import', component: PartnerDataImportComponent, data: { name: "Import", level: 3 }
            },
            {
                path: 'add', component: AddPartnerDataComponent, data: { name: "New", level: 3 }
            },
            {
                path: 'detail/:id', component: PartnerDetailComponent, data: { name: "Detail", level: 3 }
            },
            {
                path: 'add-sub/:id', component: PartnerDetailComponent, data: { name: "New PartnerSub", level: 3, action: true }
            },
        ]
    },

]
@NgModule({
    imports: [
        SharedModule,
        RouterModule.forChild(routing),
        NgProgressModule,
        NgSelectModule,
        NgxDaterangepickerMd,
        ModalModule.forRoot(),
        TabsModule.forRoot(),
        ShareModulesModule,
        ShareCommercialModule
    ],
    exports: [],
    declarations: [
        PartnerComponent,
        PartnerDataImportComponent,
        SalemanPopupComponent,
        FormAddPartnerComponent,
        AddPartnerDataComponent,
        PartnerDetailComponent,
        PartnerListComponent,
        PartnerOtherChargePopupComponent,
        UserCreatePopupComponent

    ],
    providers: [],
})
export class PartnerDataModule { }
