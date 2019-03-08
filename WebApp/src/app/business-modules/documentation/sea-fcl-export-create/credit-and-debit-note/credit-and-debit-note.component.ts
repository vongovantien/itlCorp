import { Component, OnInit } from '@angular/core';
declare var $: any;

@Component({
    selector: 'app-credit-and-debit-note',
    templateUrl: './credit-and-debit-note.component.html',
    styleUrls: ['./credit-and-debit-note.component.scss']
})
export class CreditAndDebitNoteComponent implements OnInit {

    constructor() { }

    ngOnInit() {
    }

    createCDNote() {
        $('#add-credit-debit-note-modal').modal('hide');
        $('#detail-credit-debit-note-modal').modal('show');
    }
    editCDNote() {
        $('#detail-credit-debit-note-modal').modal('hide');
        $('#edit-credit-debit-note-modal').modal('show');
    }
    viewDetailCDNote() {
        $('#edit-credit-debit-note-modal').modal('hide');
        $('#detail-credit-debit-note-modal').modal('show');
    }

}
