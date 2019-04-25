import { Component, OnInit } from '@angular/core';

@Component({
    selector: 'app-sea-lcl-export-housebill-list',
    templateUrl: './sea-lcl-export-housebill-list.component.html',
    styleUrls: ['./sea-lcl-export-housebill-list.component.scss']
})
export class SeaLclExportHousebillListComponent implements OnInit {

    openCD: boolean = false;
    open_CD() {
        this.openCD = true;
    }
    constructor() { }

    ngOnInit() {
    }

    /**
  * ng2-select
  */
    public items: Array<string> = ['option 1', 'option 2', 'option 3', 'option 4', 'option 5', 'option 6', 'option 7'];

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
