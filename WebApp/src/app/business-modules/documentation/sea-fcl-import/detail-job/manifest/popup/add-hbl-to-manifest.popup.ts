import { Component, OnInit, Input, EventEmitter, Output } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { CsTransactionDetail } from 'src/app/shared/models';
import { SortService } from 'src/app/shared/services';
@Component({
    selector: 'add-hbl-to-manifest-popup',
    templateUrl: 'add-hbl-to-manifest.popup.html'
})

export class AddHblToManifestComponent extends PopupBase {
    houseBills: any[] = [];
    checkAll = false;
    headers: CommonInterface.IHeaderTable[];
    @Output() onAdd: EventEmitter<any> = new EventEmitter<any>();
    constructor(
        private _sortService: SortService
    ) {

        super();

    }
    onCancel() {
        this.hide();
    }
    ngOnInit() {
        this.headers = [
            { title: 'HBL No', field: 'hwbNo', sortable: true, width: 100 },
            { title: 'No of Pieces', field: 'packageContainer', sortable: true },
            { title: 'G.W', field: 'grossWeight', sortable: true },
            { title: 'CBM', field: 'cbm', sortable: true },
            { title: 'Destination', field: 'podName', sortable: true },
            { title: 'Shipper', field: 'shipperName', sortable: true },
            { title: 'Consignee', field: 'consignee', sortable: true },
            { title: 'Description', field: 'desOfGoods', sortable: true },
            { title: 'Freight Charge', field: '', sortable: true },

        ];


    }


    sortHouseBills(sortData: CommonInterface.ISortData): void {
        if (!!sortData.sortField) {
            this.houseBills = this._sortService.sort(this.houseBills, sortData.sortField, sortData.order);
        }
    }

    removeAllChecked() {
        this.checkAll = false;
    }

    OnAdd() {
        const houseBillsSelected = this.houseBills.filter(item => item.isChecked === true);
        if (houseBillsSelected.length > 0) {
            this.onAdd.emit(houseBillsSelected);
            this.hide();
            console.log(houseBillsSelected);
        }

    }

    checkAllChange() {
        if (this.checkAll) {
            this.houseBills.forEach(x => {
                x.isChecked = true;
            });
        } else {
            this.houseBills.forEach(x => {
                x.isChecked = false;
            });
        }
    }


}