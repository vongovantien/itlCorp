import { Component, OnInit, AfterViewChecked } from '@angular/core';
declare var $: any;

@Component({
    selector: 'app-credit-and-debit-note-detail',
    templateUrl: './credit-and-debit-note-detail.component.html',
    styleUrls: ['./credit-and-debit-note-detail.component.scss']
})
export class CreditAndDebitNoteDetailComponent implements OnInit,AfterViewChecked {
    ngAfterViewChecked(): void {
        this.open = true;
    }

    constructor() { }

    ngOnInit() {
    }

    editCDNote() {
        $('#detail-credit-debit-note-modal').modal('hide');
        $('#edit-credit-debit-note-modal').modal('show');
    }

    open:boolean = false;

    close(){
        this.open = false;
    }
}
