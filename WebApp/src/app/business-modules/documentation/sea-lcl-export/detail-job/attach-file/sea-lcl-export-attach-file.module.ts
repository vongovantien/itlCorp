import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ShareBussinessAttachFileV2Component } from 'src/app/business-modules/share-business/components/files-attach-v2/files-attach-v2.component';
import { ShareBussinessModule } from 'src/app/business-modules/share-business/share-bussines.module';

@NgModule({
    imports: [
        CommonModule,
        ShareBussinessModule,
    ],
    exports: [],
    declarations: [
    ],
    providers: [],
    entryComponents: [
        ShareBussinessAttachFileV2Component
    ]
})
export class SeaLclExportAttachFilesModule {
    static rootComponent = ShareBussinessAttachFileV2Component;
}
