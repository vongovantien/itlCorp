import { Component } from '@angular/core';

import { ShareBussinessBuyingChargeComponent } from '../buying-charge/buying-charge.component';
import { CatalogueRepo, DocumentationRepo } from 'src/app/shared/repositories';

import { Store } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';
import { SortService } from 'src/app/shared/services';

import * as fromStore from './../../store';
import { CommonEnum } from 'src/app/shared/enums/common.enum';
import { takeUntil, catchError } from 'rxjs/operators';
import { CsShipmentSurcharge } from 'src/app/shared/models';
import { SystemConstants } from 'src/constants/system.const';

@Component({
    selector: 'selling-charge',
    templateUrl: './selling-charge.component.html',
    styleUrls: ['./../buying-charge/buying-charge.component.scss']
})

export class ShareBussinessSellingChargeComponent extends ShareBussinessBuyingChargeComponent {

    constructor(
        protected _catalogueRepo: CatalogueRepo,
        protected _store: Store<fromStore.IShareBussinessState>,
        protected _documentRepo: DocumentationRepo,
        protected _toastService: ToastrService,
        protected _sortService: SortService,
    ) {
        super(_catalogueRepo, _store, _documentRepo, _toastService, _sortService);

        this._store.select(fromStore.getSellingSurChargeState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (buyings: CsShipmentSurcharge[]) => {
                    this.charges = buyings;
                    console.log("get selling charge from store", this.charges);
                }
            );
    }

    duplicate(index: number) {
        this.isSubmitted = false;
        const newCharge = this.charges[index];
        newCharge.id = SystemConstants.EMPTY_GUID;
        const newSurCharge: CsShipmentSurcharge = new CsShipmentSurcharge(newCharge);

        this._store.dispatch(new fromStore.AddSellingSurchargeAction(newSurCharge));

    }

    saveSellingSurCharge() {
        // * Update data 
        this.isSubmitted = true;
        if (!this.checkValidate()) {
            return;
        }

        if (!this.checkDuplicate()) {
            return;
        }

        this.updateSurchargeField(CommonEnum.SurchargeTypeEnum.SELLING_RATE);

        this._documentRepo.addShipmentSurcharges(this.charges)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);

                        // * Get Profit
                        this._store.dispatch(new fromStore.GetProfitAction(this.hbl.id));
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

}
