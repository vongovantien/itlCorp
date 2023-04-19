import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Routes, RouterModule } from '@angular/router';


import { ShareBussinessModule } from '../../share-business/share-bussines.module';
import { CommonComponentModule } from 'src/app/shared/common/common.module';
import { PipeModule } from 'src/app/shared/pipes/pipe.module';
import { SeaConsolImportComponent } from './sea-consol-import.component';
import { DirectiveModule } from 'src/app/shared/directives/directive.module';
import { SeaConsolImportCreateJobComponent } from './create-job/create-job-consol-import.component';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { CommonEnum } from '@enums';
import { SeaConsolImportDetailJobComponent } from './detail-job/detail-job-consol-import.component';
import { SeaConsolImportManifestComponent } from './manifest/sea-consol-import-manifest.component';
import { SeaConsolImportLazyLoadModule } from './sea-consol-import-lazy-load.module';
import { DeactivateGuardService } from '@core';
import { ShareSeaServiceModule } from '../share-sea/share-sea-service.module';
import { ChargeConstants } from '@constants';
import { ShareBusinessReAlertComponent } from '../../share-business/components/pre-alert/pre-alert.component';

const routing: Routes = [
    {
        path: '', component: SeaConsolImportComponent, data: {
            name: "", title: 'eFMS Sea Consol Import'
        },
    },
    {
        path: 'new', component: SeaConsolImportCreateJobComponent,
        data: { name: "Create New Job" }
    },
    {
        path: ':jobId',
        data: { transactionType: CommonEnum.TransactionTypeEnum.SeaConsolImport, name: "Job Detail" },
        children: [
            {
                path: '', component: SeaConsolImportDetailJobComponent, data: { name: "" }, canDeactivate: [DeactivateGuardService]
            },
            {
                path: 'hbl', loadChildren: () => import('./detail-job/hbl/sea-consol-import-hbl.module').then(m => m.SeaConsolImportHBLModule),
                data: {
                    name: "House Bill",
                },
            },
            {
                path: 'manifest', component: SeaConsolImportManifestComponent,
                data: { name: "Manifest", },
            },
            {
                path: 'prealert', component: ShareBusinessReAlertComponent,
                data: { name: "Pre Alert", level: 6, serviceId: ChargeConstants.SCE_CODE },
            }
        ]
    },
];

@NgModule({
    declarations: [
        SeaConsolImportComponent,
        SeaConsolImportCreateJobComponent,
        SeaConsolImportDetailJobComponent,
        SeaConsolImportManifestComponent,

    ],
    imports: [
        CommonModule,
        RouterModule.forChild(routing),
        FormsModule,
        ShareBussinessModule,
        CommonComponentModule,
        DirectiveModule,
        PipeModule,
        TabsModule.forRoot(),
        SeaConsolImportLazyLoadModule,
        ShareSeaServiceModule
    ],
    exports: [],
    providers: [],
})
export class SeaConsolImportModule { }
