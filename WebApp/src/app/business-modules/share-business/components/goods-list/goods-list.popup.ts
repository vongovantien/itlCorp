import { Component, OnInit, ChangeDetectorRef, ViewChild, Input } from '@angular/core';
import { Store } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';

import { ShareBussinessContainerListPopupComponent } from '../container-list/container-list.popup';
import { CatalogueRepo } from 'src/app/shared/repositories';
import { DataService, SortService } from 'src/app/shared/services';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';

import * as fromStore from '../../store';
import { ShareGoodsImportComponent } from '../goods-import/goods-import.component';

@Component({
    selector: 'goods-list-popup',
    templateUrl: './goods-list.popup.html'
})

export class ShareBussinessGoodsListPopupComponent extends ShareBussinessContainerListPopupComponent implements OnInit {

    @ViewChild(ShareGoodsImportComponent, { static: false }) goodsImportPopup: ShareGoodsImportComponent;
    @ViewChild('confirmCancel', { static: false }) confirmCancelPopup: ConfirmPopupComponent;

    constructor(
        protected _catalogueRepo: CatalogueRepo,
        protected _store: Store<fromStore.IContainerState>,
        protected cdRef: ChangeDetectorRef,
        protected _dataService: DataService,
        protected _sortService: SortService,
        protected _toastService: ToastrService
    ) {
        super(_catalogueRepo, _store, cdRef, _sortService, _toastService);
        this.requestSort = this.sortHBLGoods;
    }

    configHeader() {
        this.headers = [
            { title: 'Cont Type', field: 'containerTypeId', sortable: true },
            { title: 'Cont Q`Ty', field: 'quantity', sortable: true },
            { title: 'G.W', field: 'gw', sortable: true, required: true },
            { title: 'CBM', field: 'cbm', sortable: true, required: true },
            { title: 'Package Type', field: 'packageTypeId', sortable: true, required: true },
            { title: 'Package Q`Ty', field: 'packageQuantity', sortable: true, required: true },
            { title: 'Container No', field: 'containerNo', sortable: true, },
            { title: 'Seal No', field: 'sealNo', sortable: true, },
            { title: 'Mark No', field: 'markNo', sortable: true, },
            { title: 'Commodity', field: 'commodityId', sortable: true, },
            { title: 'Description', field: 'description', sortable: true, },
        ];
    }

    //  * Override check validate.
    checkValidateContainer() {
        let valid: boolean = true;
        for (const container of this.initContainers) {
            if (
                (!!container.containerNo || !!container.sealNo || !!container.markNo) && !container.quantity
                || !container.packageTypeId
                || !container.gw
                || !container.cbm
                || !container.packageQuantity

            ) {
                valid = false;
                break;
            }
        }

        return valid;
    }

    onSubmitCancel() {
        this.confirmCancelPopup.hide();
        this.onSaveContainerList();
    }

    onCancel() {
        this.isSubmitted = false;

        this.confirmCancelPopup.hide();
        this.closePopup();
    }

    showImportPopup() {
        this.goodsImportPopup.mblid = this.mblid;
        this.goodsImportPopup.hblid = this.hblid;
        this.goodsImportPopup.data = [];
        this.goodsImportPopup.getData();
        this.goodsImportPopup.show();
    }

    sortHBLGoods() {
        this.initContainers = this._sortService.sort(this.initContainers, this.sortField, this.order)
    }
}
