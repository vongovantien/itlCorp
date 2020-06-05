import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { PartnerOfAcctManagementResult } from '@models';
import { Store } from '@ngrx/store';
import { IAccountingManagementState } from '../../../store/reducers/accounting-management.reducer';
import { SelectPartner } from '../../../store';

@Component({
    selector: 'select-partner-popup',
    templateUrl: './select-partner.popup.html',
})
export class AccountingManagementSelectPartnerPopupComponent extends PopupBase implements OnInit {

    listPartners: PartnerOfAcctManagementResult[] = [];

    @Output() onSelect: EventEmitter<PartnerOfAcctManagementResult> = new EventEmitter<PartnerOfAcctManagementResult>();
    selectedPartner: PartnerOfAcctManagementResult;

    constructor(
        private _store: Store<IAccountingManagementState>
    ) {
        super();
    }

    ngOnInit(): void { }

    selectPartner(item: PartnerOfAcctManagementResult) {
        this.selectedPartner = item;

    }

    onSubmitSelectPartner() {
        if (!!this.selectedPartner) {
            // this._store.dispatch(SelectPartner(this.selectedPartner))
            this.hide();
            this.onSelect.emit(this.selectedPartner);
        }
    }
}
