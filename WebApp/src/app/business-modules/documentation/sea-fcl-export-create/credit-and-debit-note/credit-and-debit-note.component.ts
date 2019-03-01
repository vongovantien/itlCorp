import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-credit-and-debit-note',
  templateUrl: './credit-and-debit-note.component.html',
  styleUrls: ['./credit-and-debit-note.component.scss']
})
export class CreditAndDebitNoteComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }
/**
   * ng2-select
   */
  public items: Array<string> = ['Option 1', 'Option 2', 'Option 3', 'Option 4',
  'Option 5', 'Option 6', 'Option 7', 'Option 8', 'Option 9', 'Option 10',];

  private _disabledV: string = '0';
  public disabled: boolean = false;


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
  }
}
