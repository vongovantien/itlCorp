import { Component } from '@angular/core';
import { AppList } from 'src/app/app.list';

@Component({
    selector: 'sea-fcl-import-cd-note',
    templateUrl: './sea-fcl-import-cd-note.component.html',
})
export class SeaFCLImportCDNoteComponent extends AppList {
    constructor() {
        super();
    }

    ngOnInit(): void { }
}
