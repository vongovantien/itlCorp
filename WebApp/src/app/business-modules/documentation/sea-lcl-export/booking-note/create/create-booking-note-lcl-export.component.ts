import { Component, OnInit } from '@angular/core';
import { AppForm } from 'src/app/app.form';

@Component({
    selector: 'app-sea-lcl-export-booking-note-create',
    templateUrl: './create-booking-note-lcl-export.component.html'
})

export class SeaLCLExportBookingNoteCreateComponent extends AppForm implements OnInit {
    constructor() {
        super();
    }

    ngOnInit() { }
}