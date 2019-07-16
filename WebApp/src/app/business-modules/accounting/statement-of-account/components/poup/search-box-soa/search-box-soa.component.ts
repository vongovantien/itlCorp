import { Component } from '@angular/core';
import { AppPage } from 'src/app/app.base';

@Component({
    selector: 'soa-search-box',
    templateUrl: './search-box-soa.component.html',
    styleUrls: ['./search-box-soa.component.scss']
})
export class StatementOfAccountSearchComponent extends AppPage {
    items: any[];
    selectedRange: any;
    statusSOA: any[];
    currencyList: any[];
    maxDate: any;
    userList: any[];
    currentUser: any;

    constructor() {
        super();
    }

    ngOnInit(): void { }


    /**
    * ng2-select
    */
    private value: any = {};
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
