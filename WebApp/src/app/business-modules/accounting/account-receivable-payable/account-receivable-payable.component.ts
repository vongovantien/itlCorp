import { Component, OnInit } from '@angular/core';

@Component({
    selector: 'app-account-receivable-payable',
    templateUrl: './account-receivable-payable.component.html',
})
export class AccountReceivablePayableComponent implements OnInit {
    selectedTab: string = '';
    constructor() { }

    ngOnInit() {
    }
    onSelectTabLocation(tabname) {
        this.selectedTab = tabname;
    }
}
