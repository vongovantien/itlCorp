import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UploadEdocComponent } from './upload-edoc.component';
import { ShareModulesModule } from '../../share-modules/share-modules.module';
import { Route, RouterModule } from '@angular/router';
import { NgSelectModule } from '@ng-select/ng-select';

const routing: Route[] = [
    {
        path: '', data: { name: "" },
        children: [
            {
                path: '', component: UploadEdocComponent
            }
        ]
    }
];

@NgModule({
    declarations: [
        UploadEdocComponent
    ],
    imports: [
        ShareModulesModule,
        RouterModule.forChild(routing),
        NgSelectModule,
        CommonModule
    ],
    exports: [],
    providers: [],
})
export class UploadEdocModule { }