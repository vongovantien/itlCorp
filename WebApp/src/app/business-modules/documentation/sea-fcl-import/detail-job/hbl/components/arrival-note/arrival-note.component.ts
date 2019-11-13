import { Component } from '@angular/core';
import { AppList } from 'src/app/app.list';

@Component({
    selector: 'sea-fcl-import-hbl-arrival-note',
    templateUrl: './arrival-note.component.html'
})

export class SeaFClImportArrivalNoteComponent extends AppList {
    constructor() {
        super();
    }

    ngOnInit() { }
}

