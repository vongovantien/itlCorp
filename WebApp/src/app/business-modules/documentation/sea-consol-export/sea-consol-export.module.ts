import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { SeaConsolExportComponent } from './sea-consol-export.component';
import { CommonModule } from '@angular/common';
import { CommonComponentModule } from 'src/app/shared/common/common.module';
import { PipeModule } from 'src/app/shared/pipes/pipe.module';
import { DirectiveModule } from 'src/app/shared/directives/directive.module';
import { SharedModule } from 'src/app/shared/shared.module';
import { ShareBussinessModule } from '../../share-business/share-bussines.module';

const routing: Routes = [
    {
        path: '', data: { name: "", title: 'eFMS Sea Consol Export' }, children: [
            {
                path: '', component: SeaConsolExportComponent
            },
            // {
            //     path: 'new', component: AirExportCreateJobComponent,
            //     data: { name: "Create New Job" }
            // },
            // {
            //     path: ':jobId',
            //     data: { transactionType: CommonEnum.TransactionTypeEnum.AirExport, name: "Job Detail" },
            //     children: [
            //         {
            //             path: '', component: AirExportDetailJobComponent, data: { name: "" }
            //         },
            //         {
            //             path: 'hbl', loadChildren: () => import('./detail-job/hbl/air-export-hbl.module').then(m => m.AirExportHBLModule),
            //             data: {
            //                 name: "House Bill",
            //             },
            //         },
            //         {
            //             path: 'manifest', component: AirExportManifestComponent,
            //             data: { name: "Manifest", },
            //         },
            //         {
            //             path: 'mawb', component: AirExportMAWBFormComponent,
            //             data: { name: "MAWB Detail", },
            //         },

            //     ]
            // },

        ]
    },


];

@NgModule({
    imports: [
        RouterModule.forChild(routing),
        CommonComponentModule,
        PipeModule,
        ShareBussinessModule,
        CommonModule,
        // SharedModule,
    ],
    exports: [],
    declarations: [
        SeaConsolExportComponent
    ],
    providers: [],
})
export class SeaConsolExportModule { }
