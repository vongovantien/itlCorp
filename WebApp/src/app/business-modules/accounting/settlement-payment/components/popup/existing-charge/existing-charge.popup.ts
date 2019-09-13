import { Component } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { SystemConstants } from 'src/constants/system.const';
import { takeUntil, catchError } from 'rxjs/operators';
import { DataService } from 'src/app/shared/services';
import { SystemRepo } from 'src/app/shared/repositories';

@Component({
    selector: 'existing-charge-popup',
    templateUrl: './existing-charge.popup.html'
})

export class SettlementExistingChargePopupComponent extends PopupBase {

    headers: CommonInterface.IHeaderTable[];

    configPartner: CommonInterface.IComboGirdConfig = {
        placeholder: 'Please select',
        displayFields: [],
        dataSource: [],
        selectedDisplayFields: [],
    };
    selectedPartner: Partial<CommonInterface.IComboGridData> = {};

  

    constructor(
        private _dataService: DataService,
        private _sysRepo: SystemRepo
    ) {
        super();
    }

    ngOnInit() {
        this.headers = [
            { title: 'Charge Name', field: 'chargeName', sortable: true },
            { title: 'Qty', field: 'chargeName', sortable: true },
            { title: 'Unit', field: 'chargeName', sortable: true },
            { title: 'Price', field: 'chargeName', sortable: true },
            { title: 'Currency', field: 'chargeName', sortable: true },
            { title: 'VAT', field: 'chargeName', sortable: true },
            { title: 'Amount', field: 'chargeName', sortable: true },
            { title: 'Settlement No', field: 'chargeName', sortable: true },
        ];

        this.getPartner();

    }

    getPartner() {
        this._dataService.getDataByKey(SystemConstants.CSTORAGE.PARTNER)
            .pipe(
                takeUntil(this.ngUnsubscribe),
                catchError(this.catchError)
            )
            .subscribe(
                (data: any) => {
                    if (!data) {
                        this._sysRepo.getListPartner(null, null, { inactive: false })
                            .pipe(catchError(this.catchError))
                            .subscribe(
                                (dataPartner: any) => {
                                    this.getPartnerData(dataPartner);
                                },
                            );
                    } else {
                        this.getPartnerData(data);
                    }
                }
            );
    }

    getPartnerData(data: any) {
        this.configPartner.dataSource = data;
        this.configPartner.displayFields = [
            { field: 'partnerNameEn', label: 'Name' },
            { field: 'partnerNameVn', label: 'Customer Name' },
        ];
        this.configPartner.selectedDisplayFields = ['partnerNameEn'];
    }

    onSelectDataFormInfo(data: any, type: string) {

    }

    reset() {
        console.log("reseting....");
    }

    searchCharge() {
        console.log("searching charge...");
    }
}
