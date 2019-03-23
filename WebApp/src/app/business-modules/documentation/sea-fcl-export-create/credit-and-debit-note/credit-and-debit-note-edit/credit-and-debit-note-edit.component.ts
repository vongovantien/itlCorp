import { Component, OnInit } from '@angular/core';
declare var $: any;

@Component({
    selector: 'app-credit-and-debit-note-edit',
    templateUrl: './credit-and-debit-note-edit.component.html',
    styleUrls: ['./credit-and-debit-note-edit.component.scss']
})
export class CreditAndDebitNoteEditComponent implements OnInit {

    constructor() { }

    ngOnInit() {
    }

    viewDetailCDNote() {
        $('#edit-credit-debit-note-modal').modal('hide');
        $('#detail-credit-debit-note-modal').modal('show');
    }
}
