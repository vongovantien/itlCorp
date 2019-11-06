import { Component, ViewChild } from "@angular/core";
import { PopupBase } from "src/app/popup.base";
import { DocumentationRepo } from "src/app/shared/repositories";
import { CdNoteAddPopupComponent } from "../add-cd-note/add-cd-note.popup";

@Component({
    selector: 'cd-note-detail-popup',
    templateUrl: './detail-cd-note.popup.html'
})
export class CdNoteDetailPopupComponent extends PopupBase {
    @ViewChild(CdNoteAddPopupComponent, { static: false }) cdNoteEditPopupComponent: CdNoteAddPopupComponent;
    
    headers: CommonInterface.IHeaderTable[];

    constructor(
        private _documentationRepo: DocumentationRepo,
    ) {
        super();
        
    }

    ngOnInit() {
        this.headers = [
            { title: 'HBL No', field: 'hwbno', sortable: true },
            { title: 'Code', field: 'chargeCode', sortable: true },
            { title: 'Charge Name', field: 'nameEn', sortable: true },
            { title: 'Quantity', field: 'quantity', sortable: true },
            { title: 'Unit', field: 'unit', sortable: true },
            { title: 'Unit Price', field: 'unitPrice', sortable: true },
            { title: 'Currency', field: 'currency', sortable: true },
            { title: 'VAT', field: 'vatrate', sortable: true },
            { title: "Credit Value (Local)", field: 'total', sortable: true },
            { title: "Debit Value (Local)", field: 'total', sortable: true },
            { title: 'Note', field: 'notes', sortable: true }
        ];
    }

    closePopup(){
        this.hide();
    }

    deleteCdNote(){
        console.log('delete cd note')
    }

    openPopupEdit() {
        //this.cdNoteAddPopupComponent.action = 'create';
        //this.cdNoteAddPopupComponent.advanceNo = this.advanceNo;
        this.cdNoteEditPopupComponent.show({ backdrop: 'static' });
    }

    previewCdNote(){
        console.log('preview cd note')
    }
}