import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OpsModuleCreditDebitNoteComponent } from './ops-module-credit-debit-note.component';
import { JobEditShareModule } from '../job-edit-share.module';
import { SharedModule } from 'src/app/shared/shared.module';


@NgModule({
    declarations: [
        OpsModuleCreditDebitNoteComponent,
    ],
    imports: [
        CommonModule,
        SharedModule,
        JobEditShareModule // ? Share with Job Edit
    ],
    exports: [],
    providers: [],
    entryComponents: [
        OpsModuleCreditDebitNoteComponent
    ]
})
export class CreditDebitNoteModule {
    static rootComponent = OpsModuleCreditDebitNoteComponent;
}