import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SharedModule } from 'src/app/shared/shared.module';
import { ModalModule } from 'ngx-bootstrap';
import { ShareBusinessAsignmentComponent } from 'src/app/business-modules/share-business/components/asignment/asignment.component';
import { ShareBussinessModule } from 'src/app/business-modules/share-business/share-bussines.module';
import { ShareBussinessCdNoteListComponent } from 'src/app/business-modules/share-business';


@NgModule({
    imports: [
        CommonModule,
        ModalModule.forRoot(),
        SharedModule,
        FormsModule,
        ShareBussinessModule
    ],
    exports: [],
    declarations: [
    ],
    providers: [],
    entryComponents: [
        ShareBussinessCdNoteListComponent
    ]
})
export class SeaLCLImportAsignmentModule {
    static rootComponent = ShareBussinessCdNoteListComponent;

}
