import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserAttachFileManagementComponent } from './user-attach-file-management.component';
import { Route, RouterModule } from '@angular/router';
import { NgSelectModule } from '@ng-select/ng-select';
import { CommonComponentModule } from 'src/app/shared/common/common.module';
import { SharedModule } from 'src/app/shared/shared.module';

const routing: Route[] = [
    {
        path: '', data: { name: "" },
        children: [
            {
                path: '', component: UserAttachFileManagementComponent
            }
        ]
    }
];


@NgModule({
    declarations: [
        UserAttachFileManagementComponent
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
export class UserAttachFileManagementModule { }