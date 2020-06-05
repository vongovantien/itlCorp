import { Component, OnInit, Input, ViewChild, EventEmitter, Output } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';

import { AccountingRepo } from '@repositories';
import { SystemConstants } from '@constants';
import { PartnerOfAcctManagementResult } from '@models';

import { ToastrService } from 'ngx-toastr';
import { AccountingManagementSelectPartnerPopupComponent } from '../select-partner/select-partner.popup';

@Component({
    selector: 'input-ref-no-popup',
    templateUrl: './input-ref-no.popup.html'
})

export class AccountingManagementInputRefNoPopupComponent extends PopupBase implements OnInit {
    @ViewChild(AccountingManagementSelectPartnerPopupComponent, { static: false }) selectPartnerPopup: AccountingManagementSelectPartnerPopupComponent;

    @Output() onSearchInputRef: EventEmitter<PartnerOfAcctManagementResult> = new EventEmitter<PartnerOfAcctManagementResult>();

    @Input() set type(t: string) {
        this._type = t;
        if (this._type !== 'invoice') {
            this.optionsType.length = 0;
            this.optionsType = [
                { title: 'Settlement', value: 'settlementCodes' },
                { title: 'SOA', value: 'soaNos' },
                { title: 'Job ID', value: 'jobNos' },
                { title: 'HBL', value: 'hbls' },
                { title: 'MBL', value: 'mbls' },
                { title: 'Debit Note', value: 'cdNotes' },
            ];
        }
    }

    get type() { return this._type; }

    private _type: string = 'invoice'; // * Default.

    optionsType: CommonInterface.ICommonTitleValue[] = [
        { title: 'Debit Note', value: 'cdNotes' },
        { title: 'SOA', value: 'soaNos' },
        { title: 'Job ID', value: 'jobNos' },
        { title: 'HBL', value: 'hbls' },
        { title: 'MBL', value: 'mbls' },
    ];

    selectedOpion: CommonInterface.ICommonTitleValue = this.optionsType[0];

    constructor(
        private _accountingRepo: AccountingRepo,
        private _toastService: ToastrService
    ) {
        super();
    }

    ngOnInit() { }

    searchRef() {
        const body: ISearchInputRef = {
            cdNotes: null,
            soaNos: null,
            jobNos: null,
            hbls: null,
            mbls: null,
            settlementCodes: null
        };

        switch (this.selectedOpion.value) {
            case 'cdNotes':
                body.cdNotes = this.keyword.trim().replace(SystemConstants.CPATTERN.LINE, ',').trim().split(',').map((item: string) => item.trim());
                break;
            case 'soaNos':
                body.soaNos = this.keyword.trim().replace(SystemConstants.CPATTERN.LINE, ',').trim().split(',').map((item: string) => item.trim());
                break;
            case 'jobNos':
                body.jobNos = this.keyword.trim().replace(SystemConstants.CPATTERN.LINE, ',').trim().split(',').map((item: string) => item.trim());
                break;
            case 'hbls':
                body.hbls = this.keyword.trim().replace(SystemConstants.CPATTERN.LINE, ',').trim().split(',').map((item: string) => item.trim());
                break;
            case 'mbls':
                body.mbls = this.keyword.trim().replace(SystemConstants.CPATTERN.LINE, ',').trim().split(',').map((item: string) => item.trim());
                break;
            case 'settlementCodes':
                body.settlementCodes = this.keyword.trim().replace(SystemConstants.CPATTERN.LINE, ',').trim().split(',').map((item: string) => item.trim());
                break;
            default:
                break;
        }
        this._accountingRepo.getChargeForVoucherByCriteria(body)
            .subscribe(
                (res: PartnerOfAcctManagementResult[]) => {
                    if (!!res && !!res.length) {
                        this.selectPartnerPopup.listPartners = res;
                        this.selectPartnerPopup.selectedPartner = null;

                        this.selectPartnerPopup.show();
                    } else {
                        this._toastService.warning("Not found data");
                    }
                }
            );
    }

    selectPartner(data: PartnerOfAcctManagementResult) {
        this.hide();
        this.onSearchInputRef.emit(data);
    }

}

interface ISearchInputRef {
    cdNotes: string[];
    soaNos: string[];
    jobNos: string[];
    hbls: string[];
    mbls: string[];
    settlementCodes: string[];
}
