import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AttachFileManagementComponent } from './attach-file-management.component';
import { ShareModulesModule } from 'src/app/business-modules/share-modules/share-modules.module';
import { Route, RouterModule } from '@angular/router';
import { NgSelectModule } from '@ng-select/ng-select';
import { CommonComponentModule } from 'src/app/shared/common/common.module';
import { SharedModule } from 'src/app/shared/shared.module';

const routing: Route[] = [
    {
        path: '', data: { name: "" },
        children: [
            {
                path: '', component: AttachFileManagementComponent
            }
        ]
    }
];


@NgModule({
    declarations: [
        AttachFileManagementComponent
    ],
    imports: [
        CommonModule,
        SharedModule,
        CommonComponentModule,
        RouterModule.forChild(routing),
        NgSelectModule,
    ],
    exports: [],
    providers: [],
})
export class AttachFileManagementModule { }