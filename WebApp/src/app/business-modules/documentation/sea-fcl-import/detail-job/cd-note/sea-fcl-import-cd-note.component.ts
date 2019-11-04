import { Component, ViewChild } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { CdNoteAddPopupComponent } from '../../components/popup/add-cd-note/add-cd-note.popup';

@Component({
    selector: 'sea-fcl-import-cd-note',
    templateUrl: './sea-fcl-import-cd-note.component.html',
})
export class SeaFCLImportCDNoteComponent extends AppList {
    @ViewChild(CdNoteAddPopupComponent, { static: false }) cdNoteAddPopupComponent: CdNoteAddPopupComponent;
    constructor() {
        super();
    }

    ngOnInit(): void { }

    openPopupAdd() {
        //this.cdNoteAddPopupComponent.action = 'create';
        //this.cdNoteAddPopupComponent.advanceNo = this.advanceNo;
        this.cdNoteAddPopupComponent.show({ backdrop: 'static' });
    }
}
