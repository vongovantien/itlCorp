import { Component, ViewChild, Input } from "@angular/core";
import { PopupBase } from "src/app/popup.base";
import { DocumentationRepo } from "src/app/shared/repositories";
import { CdNoteAddPopupComponent } from "../add-cd-note/add-cd-note.popup";
import { catchError } from "rxjs/operators";
import { SortService } from "src/app/shared/services";

@Component({
    selector: 'cd-note-detail-popup',
    templateUrl: './detail-cd-note.popup.html'
})
export class CdNoteDetailPopupComponent extends PopupBase {
    @ViewChild(CdNoteAddPopupComponent, { static: false }) cdNoteEditPopupComponent: CdNoteAddPopupComponent;
    jobId: string = null;
    cdNote: string = null;
    
    headers: CommonInterface.IHeaderTable[];
    
    CdNoteDetail: any = null;

    constructor(
        private _documentationRepo: DocumentationRepo,
        private _sortService: SortService,
    ) {
        super();
        this.requestSort = this.sortChargeCdNote;
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

    getDetailCdNote(jobId: string, cdNote: string) {
        console.log(jobId);
        console.log(cdNote);
        this._documentationRepo.getDetailsCDNote(jobId, cdNote)
            .pipe(
                catchError(this.catchError),
            ).subscribe(
                (dataCdNote: any) => {
                    console.log('CdNote detail')
                    console.log(dataCdNote);
                    this.CdNoteDetail = dataCdNote;

                    //Tính toán Amount Credit, Debit, Balance
                    //this.calculatorAmount();
                },
            );
    }

    closePopup() {
        this.hide();
    }

    deleteCdNote() {
        console.log('delete cd note')
    }

    openPopupEdit() {
        this.cdNoteEditPopupComponent.action = 'update';
        //this.cdNoteAddPopupComponent.advanceNo = this.advanceNo;
        this.cdNoteEditPopupComponent.show({ backdrop: 'static' });
    }

    previewCdNote() {
        console.log('preview cd note')
    }

    onRequestCdNoteChange(dataRequest: any) {
        console.log(dataRequest)
    }

    onUpdateCdNote(dataRequest: any) {
        console.log(dataRequest)
    }

    sortChargeCdNote(sort: string): void {
        if(this.CdNoteDetail){
            this.CdNoteDetail.listSurcharges = this._sortService.sort(this.CdNoteDetail.listSurcharges, sort, this.order);
        }
    }
}