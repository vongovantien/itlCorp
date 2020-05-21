import { Component, OnInit } from '@angular/core';
import { AppList } from 'src/app/app.list';

@Component({
    selector: 'app-sea-lcl-export-booking-note',
    templateUrl: './sea-lcl-export-booking-note.component.html'
})

export class SeaLCLExportBookingNoteComponent extends AppList implements OnInit {
    constructor() {
        super();
    }

    ngOnInit() { }
}