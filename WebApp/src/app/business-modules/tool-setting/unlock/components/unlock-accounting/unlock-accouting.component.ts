import { Component, OnInit, ViewChild } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { AccountingRepo } from '@repositories';
import { catchError } from 'rxjs/operators';
import { IShipmentLockInfo } from '../unlock-shipment/unlock-shipment.component';
import { UnlockHistoryPopupComponent } from '../unlock-history/unlock-history.popup';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'unlock-accounting',
    templateUrl: './unlock-accouting.component.html'
})

export class UnlockAccountingComponent extends AppForm implements OnInit {
    @ViewChild(UnlockHistoryPopupComponent, { static: false }) confirmPopup: UnlockHistoryPopupComponent;

    types: CommonInterface.ICommonTitleValue[] = [
        { value: 1, title: 'Advance Payment' },
        { value: 2, title: 'Settlement Payment' },
    ];
    selectedType: CommonInterface.ICommonTitleValue = this.types[0];

    lockHistory: string[] = [];
    selectedDataAccountingToUnlock: any;

    constructor(
        private _accountingRepo: AccountingRepo,
        private _toastService: ToastrService
    ) {
        super();
    }

    ngOnInit() { }

    unlockPaymentRequest() {
        if (!this.keyword.trim()) {
            this._toastService.warning("Please input keyword");
            return;
        }
        const keywords = !!this.keyword ? this.keyword.trim().replace(/(?:\r\n|\r|\n|\\n|\\r)/g, ',').trim().split(',').map((item: any) => item.trim()) : [];
        if (this.selectedType.value === 1) {
            this._accountingRepo.getAdvancePaymentToUnlock(keywords)
                .pipe(catchError(this.catchError))
                .subscribe(
                    (res: IShipmentLockInfo) => {
                        if (!!res && !!res.logs.length) {
                            this.lockHistory = res.logs;
                            this.selectedDataAccountingToUnlock = res.lockedLogs;
                        }

                        this.confirmPopup.show();
                    }
                );
        } else {
            this._accountingRepo.getSettlementPaymentToUnlock(keywords)
                .pipe(catchError(this.catchError))
                .subscribe(
                    (res: IShipmentLockInfo) => {
                        if (!!res && !!res.logs.length) {
                            this.lockHistory = res.logs;
                            this.selectedDataAccountingToUnlock = res.lockedLogs;
                        }

                        this.confirmPopup.show();
                    }
                );
        }
    }

    onUnlockAccounting($event: boolean) {
        if ($event) {
            if (this.selectedType.value === 1) {
                this._accountingRepo.unlockAdvance(this.selectedDataAccountingToUnlock)
                    .pipe(catchError(this.catchError))
                    .subscribe(
                        (res: any) => {
                            if (res.success) {
                                this._toastService.success("Unlock Successfull");
                            } else {
                                this._toastService.error("Unlock failed, Please check again!");
                            }
                        }
                    )
            } else {
                this._accountingRepo.unlockSettlement(this.selectedDataAccountingToUnlock)
                    .pipe(catchError(this.catchError))
                    .subscribe(
                        (res: any) => {
                            if (res.success) {
                                this._toastService.success("Unlock Successfull");
                            } else {
                                this._toastService.error("Unlock failed, Please check again!");
                            }
                        }
                    );
            }
        }
    }
}
