import { NgModule } from '@angular/core';
import { SeaLCLExportBookingNoteComponent } from './sea-lcl-export-booking-note.component';
import { CommonModule } from '@angular/common';
import { SharedModule } from 'src/app/shared/shared.module';
import { FormsModule } from '@angular/forms';
import { Routes, RouterModule } from '@angular/router';
import { SeaLCLExportBookingNoteDetailComponent } from './detail/detail-booking-note-lcl-export.component';
import { SeaLCLExportBookingNoteCreateComponent } from './create/create-booking-note-lcl-export.component';

const routing: Routes = [
    {
        path: '', component: SeaLCLExportBookingNoteComponent, data: { name: '' }
    },
    {
        path: 'new', component: SeaLCLExportBookingNoteCreateComponent,
        data: { name: 'New', }
    },
    {
        path: ':bookingNoteId', component: SeaLCLExportBookingNoteDetailComponent,
        data: { name: 'Detail', }
    }
];

@NgModule({
    imports: [
        CommonModule,
        SharedModule,
        RouterModule.forChild(routing)
    ],
    exports: [],
    declarations: [
        SeaLCLExportBookingNoteComponent,
        SeaLCLExportBookingNoteCreateComponent,
        SeaLCLExportBookingNoteDetailComponent
    ],
    providers: [],
})
export class SeaLCLExportBookingNoteModule { }
