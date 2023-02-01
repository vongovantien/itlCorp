import { NgModule } from '@angular/core';
import { ShareBussinessModule } from 'src/app/business-modules/share-business/share-bussines.module';
import { ShareBussinessFilesAttachComponent } from 'src/app/business-modules/share-business/components/files-attach/files-attach.component';
import { ShareBussinessAttachFileV2Component } from 'src/app/business-modules/share-business/components/files-attach-v2/files-attach-v2.component';

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
