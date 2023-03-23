import { NgModule } from '@angular/core';
import { ShareBussinessAttachFileV2Component } from 'src/app/business-modules/share-business/components/edoc/files-attach-v2/files-attach-v2.component';
import { ShareBussinessModule } from 'src/app/business-modules/share-business/share-bussines.module';

@NgModule({
    imports: [
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
export class AirExportAttachFilesModule {
    static rootComponent = ShareBussinessAttachFileV2Component;
}
