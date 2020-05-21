import { Component, OnInit } from '@angular/core';
import { AppForm } from 'src/app/app.form';

@Component({
    selector: 'form-booking-note-lcl-export',
    templateUrl: './form-booking-note-lcl-export.component.html'
})

export class SeaLCLExportFormBookingNoteComponent extends AppForm implements OnInit {
    constructor() {
        super();
    }

    ngOnInit() { }
}