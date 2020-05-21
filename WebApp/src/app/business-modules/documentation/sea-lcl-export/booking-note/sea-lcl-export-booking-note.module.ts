import { NgModule } from '@angular/core';
import { SeaLCLExportBookingNoteComponent } from './sea-lcl-export-booking-note.component';
import { CommonModule } from '@angular/common';
import { SharedModule } from 'src/app/shared/shared.module';
import { FormsModule } from '@angular/forms';
import { Routes, RouterModule } from '@angular/router';
import { SeaLCLExportBookingNoteDetailComponent } from './detail/detail-booking-note-lcl-export.component';
import { SeaLCLExportBookingNoteCreateComponent } from './create/create-booking-note-lcl-export.component';
import { SeaLCLExportFormBookingNoteComponent } from './components/form-booking-note-lcl-export.component';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { FroalaEditorModule } from 'angular-froala-wysiwyg';

const routing: Routes = [
    {
        path: '', component: SeaLCLExportBookingNoteComponent, data: { name: '' }
    },
    {
        path: 'new', component: SeaLCLExportBookingNoteCreateComponent,
        data: { name: 'Add New', }
    },
    {
        path: ':bookingNoteId', component: SeaLCLExportBookingNoteDetailComponent,
        data: { name: 'View/Edit Booking Note', }
    }
];

@NgModule({
    imports: [
        CommonModule,
        SharedModule,
        RouterModule.forChild(routing),
        NgxDaterangepickerMd,
        FroalaEditorModule.forRoot()
    ],
    exports: [],
    declarations: [
        SeaLCLExportBookingNoteComponent,
        SeaLCLExportBookingNoteCreateComponent,
        SeaLCLExportBookingNoteDetailComponent,
        SeaLCLExportFormBookingNoteComponent
    ],
    providers: [],
})
export class SeaLCLExportBookingNoteModule { }
