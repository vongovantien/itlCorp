import { Component, OnInit } from '@angular/core';
declare var $: any;

@Component({
    selector: 'app-sea-lcl-export-credit-and-debit-note-detail',
    templateUrl: './sea-lcl-export-credit-and-debit-note-detail.component.html',
    styleUrls: ['./sea-lcl-export-credit-and-debit-note-detail.component.scss']
})
export class SeaLclExportCreditAndDebitNoteDetailComponent implements OnInit {

    constructor() { }

    ngOnInit() {
    }

    editCDNote() {
        $('#detail-credit-debit-note-modal').modal('hide');
        $('#edit-credit-debit-note-modal').modal('show');
    }
}
