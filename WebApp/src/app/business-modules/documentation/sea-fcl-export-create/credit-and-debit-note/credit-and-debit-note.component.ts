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

    editCDNote() {
        $('#detail-credit-debit-note-modal').modal('hide');
        $('#edit-credit-debit-note-modal').modal('show');
    }
    viewDetailCDNote() {
        $('#edit-credit-debit-note-modal').modal('hide');
        $('#detail-credit-debit-note-modal').modal('show');
    }

    /**
      * ng2-select
      */
    public items: Array<string> = ['Credit note', 'Debit note', 'Invoice'];

    private value: any = {};
    private _disabledV: string = '0';
    public disabled: boolean = false;

    private get disabledV(): string {
        return this._disabledV;
    }

    private set disabledV(value: string) {
        this._disabledV = value;
        this.disabled = this._disabledV === '1';
    }

    public selected(value: any): void {
        console.log('Selected value is: ', value);
    }

    public removed(value: any): void {
        console.log('Removed value is: ', value);
    }

    public typed(value: any): void {
        console.log('New search input: ', value);
    }

    public refreshValue(value: any): void {
        this.value = value;
    }

}
